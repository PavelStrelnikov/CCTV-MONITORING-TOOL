"""Background polling job — polls devices based on their individual poll_interval_seconds."""

from __future__ import annotations

import asyncio
import socket
import time
from datetime import datetime, timezone
from typing import TYPE_CHECKING, Callable

import structlog

from cctv_monitor.core.crypto import decrypt_value
from cctv_monitor.core.types import AlertStatus, DeviceTransport, DeviceVendor
from cctv_monitor.drivers.hikvision.errors import IsapiAuthError
from cctv_monitor.drivers.hikvision.transports.isapi import IsapiTransport
from cctv_monitor.models.alert import AlertEvent
from cctv_monitor.models.device import DeviceConfig
from cctv_monitor.models.device_health import DeviceHealthSummary
from cctv_monitor.alerts.engine import AlertEngine
from cctv_monitor.polling.sdk_subprocess import poll_device_via_sdk
from cctv_monitor.storage.repositories import AlertRepository, DeviceHealthLogRepository, DeviceRepository, SystemSettingsRepository
from cctv_monitor.storage.tables import AlertTable

if TYPE_CHECKING:
    from cctv_monitor.core.config import Settings
    from cctv_monitor.core.http_client import HttpClientManager
    from cctv_monitor.drivers.registry import DriverRegistry

logger = structlog.get_logger()


async def _check_tcp_port(host: str, port: int, timeout: float = 3.0) -> bool:
    """Check if a TCP port is open (non-blocking)."""
    loop = asyncio.get_event_loop()
    try:
        def _probe():
            with socket.create_connection((host, port), timeout=timeout):
                pass
        await loop.run_in_executor(None, _probe)
        return True
    except (OSError, TimeoutError):
        return False


async def poll_all_devices(
    session_factory,
    settings: Settings,
    registry: DriverRegistry,
    http_client: HttpClientManager,
    sdk_binding_getter: Callable,
) -> None:
    """Poll devices based on per-device or global default interval."""

    async with session_factory() as session:
        repo = DeviceRepository(session)
        settings_repo = SystemSettingsRepository(session)

        polling_enabled = await settings_repo.get("polling_enabled")
        if polling_enabled == "false":
            logger.debug("poll_all.polling_disabled")
            return

        devices = await repo.get_active_devices()
        default_interval = await settings_repo.get_int("default_poll_interval")

    if not devices:
        logger.debug("poll_all.no_active_devices")
        return

    now = datetime.now(timezone.utc)
    success_count = 0
    fail_count = 0
    skip_count = 0

    for device in devices:
        # Use device-specific interval, or fall back to global default
        interval = device.poll_interval_seconds or default_interval
        if not interval or interval <= 0:
            skip_count += 1
            continue

        # Check if enough time has passed since last poll
        if device.last_poll_at:
            elapsed = (now - device.last_poll_at).total_seconds()
            if elapsed < interval:
                skip_count += 1
                continue

        password = decrypt_value(device.password_encrypted, settings.ENCRYPTION_KEY)

        # Check which ports are open (advisory, for display)
        web_open = None
        sdk_open = None
        if device.web_port:
            web_open = await _check_tcp_port(device.host, device.web_port, timeout=5.0)
        if device.sdk_port:
            sdk_open = await _check_tcp_port(device.host, device.sdk_port, timeout=5.0)

        # Try ISAPI if web port open, then SDK as fallback (always try if configured)
        ok = False
        if device.web_port and web_open:
            ok = await _poll_device_isapi(
                device, password, session_factory, registry, http_client,
            )
        elif device.sdk_port and settings.HCNETSDK_LIB_PATH:
            ok = await _poll_device_sdk(
                device, password, session_factory, settings.HCNETSDK_LIB_PATH,
            )
        else:
            logger.warning("poll_all.skip_no_port", device_id=device.device_id,
                           web_port_open=web_open, sdk_port_open=sdk_open)
            fail_count += 1
            continue

        if ok:
            success_count += 1
        else:
            fail_count += 1

    logger.info(
        "poll_all.completed",
        success=success_count, failed=fail_count,
        skipped=skip_count, total=len(devices),
    )


