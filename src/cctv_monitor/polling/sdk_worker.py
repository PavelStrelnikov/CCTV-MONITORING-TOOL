"""Subprocess worker for SDK polling.

Runs in an isolated process so that SDK DLL crashes (access violations)
don't kill the main server. Communicates results via JSON on stdout.

Usage:
    python -m cctv_monitor.polling.sdk_worker \
        --host 1.2.3.4 --port 8000 --user admin --password secret \
        --lib-path /path/to/HCNetSDK.dll
"""

from __future__ import annotations

import argparse
import json
import sys
import time
from datetime import datetime, timezone


def _build_sdk_channel_candidates(
    ch_id: int,
    *,
    start_dchan: int,
    start_chan: int,
    ip_chan_num: int,
    chan_num: int,
) -> list[int]:
    """Build candidate SDK channel numbers for old/new NVR channel mapping.

    Some legacy firmwares expose snapshot channels as 1..N, while others
    require digital start offset (e.g. 33..). We try a short ordered list.
    """
    candidates: list[int] = []

    # 1) Raw channel as provided by API.
    candidates.append(ch_id)

    # 2) Digital-IP mapping (common NVR case): 1..N -> start_dchan..start_dchan+N-1.
    if ip_chan_num > 0:
        candidates.append(start_dchan + ch_id - start_chan)
        # If API already gave digital channel (33+), try converting back to slot index.
        if ch_id >= start_dchan:
            candidates.append(ch_id - start_dchan + start_chan)

    # 3) Analog mapping fallback (older DVR layouts).
    if chan_num > 0:
        candidates.append(start_chan + ch_id - 1)

    # 4) Legacy "digital starts at 33" heuristic used by some old models.
    if ch_id > 32:
        candidates.append(ch_id - 32)

    # Deduplicate and keep only positive channels.
    out: list[int] = []
    seen: set[int] = set()
    for c in candidates:
        if c > 0 and c not in seen:
            seen.add(c)
            out.append(c)
    return out


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--host", required=True)
    parser.add_argument("--port", type=int, required=True)
    parser.add_argument("--user", required=True)
    parser.add_argument("--password", required=True)
    parser.add_argument("--lib-path", required=True)
    parser.add_argument("--recordings-only", action="store_true",
                        help="Only check recordings, skip everything else")
    parser.add_argument("--channels", type=str, default="",
                        help="Comma-separated channel IDs for recordings-only mode")
    parser.add_argument("--snapshot", action="store_true",
                        help="Capture JPEG snapshot for a single channel")
    parser.add_argument("--channel", type=int, default=0,
                        help="Channel number for snapshot mode")
    parser.add_argument("--snapshot-batch", action="store_true",
                        help="Capture JPEG snapshots for multiple channels (JSON output)")
    parser.add_argument("--snapshot-channels", type=str, default="",
                        help="Comma-separated channel IDs for batch snapshot mode")
    args = parser.parse_args()

    # Batch snapshot mode: one login, multiple channels, JSON output
    if args.snapshot_batch:
        _snapshot_batch_mode(args)
        return

    # Snapshot mode: capture JPEG and write binary to stdout
    if args.snapshot:
        _snapshot_mode(args)
        return

    # Lightweight mode: only check recordings for given channels
    if args.recordings_only:
        _recordings_only_mode(args)
        return

    result: dict = {
        "success": False,
        "device_info": None,
        "cameras": [],
        "disks": [],
        "recordings": [],
        "error": None,
    }

    try:
        # TCP probe first — fail fast if port is not reachable
        import socket
        try:
            with socket.create_connection((args.host, args.port), timeout=5):
                pass
        except (OSError, TimeoutError):
            result["error"] = f"SDK port {args.host}:{args.port} is not reachable"
            json.dump(result, sys.stdout, default=str)
            sys.stdout.flush()
            return

        from cctv_monitor.drivers.hikvision.transports.sdk_bindings import HCNetSDKBinding

        binding = HCNetSDKBinding(lib_path=args.lib_path)
        binding.init()

        # Login
        user_id, login_info = binding.login(
            args.host, args.port, args.user, args.password,
        )

        start_t = time.monotonic()

        # Device info
        try:
            config = binding.get_device_config(user_id)
            result["device_info"] = config
        except Exception as exc:
            result["device_info_error"] = str(exc)

        # Device time check via ISAPI tunnel
        try:
            xml_data = binding.std_xml_config(
                user_id, "GET /ISAPI/System/time",
            )
            if xml_data:
                import re as _re
                lt_m = _re.search(r"<localTime>([^<]+)</localTime>", xml_data, _re.IGNORECASE)
                if lt_m:
                    device_time_str = lt_m.group(1)
                    clean = device_time_str.rstrip("Z")
                    if "+" in clean[10:]:
                        clean = clean[:clean.rindex("+")]
                    elif clean.count("-") > 2:
                        clean = clean[:clean.rindex("-")]
                    device_local = datetime.strptime(clean, "%Y-%m-%dT%H:%M:%S")
                    server_local = datetime.now()
                    drift = int((device_local - server_local).total_seconds())
                    tz_m = _re.search(r"<timeZone>([^<]+)</timeZone>", xml_data, _re.IGNORECASE)
                    mode_m = _re.search(r"<timeMode>([^<]+)</timeMode>", xml_data, _re.IGNORECASE)
                    result["time_check"] = {
                        "device_time": device_time_str,
                        "server_time": server_local.strftime("%Y-%m-%dT%H:%M:%S"),
                        "drift_seconds": drift,
                        "timezone": tz_m.group(1) if tz_m else None,
                        "time_mode": mode_m.group(1) if mode_m else None,
                    }
        except Exception:
            pass

        # Cameras via ISAPI tunnel
        ip_chan_num = login_info.get("ip_chan_num", 0)
        chan_num = login_info.get("chan_num", 0)
        start_dchan = login_info.get("start_dchan", 33)
        start_chan = login_info.get("start_chan", 1)

        cameras = _get_cameras(binding, user_id, ip_chan_num, chan_num, start_dchan, start_chan)
        result["cameras"] = cameras

        # Disks — basic info via binary SDK (safe)
        try:
            raw_disks = binding.get_hdd_config(user_id)
            disks = []
            for d in raw_disks:
                status_name = d.get("status_name", "unknown")
                disks.append({
                    "disk_id": d.get("disk_id", ""),
                    "capacity_bytes": d.get("capacity_mb", 0) * 1024 * 1024,
                    "free_bytes": d.get("free_space_mb", 0) * 1024 * 1024,
                    "status": "ok" if status_name == "normal" else status_name,
                    "health_status": "ok" if status_name == "normal" else status_name,
                    "online": status_name == "normal",
                    "temperature": None,
                    "power_on_hours": None,
                    "smart_status": None,
                })
            result["disks"] = disks
        except Exception as exc:
            result["disks_error"] = str(exc)

        result["response_time_ms"] = (time.monotonic() - start_t) * 1000
        result["success"] = True

        # ── Flush core results to stdout ─────────────────────────────
        # Everything below this point may crash the subprocess (ISAPI
        # tunnel / SDK commands that cause ACCESS_VIOLATION on some
        # device models).  The parent process will salvage core data
        # from the first JSON line even if the subprocess dies.
        core_json = json.dumps(result, default=str)
        sys.stdout.write(core_json)
        sys.stdout.flush()

        # ── Risky phase: SMART + Recording (after core flush) ────────
        extras: dict = {}

        # SMART data per disk via ISAPI tunnel (may crash)
        try:
            smart_data = _get_smart_data(binding, user_id, len(raw_disks))
            if smart_data:
                extras["smart"] = smart_data
        except Exception:
            pass

        # Recording check via SDK FindFile (safe binary API)
        try:
            recordings = _check_recordings(
                binding, user_id, cameras, ip_chan_num, start_dchan,
            )
            if recordings:
                extras["recordings"] = recordings
        except Exception:
            pass

        if extras:
            sys.stdout.write("\n" + json.dumps(extras, default=str))
            sys.stdout.flush()

        # Logout
        try:
            binding.logout(user_id)
        except Exception:
            pass

        # Cleanup SDK
        try:
            binding.cleanup()
        except Exception:
            pass

    except Exception as exc:
        result["error"] = str(exc)
        # Output JSON to stdout
        json.dump(result, sys.stdout, default=str)
        sys.stdout.flush()


