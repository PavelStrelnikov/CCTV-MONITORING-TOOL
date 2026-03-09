"""Run SDK poll in an isolated subprocess.

If the SDK DLL crashes (access violation / segfault), only the subprocess
dies — the main server process is unaffected.
"""

from __future__ import annotations

import asyncio
import json
import sys
from typing import Any

import structlog

logger = structlog.get_logger()

SDK_POLL_TIMEOUT = 60  # seconds — total subprocess lifetime limit


def _merge_extras(data: dict, extras: dict) -> None:
    """Merge SMART and recording extras into core result dict."""
    # Recordings
    if "recordings" in extras:
        data["recordings"] = extras["recordings"]

    # SMART data — enrich existing disk entries
    smart = extras.get("smart")
    if smart and "disks" in data:
        for disk in data["disks"]:
            disk_id = disk.get("disk_id", "")
            # smart_data keys are int (1-based index), try matching
            for key, sdata in smart.items():
                key_str = str(key)
                if key_str == disk_id or key_str == str(data["disks"].index(disk) + 1):
                    disk["temperature"] = sdata.get("temperature")
                    disk["power_on_hours"] = sdata.get("power_on_hours")
                    disk["smart_status"] = sdata.get("smart_status")
                    if sdata.get("smart_status"):
                        disk["health_status"] = sdata["smart_status"]
                    break


async def poll_device_via_sdk(
    host: str,
    port: int,
    username: str,
    password: str,
    lib_path: str,
) -> dict[str, Any]:
    """Spawn a subprocess to poll a device via SDK.

    Returns dict with keys: success, device_info, cameras, disks,
    response_time_ms, error.
    """
    cmd = [
        sys.executable, "-m", "cctv_monitor.polling.sdk_worker",
        "--host", host,
        "--port", str(port),
        "--user", username,
        "--password", password,
        "--lib-path", lib_path,
    ]

    try:
        proc = await asyncio.create_subprocess_exec(
            *cmd,
            stdout=asyncio.subprocess.PIPE,
            stderr=asyncio.subprocess.PIPE,
        )

        try:
            stdout, stderr = await asyncio.wait_for(
                proc.communicate(), timeout=SDK_POLL_TIMEOUT,
            )
        except asyncio.TimeoutError:
            proc.kill()
            await proc.wait()
            logger.error(
                "sdk_subprocess.timeout",
                host=host, port=port, timeout=SDK_POLL_TIMEOUT,
            )
            return {
                "success": False,
                "error": f"SDK subprocess timed out after {SDK_POLL_TIMEOUT}s",
                "device_info": None,
                "cameras": [],
                "disks": [],
            }

        # Always log stderr for debug
        if stderr:
            stderr_text = stderr.decode(errors="replace").strip()
            if stderr_text:
                logger.info("sdk_subprocess.stderr", host=host, port=port, stderr=stderr_text[:2000])

        # Parse JSON output — worker writes core data first, then optional
        # recording data on a second line (recording cmd may crash subprocess).
        raw = stdout.decode(errors="replace").strip()

        if proc.returncode != 0:
            # Even on crash, try to salvage partial output (core data written
            # before the recording command that may have caused the crash).
            if raw:
                try:
                    lines = raw.split("\n", 1)
                    data = json.loads(lines[0])
                    if data.get("success"):
                        logger.warning(
                            "sdk_subprocess.partial_crash",
                            host=host, port=port,
                            returncode=proc.returncode,
                        )
                        # Merge extras if second line exists
                        if len(lines) > 1:
                            try:
                                extras = json.loads(lines[1])
                                _merge_extras(data, extras)
                            except (json.JSONDecodeError, ValueError):
                                pass
                        return data
                except (json.JSONDecodeError, ValueError):
                    pass

            stderr_text = stderr.decode(errors="replace").strip()
            logger.error(
                "sdk_subprocess.crashed",
                host=host, port=port,
                returncode=proc.returncode,
                stderr=stderr_text[:500],
            )
            return {
                "success": False,
                "error": f"SDK subprocess crashed (exit code {proc.returncode})",
                "device_info": None,
                "cameras": [],
                "disks": [],
            }

        if not raw:
            return {
                "success": False,
                "error": "SDK subprocess produced no output",
                "device_info": None,
                "cameras": [],
                "disks": [],
            }

        lines = raw.split("\n", 1)
        data = json.loads(lines[0])
        # Merge extras (SMART + recording) from second line if present
        if len(lines) > 1:
            try:
                extras = json.loads(lines[1])
                _merge_extras(data, extras)
            except (json.JSONDecodeError, ValueError):
                pass
        return data

    except Exception as exc:
        logger.error("sdk_subprocess.spawn_error", error=str(exc))
        return {
            "success": False,
            "error": f"Failed to spawn SDK subprocess: {exc}",
            "device_info": None,
            "cameras": [],
            "disks": [],
        }


async def poll_device_via_sdk_recordings(
    host: str,
    port: int,
    username: str,
    password: str,
    lib_path: str,
    channels: list[str],
) -> list[dict]:
    """Spawn a lightweight subprocess to check recordings via SDK FindFile.

    Used by ISAPI-polled devices to get reliable recording status
    (ISAPI POST /ContentMgmt/search is unreliable on many models).

    Returns list of dicts: [{"channel_id": "33", "recording": "recording"}, ...]
    """
    cmd = [
        sys.executable, "-m", "cctv_monitor.polling.sdk_worker",
        "--host", host,
        "--port", str(port),
        "--user", username,
        "--password", password,
        "--lib-path", lib_path,
        "--recordings-only",
        "--channels", ",".join(channels),
    ]

    try:
        proc = await asyncio.create_subprocess_exec(
            *cmd,
            stdout=asyncio.subprocess.PIPE,
            stderr=asyncio.subprocess.PIPE,
        )

        try:
            stdout, stderr = await asyncio.wait_for(
                proc.communicate(), timeout=SDK_POLL_TIMEOUT,
            )
        except asyncio.TimeoutError:
            proc.kill()
            await proc.wait()
            logger.error(
                "sdk_subprocess.recordings_timeout",
                host=host, port=port, timeout=SDK_POLL_TIMEOUT,
            )
            return []

        if stderr:
            stderr_text = stderr.decode(errors="replace").strip()
            if stderr_text:
                logger.info("sdk_subprocess.recordings_stderr", host=host, stderr=stderr_text[:1000])

        raw = stdout.decode(errors="replace").strip()
        if not raw:
            return []

        data = json.loads(raw)
        return data.get("recordings", [])

    except Exception as exc:
        logger.error("sdk_subprocess.recordings_spawn_error", error=str(exc))
        return []