async def _poll_device_isapi(
    device, password: str, session_factory,
    registry: DriverRegistry, http_client: HttpClientManager,
) -> bool:
    """Poll a single device via ISAPI. Returns True on success."""
    driver = None
    try:
        vendor = DeviceVendor(device.vendor)
        driver_cls = registry.get(vendor)
        transport = IsapiTransport(http_client)
        driver = driver_cls(transport)

        config = DeviceConfig(
            device_id=device.device_id, name=device.name, vendor=vendor,
            host=device.host, web_port=device.web_port, sdk_port=device.sdk_port,
            web_protocol=device.web_protocol,
            username=device.username, password=password,
            transport_mode=DeviceTransport(device.transport_mode),
            polling_policy_id=device.polling_policy_id, is_active=device.is_active,
        )

        await driver.connect(config, port=device.web_port)
        start_t = time.monotonic()

        device_info = None
        try:
            device_info = await driver.get_device_info()
        except Exception:
            pass

        cameras_raw = []
        try:
            cameras_raw = await driver.get_camera_statuses()
        except Exception:
            pass

        disks_raw = []
        try:
            disks_raw = await driver.get_disk_statuses()
        except Exception:
            pass

        # Recording status
        rec_map: dict[str, str] = {}
        try:
            rec_statuses = await driver.get_recording_statuses()
            for r in rec_statuses:
                rec_map[r.channel_id] = r.status.value
        except Exception:
            pass

        # Time check
        time_check = None
        try:
            time_data = await transport.get_device_time()
            if time_data.get("raw_xml"):
                from cctv_monitor.drivers.hikvision.mappers import HikvisionMapper
                parsed = HikvisionMapper.parse_device_time(time_data["raw_xml"])
                if parsed.get("local_time"):
                    device_time_str = parsed["local_time"]
                    clean = device_time_str.rstrip("Z")
                    if "+" in clean[10:]:
                        clean = clean[:clean.rindex("+")]
                    elif clean.count("-") > 2:
                        clean = clean[:clean.rindex("-")]
                    device_local = datetime.strptime(clean, "%Y-%m-%dT%H:%M:%S")
                    server_local = datetime.now()
                    drift = int((device_local - server_local).total_seconds())
                    time_check = {
                        "device_time": device_time_str,
                        "server_time": server_local.strftime("%Y-%m-%dT%H:%M:%S"),
                        "drift_seconds": drift,
                        "timezone": parsed.get("timezone"),
                        "time_mode": parsed.get("time_mode"),
                    }
        except Exception:
            pass

        response_time = (time.monotonic() - start_t) * 1000
        now = datetime.now(timezone.utc)

        # Check both ports
        web_port_open = True  # We just connected via ISAPI, so it's open
        sdk_port_open = None
        if device.sdk_port:
            sdk_port_open = await _check_tcp_port(device.host, device.sdk_port)

        online = sum(1 for c in cameras_raw if c.status.value == "online")
        disk_ok = all(d.status.value == "ok" for d in disks_raw) if disks_raw else True

        cameras_json = [
            {"channel_id": c.channel_id, "channel_name": c.channel_name,
             "status": c.status.value, "ip_address": c.ip_address,
             "recording": rec_map.get(c.channel_id),
             "checked_at": c.checked_at.isoformat()}
            for c in cameras_raw
        ]
        disks_json = [
            {"disk_id": d.disk_id, "status": d.status.value,
             "capacity_bytes": d.capacity_bytes, "free_bytes": d.free_bytes,
             "health_status": d.health_status,
             "checked_at": d.checked_at.isoformat(),
             "temperature": d.temperature, "power_on_hours": d.power_on_hours,
             "smart_status": d.smart_status}
            for d in disks_raw
        ]
        health_json: dict = {
            "reachable": True, "camera_count": len(cameras_raw),
            "online_cameras": online, "offline_cameras": len(cameras_raw) - online,
            "disk_ok": disk_ok, "response_time_ms": response_time,
            "checked_at": now.isoformat(),
            "web_port_open": web_port_open, "sdk_port_open": sdk_port_open,
        }
        if time_check:
            health_json["time_check"] = time_check

        async with session_factory() as session:
            dev_repo = DeviceRepository(session)
            update_fields: dict = {
                "last_poll_at": now,
                "last_health_json": {
                    "cameras": cameras_json, "disks": disks_json, "health": health_json,
                },
            }
            if device_info:
                update_fields["model"] = device_info.model
                update_fields["serial_number"] = device_info.serial_number
                update_fields["firmware_version"] = device_info.firmware_version

            await dev_repo.update(device.device_id, **update_fields)

            health_log_repo = DeviceHealthLogRepository(session)
            await health_log_repo.insert(
                device_id=device.device_id, reachable=True,
                camera_count=len(cameras_raw), online_cameras=online,
                offline_cameras=len(cameras_raw) - online,
                disk_ok=disk_ok, response_time_ms=response_time,
            )
            await session.commit()

        ignored = {str(ch) for ch in (device.ignored_channels or [])}
        await _evaluate_alerts(device.device_id, health_json, cameras_json, session_factory, ignored_channels=ignored)

        logger.info(
            "poll_all.device_ok", device_id=device.device_id,
            cameras=len(cameras_raw), online=online,
            response_ms=round(response_time, 1), transport="isapi",
        )
        return True

    except IsapiAuthError:
        logger.error("poll_all.auth_failed", device_id=device.device_id,
                      msg="Authentication failed — wrong credentials, skipping")
        await _save_failure(device.device_id, session_factory)
        return False
    except Exception as exc:
        logger.error("poll_all.device_error", device_id=device.device_id, error=str(exc))
        await _save_failure(device.device_id, session_factory)
        return False

    finally:
        if driver is not None:
            try:
                await driver.disconnect()
            except Exception:
                pass