def _snapshot_batch_mode(args) -> None:
    """Capture JPEG snapshots for multiple channels with a single SDK login.

    Writes JSON to stdout: {"results": {"<channel_id>": "<base64_jpeg>", ...}, "errors": {"<channel_id>": "<error>", ...}}
    """
    import base64
    import socket

    channel_ids = [int(ch.strip()) for ch in args.snapshot_channels.split(",") if ch.strip()]
    if not channel_ids:
        sys.stderr.write("No channels specified for batch snapshot\n")
        sys.exit(1)

    try:
        # TCP probe
        try:
            with socket.create_connection((args.host, args.port), timeout=5):
                pass
        except (OSError, TimeoutError):
            sys.stderr.write(f"SDK port {args.host}:{args.port} not reachable\n")
            sys.exit(1)

        from cctv_monitor.drivers.hikvision.transports.sdk_bindings import HCNetSDKBinding

        binding = HCNetSDKBinding(lib_path=args.lib_path)
        binding.init()

        user_id, login_info = binding.login(
            args.host, args.port, args.user, args.password,
        )

        start_dchan = login_info.get("start_dchan", 33)
        start_chan = login_info.get("start_chan", 1)

        results: dict[str, str] = {}
        errors: dict[str, str] = {}

        for ch_id in channel_ids:
            candidates = _build_sdk_channel_candidates(
                ch_id,
                start_dchan=start_dchan,
                start_chan=start_chan,
                ip_chan_num=login_info.get("ip_chan_num", 0),
                chan_num=login_info.get("chan_num", 0),
            )
            last_exc: Exception | None = None
            for sdk_channel in candidates:
                try:
                    jpeg_bytes = binding.capture_jpeg(user_id, sdk_channel)
                    results[str(ch_id)] = base64.b64encode(jpeg_bytes).decode("ascii")
                    last_exc = None
                    break
                except Exception as exc:
                    last_exc = exc
            if last_exc is not None:
                errors[str(ch_id)] = f"{last_exc}; tried={candidates}"

        try:
            binding.logout(user_id)
        except Exception:
            pass
        try:
            binding.cleanup()
        except Exception:
            pass

        output = json.dumps({"results": results, "errors": errors})
        sys.stdout.write(output)
        sys.stdout.flush()

    except Exception as exc:
        sys.stderr.write(f"Batch snapshot failed: {exc}\n")
        sys.exit(1)