async def _poll_device_sdk(
    device, password: str, session_factory, lib_path: str,
) -> bool:
    """Poll a single device via SDK subprocess (crash-safe). Returns True on success."""
    result = await poll_device_via_sdk(
        host=device.host, port=device.sdk_port,
        username=device.username, password=password,
        lib_path=lib_path,
    )

    if not result["success"]:
        logger.error(
            "poll_all.device_error", device_id=device.device_id,
            error=result.get("error", "SDK subprocess failed"), transport="sdk_subprocess",
        )
        await _save_failure(device.device_id, session_factory)
        return False

    now = datetime.now(timezone.utc)
    cameras = result.get("cameras", [])
    disks = result.get("disks", [])
    recordings = result.get("recordings", [])
    response_time = result.get("response_time_ms", 0)

    # Build recording map: channel_id -> recording status
    rec_map: dict[str, str] = {}
    for r in recordings:
        rec_map[r.get("channel_id", "")] = r.get("recording", "unknown")

    online = sum(1 for c in cameras if c.get("online") is True)
    disk_ok = all(d.get("status") in ("normal", "ok") for d in disks) if disks else True

    cameras_json = [
        {"channel_id": c.get("channel_id", ""), "channel_name": c.get("channel_name", ""),
         "status": "online" if c.get("online") else ("offline" if c.get("online") is False else "unknown"),
         "ip_address": c.get("ip_address"),
         "recording": rec_map.get(c.get("channel_id", "")),
         "checked_at": now.isoformat()}
        for c in cameras
    ]
    disks_json = [
        {"disk_id": str(d.get("disk_id", "")), "status": d.get("status", "unknown"),
         "capacity_bytes": d.get("capacity_bytes", 0), "free_bytes": d.get("free_bytes", 0),
         "health_status": d.get("health_status", "unknown"),
         "checked_at": now.isoformat(),
         "temperature": d.get("temperature"), "power_on_hours": d.get("power_on_hours"),
         "smart_status": d.get("smart_status")}
        for d in disks
    ]
    # SDK worked so sdk_port is open; check web_port if configured
    web_port_open = None
    if device.web_port:
        web_port_open = await _check_tcp_port(device.host, device.web_port)

    health_json: dict = {
        "reachable": True, "camera_count": len(cameras),
        "online_cameras": online, "offline_cameras": len(cameras) - online,
        "disk_ok": disk_ok, "response_time_ms": response_time,
        "checked_at": now.isoformat(),
        "web_port_open": web_port_open, "sdk_port_open": True,
    }
    # SDK subprocess returns time_check from ISAPI tunnel
    if result.get("time_check"):
        health_json["time_check"] = result["time_check"]

    async with session_factory() as session:
        dev_repo = DeviceRepository(session)
        update_fields: dict = {
            "last_poll_at": now,
            "last_health_json": {
                "cameras": cameras_json, "disks": disks_json, "health": health_json,
            },
        }
        device_info = result.get("device_info")
        if device_info and isinstance(device_info, dict):
            if device_info.get("model"):
                update_fields["model"] = device_info["model"]
            if device_info.get("serial_number"):
                update_fields["serial_number"] = device_info["serial_number"]
            if device_info.get("firmware_version"):
                update_fields["firmware_version"] = device_info["firmware_version"]

        await dev_repo.update(device.device_id, **update_fields)

        health_log_repo = DeviceHealthLogRepository(session)
        await health_log_repo.insert(
            device_id=device.device_id, reachable=True,
            camera_count=len(cameras), online_cameras=online,
            offline_cameras=len(cameras) - online,
            disk_ok=disk_ok, response_time_ms=response_time,
        )
        await session.commit()

    ignored = {str(ch) for ch in (device.ignored_channels or [])}
    await _evaluate_alerts(device.device_id, health_json, cameras_json, session_factory, ignored_channels=ignored)

    logger.info(
        "poll_all.device_ok", device_id=device.device_id,
        cameras=len(cameras), online=online,
        response_ms=round(response_time, 1), transport="sdk_subprocess",
    )
    return True