def _snapshot_mode(args) -> None:
    """Capture a JPEG snapshot and write raw bytes to stdout."""
    import socket

    try:
        # TCP probe
        try:
            with socket.create_connection((args.host, args.port), timeout=5):
                pass
        except (OSError, TimeoutError):
            # Write error as JSON so the parent can detect failure
            sys.stderr.write(f"SDK port {args.host}:{args.port} not reachable\n")
            sys.exit(1)

        from cctv_monitor.drivers.hikvision.transports.sdk_bindings import HCNetSDKBinding

        binding = HCNetSDKBinding(lib_path=args.lib_path)
        binding.init()

        user_id, login_info = binding.login(
            args.host, args.port, args.user, args.password,
        )

        start_dchan = login_info.get("start_dchan", 33)
        start_chan = login_info.get("start_chan", 1)
        ip_chan_num = login_info.get("ip_chan_num", 0)
        chan_num = login_info.get("chan_num", 0)
        candidates = _build_sdk_channel_candidates(
            args.channel,
            start_dchan=start_dchan,
            start_chan=start_chan,
            ip_chan_num=ip_chan_num,
            chan_num=chan_num,
        )
        sys.stderr.write(
            f"snapshot: ch_in={args.channel} candidates={candidates} "
            f"start_dchan={start_dchan} start_chan={start_chan} "
            f"ip_chan={ip_chan_num} analog_chan={chan_num}\n"
        )

        jpeg_bytes = None
        last_exc: Exception | None = None
        for sdk_channel in candidates:
            try:
                jpeg_bytes = binding.capture_jpeg(user_id, sdk_channel)
                last_exc = None
                break
            except Exception as exc:
                last_exc = exc
        if jpeg_bytes is None:
            raise Exception(f"{last_exc}; tried={candidates}")

        # Write raw JPEG to stdout (binary mode)
        sys.stdout.buffer.write(jpeg_bytes)
        sys.stdout.buffer.flush()

        try:
            binding.logout(user_id)
        except Exception:
            pass
        try:
            binding.cleanup()
        except Exception:
            pass

    except Exception as exc:
        sys.stderr.write(f"Snapshot failed: {exc}\n")
        sys.exit(1)


def _recordings_only_mode(args) -> None:
    """Lightweight mode: login, check recordings for given channels, exit."""
    import socket
    from datetime import timedelta

    result: dict = {"recordings": []}

    try:
        # TCP probe
        try:
            with socket.create_connection((args.host, args.port), timeout=5):
                pass
        except (OSError, TimeoutError):
            json.dump(result, sys.stdout, default=str)
            sys.stdout.flush()
            return

        from cctv_monitor.drivers.hikvision.transports.sdk_bindings import HCNetSDKBinding

        binding = HCNetSDKBinding(lib_path=args.lib_path)
        binding.init()

        user_id, login_info = binding.login(
            args.host, args.port, args.user, args.password,
        )

        start_dchan = login_info.get("start_dchan", 33)
        start_chan = login_info.get("start_chan", 1)

        now = datetime.now()
        start = now - timedelta(hours=24)

        channel_ids = [ch.strip() for ch in args.channels.split(",") if ch.strip()]
        recordings = []
        for ch_id_str in channel_ids:
            if not ch_id_str.isdigit():
                continue
            ch_id = int(ch_id_str)

            # ISAPI channel_ids are 1-based (1,2,3...) — need to map to
            # SDK channel numbers. If ch_id < start_dchan, it's an ISAPI
            # sequential ID — remap to NVR digital channel.
            if ch_id < start_dchan:
                sdk_ch = start_dchan + ch_id - start_chan
            else:
                sdk_ch = ch_id

            try:
                rec = binding.find_recordings(user_id, sdk_ch, start, now)
                recordings.append({
                    "channel_id": ch_id_str,
                    "recording": "recording" if rec["has_recordings"] else "not_recording",
                })
            except Exception:
                pass

        result["recordings"] = recordings

        try:
            binding.logout(user_id)
        except Exception:
            pass
        try:
            binding.cleanup()
        except Exception:
            pass

    except Exception:
        pass

    json.dump(result, sys.stdout, default=str)
    sys.stdout.flush()


def _get_smart_data(binding, user_id: int, disk_count: int) -> dict[int, dict]:
    """Get SMART data per disk via ISAPI tunnel through SDK.

    Note: Binary SDK cmd 3262 (NET_DVR_GET_HDD_SMART_INFO) is NOT used here
    because it can cause ACCESS_VIOLATION crashes on some device models,
    killing the entire subprocess.
    """
    result: dict[int, dict] = {}
    for disk_id in range(1, disk_count + 1):
        try:
            xml_data = binding.std_xml_config(
                user_id,
                f"GET /ISAPI/ContentMgmt/Storage/hdd/{disk_id}/SMARTTest/status",
            )
            if xml_data:
                info = _parse_smart_xml(xml_data)
                if info.get("temperature") is not None or info.get("power_on_hours") is not None:
                    result[disk_id] = info
        except Exception:
            pass

    return result


def _parse_smart_xml(xml_data: str) -> dict:
    """Parse SMART XML response from ISAPI tunnel."""
    import re

    info: dict = {
        "smart_status": "ok",
        "power_on_hours": None,
        "temperature": None,
    }

    self_eval = re.search(r"<selfEvaluaingStatus>(\w+)</selfEvaluaingStatus>", xml_data, re.IGNORECASE)
    if self_eval and self_eval.group(1).lower() in ("error", "fail", "fault"):
        info["smart_status"] = "error"

    all_eval = re.search(r"<allEvaluaingStatus>(\w+)</allEvaluaingStatus>", xml_data, re.IGNORECASE)
    if all_eval and all_eval.group(1).lower() in ("fault", "error", "fail"):
        info["smart_status"] = "error"

    temp_m = re.search(r"<temprature>(\d+)</temprature>", xml_data, re.IGNORECASE)
    if not temp_m:
        temp_m = re.search(r"<temperature>(\d+)</temperature>", xml_data, re.IGNORECASE)
    if temp_m:
        info["temperature"] = int(temp_m.group(1))

    pod_m = re.search(r"<powerOnDay>(\d+)</powerOnDay>", xml_data, re.IGNORECASE)
    if pod_m:
        info["power_on_hours"] = int(pod_m.group(1)) * 24

    attrs = re.findall(
        r"<TestResult>.*?<attributeID>(\d+)</attributeID>.*?<rawValue>(\d+)</rawValue>.*?</TestResult>",
        xml_data, re.DOTALL | re.IGNORECASE,
    )
    if not attrs:
        attrs = re.findall(
            r"<SMARTAttribute>.*?<id>(\d+)</id>.*?<rawValue>(\d+)</rawValue>.*?</SMARTAttribute>",
            xml_data, re.DOTALL | re.IGNORECASE,
        )

    for attr_id_str, raw_str in attrs:
        attr_id = int(attr_id_str)
        raw_val = int(raw_str)
        if attr_id == 9:
            info["power_on_hours"] = raw_val
        elif attr_id == 194:
            if info["temperature"] is None:
                info["temperature"] = raw_val & 0xFF
        elif attr_id == 5:
            if raw_val > 100:
                info["smart_status"] = "error"
            elif raw_val > 0 and info["smart_status"] == "ok":
                info["smart_status"] = "warning"

    return info