_alert_engine = AlertEngine()


async def _evaluate_alerts(
    device_id: str,
    health_json: dict,
    cameras_json: list,
    session_factory,
    ignored_channels: set[str] | None = None,
) -> None:
    """Run alert engine against poll results and persist new/resolved alerts."""
    try:
        h = health_json
        ignored = ignored_channels or set()
        # Filter ignored channels for recording/camera stats
        monitored = [c for c in cameras_json if str(c.get("channel_id", "")) not in ignored] if ignored else cameras_json
        # Count recording stats
        rec_total = sum(1 for c in monitored if c.get("recording"))
        rec_ok = sum(1 for c in monitored if c.get("recording") == "recording")

        if ignored and h.get("reachable", False):
            mon_online = sum(1 for c in monitored if c.get("status", "").lower() == "online")
            cam_count = len(monitored)
            cam_online = mon_online
            cam_offline = cam_count - mon_online
        else:
            cam_count = h.get("camera_count", 0)
            cam_online = h.get("online_cameras", 0)
            cam_offline = h.get("offline_cameras", 0)

        summary = DeviceHealthSummary(
            device_id=device_id,
            reachable=h.get("reachable", False),
            camera_count=cam_count,
            online_cameras=cam_online,
            offline_cameras=cam_offline,
            disk_ok=h.get("disk_ok", True),
            recording_ok=(rec_ok == rec_total) if rec_total > 0 else True,
            response_time_ms=h.get("response_time_ms", 0),
            checked_at=datetime.now(timezone.utc),
        )

        async with session_factory() as session:
            alert_repo = AlertRepository(session)
            active_rows = await alert_repo.get_active_alerts(device_id)

            # Convert DB rows to domain objects for engine
            active_events = [
                AlertEvent(
                    device_id=r.device_id,
                    alert_type=r.alert_type,
                    severity=r.severity,
                    message=r.message,
                    source=r.source,
                    status=r.status,
                    created_at=r.created_at,
                    id=r.id,
                    channel_id=r.channel_id,
                    resolved_at=r.resolved_at,
                )
                for r in active_rows
            ]

            new_alerts, resolved = _alert_engine.evaluate(summary, active_events)

            for alert in new_alerts:
                await alert_repo.create_alert(AlertTable(
                    device_id=alert.device_id,
                    alert_type=alert.alert_type,
                    severity=alert.severity,
                    message=alert.message,
                    source=alert.source,
                    status=AlertStatus.ACTIVE,
                    created_at=alert.created_at,
                ))

            for alert in resolved:
                if alert.id is not None:
                    await alert_repo.resolve_alert(alert.id)

            await session.commit()

            if new_alerts or resolved:
                logger.info(
                    "alerts.evaluated",
                    device_id=device_id,
                    new=len(new_alerts),
                    resolved=len(resolved),
                )
    except Exception:
        logger.error("alerts.evaluate_error", device_id=device_id, exc_info=True)


async def _save_failure(device_id: str, session_factory) -> None:
    """Save unreachable state for a failed device poll."""
    try:
        now = datetime.now(timezone.utc)
        async with session_factory() as session:
            dev_repo = DeviceRepository(session)
            await dev_repo.update(
                device_id, last_poll_at=now,
                last_health_json={
                    "cameras": [], "disks": [],
                    "health": {
                        "reachable": False, "camera_count": 0,
                        "online_cameras": 0, "offline_cameras": 0,
                        "disk_ok": False, "response_time_ms": 0,
                        "checked_at": now.isoformat(),
                    },
                },
            )
            health_log_repo = DeviceHealthLogRepository(session)
            await health_log_repo.insert(
                device_id=device_id, reachable=False,
                camera_count=0, online_cameras=0,
                offline_cameras=0, disk_ok=False, response_time_ms=0,
            )
            await session.commit()

        failure_health = {
            "reachable": False, "camera_count": 0,
            "online_cameras": 0, "offline_cameras": 0,
            "disk_ok": False, "response_time_ms": 0,
        }
        await _evaluate_alerts(device_id, failure_health, [], session_factory)
    except Exception:
        logger.error("poll_all.save_failure_state_error", device_id=device_id)