def _get_cameras(
    binding, user_id: int,
    ip_chan_num: int, chan_num: int,
    start_dchan: int, start_chan: int,
) -> list[dict]:
    """Get camera channels — tries ISAPI tunnel, falls back to cmd 6126."""
    import re

    cameras: list[dict] = []

    # Try ISAPI tunnel first
    try:
        xml_data = binding.std_xml_config(
            user_id,
            "GET /ISAPI/ContentMgmt/InputProxy/channels/status",
        )
        if xml_data:
            blocks = re.findall(
                r"<InputProxyChannelStatus[^>]*>(.*?)</InputProxyChannelStatus>",
                xml_data, re.DOTALL | re.IGNORECASE,
            )
            configured: dict[int, dict] = {}
            for block in blocks:
                id_m = re.search(r"<id>(\d+)</id>", block)
                if not id_m:
                    continue
                ch_id = int(id_m.group(1))
                online_m = re.search(r"<online>([^<]+)</online>", block)
                is_online = online_m is not None and online_m.group(1).lower() == "true"
                ip_m = re.search(r"<ipAddress>([^<]+)</ipAddress>", block)
                ip_addr = ip_m.group(1) if ip_m else None
                name_m = re.search(r"<name>([^<]+)</name>", block)
                name = name_m.group(1) if name_m else None
                configured[ch_id] = {
                    "online": is_online,
                    "ip_address": ip_addr,
                    "channel_name": name,
                }

            for slot in range(1, ip_chan_num + 1):
                ch_id = start_dchan + slot - 1
                ch_data = configured.get(slot)
                cameras.append({
                    "channel_id": str(ch_id),
                    "channel_name": (ch_data["channel_name"] or f"Channel {ch_id}") if ch_data else f"Channel {ch_id}",
                    "online": ch_data["online"] if ch_data else None,
                    "ip_address": ch_data.get("ip_address") if ch_data else None,
                    "source": "isapi_via_sdk",
                })

            if cameras:
                return cameras
    except Exception:
        pass

    # Fallback: cmd 6126
    online_map: dict[str, bool | None] = {}
    if ip_chan_num > 0:
        try:
            digital_state = binding.get_digital_channel_state(
                user_id, start_dchan, ip_chan_num,
            )
            for ds in digital_state:
                online_map[ds["channel_id"]] = ds["online"]
        except Exception:
            pass

    for i in range(ip_chan_num):
        ch_id = start_dchan + i
        cameras.append({
            "channel_id": str(ch_id),
            "channel_name": f"Channel {ch_id}",
            "online": online_map.get(str(ch_id)),
            "ip_address": None,
            "source": "sdk_cmd6126",
        })

    # Analog channels
    for i in range(chan_num):
        ch_id = start_chan + i
        cameras.append({
            "channel_id": str(ch_id),
            "channel_name": f"Analog {ch_id}",
            "online": None,
            "ip_address": None,
            "source": "sdk_login_info",
        })

    return cameras


def _check_recordings(
    binding, user_id: int, cameras: list[dict],
    ip_chan_num: int, start_dchan: int,
) -> list[dict]:
    """Check recording files per channel via SDK FindFile API.

    Searches for any recording files in the last 24 hours per channel.
    Returns list of dicts with channel_id and recording status.
    """
    from datetime import timedelta

    now = datetime.now()
    start = now - timedelta(hours=24)
    recordings: list[dict] = []

    for cam in cameras:
        ch_id_str = cam.get("channel_id", "")
        if not ch_id_str.isdigit():
            continue
        ch_id = int(ch_id_str)

        try:
            result = binding.find_recordings(user_id, ch_id, start, now)
            recordings.append({
                "channel_id": ch_id_str,
                "recording": "recording" if result["has_recordings"] else "not_recording",
            })
        except Exception:
            pass

    return recordings


if __name__ == "__main__":
    main()
