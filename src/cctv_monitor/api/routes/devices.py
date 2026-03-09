import asyncio
import io
import logging
import socket

logger = logging.getLogger(__name__)

from fastapi import APIRouter, Depends, HTTPException
from fastapi.responses import StreamingResponse
from sqlalchemy import delete as sa_delete
from sqlalchemy.exc import IntegrityError
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.api.deps import get_session, get_settings, get_driver_registry, get_http_client
from cctv_monitor.api.schemas import (
    DeviceCreate, DeviceUpdate, DeviceOut, DeviceDetailOut, PollResultOut,
    HealthSummaryOut, AlertOut, CameraChannelOut, DiskOut,
)
from cctv_monitor.core.config import Settings
from cctv_monitor.core.crypto import encrypt_value, decrypt_value
from cctv_monitor.core.types import DeviceTransport, DeviceVendor
from cctv_monitor.drivers.hikvision.transports.isapi import IsapiTransport
from cctv_monitor.drivers.registry import DriverRegistry
from cctv_monitor.core.http_client import HttpClientManager
from cctv_monitor.models.device import DeviceConfig
from cctv_monitor.storage.repositories import DeviceRepository, AlertRepository, DeviceTagRepository, DeviceHealthLogRepository
from cctv_monitor.storage.tables import AlertTable, DeviceTable

router = APIRouter(tags=["devices"])


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


def _device_out(d: DeviceTable, tags: list[dict] | None = None) -> DeviceOut:
    cached = d.last_health_json or {}
    health_data = cached.get("health")
    health = HealthSummaryOut(**health_data) if health_data else None
    return DeviceOut(
        device_id=d.device_id, name=d.name, vendor=d.vendor,
        host=d.host, web_port=d.web_port, sdk_port=d.sdk_port,
        transport_mode=d.transport_mode, is_active=d.is_active,
        last_health=health,
        model=d.model, serial_number=d.serial_number,
        firmware_version=d.firmware_version, last_poll_at=d.last_poll_at,
        poll_interval_seconds=d.poll_interval_seconds,
        tags=tags or [],
        ignored_channels=d.ignored_channels or [],
    )


@router.get("/devices", response_model=list[DeviceOut])
async def list_devices(
    tag: str | None = None,
    search: str | None = None,
    session: AsyncSession = Depends(get_session),
):
    repo = DeviceRepository(session)
    devices = await repo.list_all()
    tag_repo = DeviceTagRepository(session)

    result = []
    for d in devices:
        tags = await tag_repo.get_tags_with_colors(d.device_id)
        tag_names = [t["name"] for t in tags]
        if tag and tag not in tag_names:
            continue
        if search and search.lower() not in d.name.lower() and search not in d.host:
            continue
        result.append(_device_out(d, tags))
    return result


@router.post("/devices", response_model=DeviceOut, status_code=201)
async def create_device(
    body: DeviceCreate,
    session: AsyncSession = Depends(get_session),
    settings: Settings = Depends(get_settings),
):
    encrypted_password = encrypt_value(body.password, settings.ENCRYPTION_KEY)
    device = DeviceTable(
        device_id=body.device_id, name=body.name, vendor=body.vendor,
        host=body.host, web_port=body.web_port, sdk_port=body.sdk_port,
        username=body.username, password_encrypted=encrypted_password,
        transport_mode=body.transport_mode, is_active=True,
        poll_interval_seconds=body.poll_interval_seconds,
    )
    repo = DeviceRepository(session)
    try:
        await repo.create(device)
        await session.commit()
    except IntegrityError as exc:
        await session.rollback()
        err = str(exc.orig) if exc.orig else str(exc)
        if "polling_policies" in err or "foreign key" in err.lower():
            raise HTTPException(status_code=500, detail="Polling policy 'standard' not found. Run seed first.")
        raise HTTPException(status_code=409, detail=f"Device '{body.device_id}' already exists")
    return _device_out(device)


@router.delete("/devices/{device_id}", status_code=204)
async def delete_device(device_id: str, session: AsyncSession = Depends(get_session)):
    repo = DeviceRepository(session)
    # Delete related alerts first to avoid FK constraint violation
    await session.execute(sa_delete(AlertTable).where(AlertTable.device_id == device_id))
    deleted = await repo.delete(device_id)
    if not deleted:
        raise HTTPException(status_code=404, detail="Device not found")
    await session.commit()


@router.patch("/devices/{device_id}", response_model=DeviceOut)
async def update_device(
    device_id: str,
    body: DeviceUpdate,
    session: AsyncSession = Depends(get_session),
    settings: Settings = Depends(get_settings),
):
    repo = DeviceRepository(session)
    fields = body.model_dump(exclude_unset=True)
    if "password" in fields:
        fields["password_encrypted"] = encrypt_value(fields.pop("password"), settings.ENCRYPTION_KEY)
    if not fields:
        raise HTTPException(status_code=400, detail="No fields to update")
    device = await repo.update(device_id, **fields)
    if device is None:
        raise HTTPException(status_code=404, detail="Device not found")
    await session.commit()
    return _device_out(device)


@router.get("/devices/{device_id}", response_model=DeviceDetailOut)
async def get_device_detail(device_id: str, session: AsyncSession = Depends(get_session)):
    repo = DeviceRepository(session)
    device = await repo.get_by_id(device_id)
    if device is None:
        raise HTTPException(status_code=404, detail="Device not found")

    tag_repo = DeviceTagRepository(session)
    tags = await tag_repo.get_tags_with_colors(device_id)

    alert_repo = AlertRepository(session)
    alerts = await alert_repo.get_active_alerts(device_id)

    # Extract from cached last_health_json
    cached = device.last_health_json or {}
    cameras = [CameraChannelOut(**c) for c in cached.get("cameras", [])]
    disks = [DiskOut(**d) for d in cached.get("disks", [])]
    health_data = cached.get("health")
    health = HealthSummaryOut(**health_data) if health_data else None

    return DeviceDetailOut(
        device=_device_out(device, tags),
        cameras=cameras, disks=disks,
        alerts=[
            AlertOut(
                id=a.id, device_id=a.device_id, device_name=device.name,
                alert_type=a.alert_type, severity=a.severity,
                message=a.message, status=a.status, created_at=a.created_at,
                resolved_at=a.resolved_at,
            ) for a in alerts
        ],
        health=health,
    )


@router.get("/devices/{device_id}/credentials")
async def get_credentials(
    device_id: str,
    session: AsyncSession = Depends(get_session),
    settings: Settings = Depends(get_settings),
):
    repo = DeviceRepository(session)
    device = await repo.get_by_id(device_id)
    if device is None:
        raise HTTPException(status_code=404, detail="Device not found")
    password = decrypt_value(device.password_encrypted, settings.ENCRYPTION_KEY)
    return {"username": device.username, "password": password}


@router.get("/devices/{device_id}/ignored-channels", response_model=list[str])
async def get_ignored_channels(device_id: str, session: AsyncSession = Depends(get_session)):
    repo = DeviceRepository(session)
    device = await repo.get_by_id(device_id)
    if device is None:
        raise HTTPException(status_code=404, detail="Device not found")
    return device.ignored_channels or []


@router.put("/devices/{device_id}/ignored-channels", response_model=list[str])
async def set_ignored_channels(
    device_id: str,
    channels: list[str],
    session: AsyncSession = Depends(get_session),
):
    repo = DeviceRepository(session)
    device = await repo.get_by_id(device_id)
    if device is None:
        raise HTTPException(status_code=404, detail="Device not found")
    device.ignored_channels = channels
    await session.commit()
    return device.ignored_channels or []


@router.post("/devices/{device_id}/poll", response_model=PollResultOut)
async def poll_device(
    device_id: str,
    session: AsyncSession = Depends(get_session),
    settings: Settings = Depends(get_settings),
    registry: DriverRegistry = Depends(get_driver_registry),
    http_client: HttpClientManager = Depends(get_http_client),
):
    repo = DeviceRepository(session)
    device = await repo.get_by_id(device_id)
    if device is None:
        raise HTTPException(status_code=404, detail="Device not found")

    password = decrypt_value(device.password_encrypted, settings.ENCRYPTION_KEY)

    # TCP-check both ports in parallel (for display only, not for transport selection)
    web_port_open = None
    sdk_port_open = None
    if device.web_port and device.sdk_port:
        web_port_open, sdk_port_open = await asyncio.gather(
            _check_tcp_port(device.host, device.web_port, timeout=5.0),
            _check_tcp_port(device.host, device.sdk_port, timeout=5.0),
        )
    elif device.web_port:
        web_port_open = await _check_tcp_port(device.host, device.web_port, timeout=5.0)
    elif device.sdk_port:
        sdk_port_open = await _check_tcp_port(device.host, device.sdk_port, timeout=5.0)

    # Strategy: try ISAPI first (if web_port open), then SDK (if sdk_port configured).
    # TCP check is advisory — we always try the transport if port is configured.

    # 1) Try ISAPI if web port is open
    if device.web_port and web_port_open:
        return await _poll_via_isapi(
            device, password, device_id, repo, session,
            registry, http_client,
            web_port_open=web_port_open, sdk_port_open=sdk_port_open,
        )

    # 2) Try SDK subprocess if sdk_port is configured (regardless of TCP check result)
    if device.sdk_port and settings.HCNETSDK_LIB_PATH:
        result = await _poll_via_sdk_subprocess(
            device, password, device_id, repo, session,
            settings.HCNETSDK_LIB_PATH,
            web_port_open=web_port_open, sdk_port_open=sdk_port_open,
        )
        # If SDK succeeded, update sdk_port_open to True (it clearly works)
        if result.health.reachable and sdk_port_open is False:
            result.health.sdk_port_open = True
        return result

    # 3) No transport available — save failure
    from datetime import datetime, timezone as tz
    now = datetime.now(tz.utc)
    health = HealthSummaryOut(
        reachable=False, camera_count=0, online_cameras=0,
        offline_cameras=0, disk_ok=False, response_time_ms=0,
        checked_at=now,
        web_port_open=web_port_open, sdk_port_open=sdk_port_open,
    )
    health_json = {
        "reachable": False, "camera_count": 0,
        "online_cameras": 0, "offline_cameras": 0,
        "disk_ok": False, "response_time_ms": 0,
        "checked_at": now.isoformat(),
        "web_port_open": web_port_open, "sdk_port_open": sdk_port_open,
    }
    try:
        await repo.update(device_id, last_poll_at=now, last_health_json={
            "cameras": [], "disks": [], "health": health_json,
        })
        health_log_repo = DeviceHealthLogRepository(session)
        await health_log_repo.insert(
            device_id=device_id, reachable=False,
            camera_count=0, online_cameras=0,
            offline_cameras=0, disk_ok=False, response_time_ms=0,
        )
        await session.commit()
    except Exception:
        pass
    return PollResultOut(device_id=device_id, health=health)


async def _poll_via_isapi(
    device, password: str, device_id: str,
    repo: DeviceRepository, session: AsyncSession,
    registry: DriverRegistry, http_client: HttpClientManager,
    web_port_open: bool | None = None, sdk_port_open: bool | None = None,
) -> PollResultOut:
    """Poll device via ISAPI transport (in-process, safe)."""
    from datetime import datetime, timezone
    import time as _time

    vendor = DeviceVendor(device.vendor)
    driver_cls = registry.get(vendor)
    transport = IsapiTransport(http_client)
    driver = driver_cls(transport)

    config = DeviceConfig(
        device_id=device.device_id, name=device.name, vendor=vendor,
        host=device.host, web_port=device.web_port, sdk_port=device.sdk_port,
        username=device.username, password=password,
        transport_mode=DeviceTransport(device.transport_mode),
        polling_policy_id=device.polling_policy_id, is_active=device.is_active,
    )

    try:
        await driver.connect(config, port=device.web_port)
        start_t = _time.monotonic()

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

        # Recording status — prefer SDK FindFile (reliable) over ISAPI search
        rec_map: dict[str, str] = {}
        if device.sdk_port and settings.HCNETSDK_LIB_PATH:
            try:
                from cctv_monitor.polling.sdk_subprocess import poll_device_via_sdk_recordings
                sdk_recs = await poll_device_via_sdk_recordings(
                    host=device.host, port=device.sdk_port,
                    username=device.username, password=password,
                    lib_path=settings.HCNETSDK_LIB_PATH,
                    channels=[c.channel_id for c in cameras_raw],
                )
                for r in sdk_recs:
                    rec_map[r.get("channel_id", "")] = r.get("recording", "unknown")
            except Exception:
                pass
        if not rec_map:
            try:
                rec_statuses = await driver.get_recording_statuses()
                for r in rec_statuses:
                    rec_map[r.channel_id] = r.status.value
            except Exception:
                pass

        # Time check
        time_check_data: dict | None = None
        try:
            time_check_data = await driver.get_device_time()
        except Exception:
            pass

        response_time = (_time.monotonic() - start_t) * 1000
        now = datetime.now(timezone.utc)

        online = sum(1 for c in cameras_raw if c.status.value == "online")
        disk_ok = all(d.status.value == "ok" for d in disks_raw) if disks_raw else True

        health = HealthSummaryOut(
            reachable=True, camera_count=len(cameras_raw),
            online_cameras=online, offline_cameras=len(cameras_raw) - online,
            disk_ok=disk_ok, response_time_ms=response_time,
            checked_at=now,
            web_port_open=web_port_open, sdk_port_open=sdk_port_open,
        )

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
        health_json = {
            "reachable": True, "camera_count": len(cameras_raw),
            "online_cameras": online, "offline_cameras": len(cameras_raw) - online,
            "disk_ok": disk_ok, "response_time_ms": response_time,
            "checked_at": now.isoformat(),
            "web_port_open": web_port_open, "sdk_port_open": sdk_port_open,
            "time_check": time_check_data,
        }

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

        await repo.update(device_id, **update_fields)

        health_log_repo = DeviceHealthLogRepository(session)
        await health_log_repo.insert(
            device_id=device_id, reachable=True,
            camera_count=len(cameras_raw), online_cameras=online,
            offline_cameras=len(cameras_raw) - online,
            disk_ok=disk_ok, response_time_ms=response_time,
        )
        await session.commit()

    except Exception:
        now_err = datetime.now(timezone.utc)
        health = HealthSummaryOut(
            reachable=False, camera_count=0, online_cameras=0,
            offline_cameras=0, disk_ok=False, response_time_ms=0,
            checked_at=now_err,
            web_port_open=web_port_open, sdk_port_open=sdk_port_open,
        )
        health_json_err = {
            "reachable": False, "camera_count": 0,
            "online_cameras": 0, "offline_cameras": 0,
            "disk_ok": False, "response_time_ms": 0,
            "checked_at": now_err.isoformat(),
            "web_port_open": web_port_open, "sdk_port_open": sdk_port_open,
        }
        try:
            health_log_repo = DeviceHealthLogRepository(session)
            await health_log_repo.insert(
                device_id=device_id, reachable=False,
                camera_count=0, online_cameras=0,
                offline_cameras=0, disk_ok=False, response_time_ms=0,
            )
            await repo.update(device_id, last_poll_at=now_err, last_health_json={
                "cameras": [], "disks": [], "health": health_json_err,
            })
            await session.commit()
        except Exception:
            pass
        return PollResultOut(device_id=device_id, health=health)
    finally:
        try:
            await driver.disconnect()
        except Exception:
            pass

    return PollResultOut(device_id=device_id, health=health)


async def _poll_via_sdk_subprocess(
    device, password: str, device_id: str,
    repo: DeviceRepository, session: AsyncSession,
    lib_path: str,
    web_port_open: bool | None = None, sdk_port_open: bool | None = None,
) -> PollResultOut:
    """Poll device via SDK in an isolated subprocess (crash-safe)."""
    from datetime import datetime, timezone
    from cctv_monitor.polling.sdk_subprocess import poll_device_via_sdk

    result = await poll_device_via_sdk(
        host=device.host, port=device.sdk_port,
        username=device.username, password=password,
        lib_path=lib_path,
    )

    now = datetime.now(timezone.utc)

    if not result["success"]:
        health = HealthSummaryOut(
            reachable=False, camera_count=0, online_cameras=0,
            offline_cameras=0, disk_ok=False, response_time_ms=0,
            checked_at=now,
            web_port_open=web_port_open, sdk_port_open=sdk_port_open,
        )
        health_json_err = {
            "reachable": False, "camera_count": 0,
            "online_cameras": 0, "offline_cameras": 0,
            "disk_ok": False, "response_time_ms": 0,
            "checked_at": now.isoformat(),
            "web_port_open": web_port_open, "sdk_port_open": sdk_port_open,
        }
        try:
            health_log_repo = DeviceHealthLogRepository(session)
            await health_log_repo.insert(
                device_id=device_id, reachable=False,
                camera_count=0, online_cameras=0,
                offline_cameras=0, disk_ok=False, response_time_ms=0,
            )
            await repo.update(device_id, last_poll_at=now, last_health_json={
                "cameras": [], "disks": [], "health": health_json_err,
            })
            await session.commit()
        except Exception:
            pass
        return PollResultOut(device_id=device_id, health=health)

    # Parse subprocess results
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

    health = HealthSummaryOut(
        reachable=True, camera_count=len(cameras),
        online_cameras=online, offline_cameras=len(cameras) - online,
        disk_ok=disk_ok, response_time_ms=response_time,
        checked_at=now,
        web_port_open=web_port_open, sdk_port_open=sdk_port_open,
    )

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
    health_json = {
        "reachable": True, "camera_count": len(cameras),
        "online_cameras": online, "offline_cameras": len(cameras) - online,
        "disk_ok": disk_ok, "response_time_ms": response_time,
        "checked_at": now.isoformat(),
        "web_port_open": web_port_open, "sdk_port_open": sdk_port_open,
        "time_check": result.get("time_check"),
    }

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

    await repo.update(device_id, **update_fields)
    health_log_repo = DeviceHealthLogRepository(session)
    await health_log_repo.insert(
        device_id=device_id, reachable=True,
        camera_count=len(cameras), online_cameras=online,
        offline_cameras=len(cameras) - online,
        disk_ok=disk_ok, response_time_ms=response_time,
    )
    await session.commit()

    return PollResultOut(device_id=device_id, health=health)


@router.get("/devices/{device_id}/poll-stream")
async def poll_device_stream(
    device_id: str,
    session: AsyncSession = Depends(get_session),
    settings: Settings = Depends(get_settings),
    registry: DriverRegistry = Depends(get_driver_registry),
    http_client: HttpClientManager = Depends(get_http_client),
):
    """SSE endpoint — streams poll steps in real time."""
    import json as _json
    import time as _time
    from datetime import datetime, timezone

    repo = DeviceRepository(session)
    device = await repo.get_by_id(device_id)
    if device is None:
        raise HTTPException(status_code=404, detail="Device not found")

    password = decrypt_value(device.password_encrypted, settings.ENCRYPTION_KEY)

    async def _event_stream():
        def _sse(step: str, status: str, detail: str = "", **extra):
            data = {"step": step, "status": status, "detail": detail, **extra}
            return f"data: {_json.dumps(data, ensure_ascii=False)}\n\n"

        web_port_open = None
        sdk_port_open = None
        transport_used = None

        # Step 1: Check web port
        if device.web_port:
            yield _sse("web_port", "running", f"Checking TCP port {device.web_port}...")
            web_port_open = await _check_tcp_port(device.host, device.web_port, timeout=5.0)
            yield _sse("web_port", "success" if web_port_open else "error",
                        f"Port {device.web_port} {'open' if web_port_open else 'closed'}")
        else:
            yield _sse("web_port", "skipped", "Not configured")

        # Step 2: Check SDK port
        if device.sdk_port:
            yield _sse("sdk_port", "running", f"Checking TCP port {device.sdk_port}...")
            sdk_port_open = await _check_tcp_port(device.host, device.sdk_port, timeout=5.0)
            yield _sse("sdk_port", "success" if sdk_port_open else "error",
                        f"Port {device.sdk_port} {'open' if sdk_port_open else 'closed'}")
        else:
            yield _sse("sdk_port", "skipped", "Not configured")

        # Step 3: Connect — choose transport
        driver = None
        connected = False

        if device.web_port and web_port_open:
            transport_used = "isapi"
            yield _sse("connect", "running", f"Connecting via ISAPI ({device.host}:{device.web_port})...")
            try:
                vendor = DeviceVendor(device.vendor)
                driver_cls = registry.get(vendor)
                transport_obj = IsapiTransport(http_client)
                driver = driver_cls(transport_obj)
                config = DeviceConfig(
                    device_id=device.device_id, name=device.name, vendor=vendor,
                    host=device.host, web_port=device.web_port, sdk_port=device.sdk_port,
                    username=device.username, password=password,
                    transport_mode=DeviceTransport(device.transport_mode),
                    polling_policy_id=device.polling_policy_id, is_active=device.is_active,
                )
                await driver.connect(config, port=device.web_port)
                connected = True
                yield _sse("connect", "success", "ISAPI connected")
            except Exception as exc:
                yield _sse("connect", "error", f"ISAPI failed: {exc}")
                driver = None

        if not connected and device.sdk_port:
            transport_used = "sdk"
            yield _sse("connect", "running", f"Connecting via SDK ({device.host}:{device.sdk_port})...")
            # SDK uses subprocess — we'll handle it below
            yield _sse("connect", "success", "Using SDK subprocess")
            connected = True

        if not connected:
            yield _sse("connect", "error", "No transport available")
            yield _sse("done", "error", "Poll failed — no connection")
            # Save failure
            now = datetime.now(timezone.utc)
            health_json = {
                "reachable": False, "camera_count": 0,
                "online_cameras": 0, "offline_cameras": 0,
                "disk_ok": False, "response_time_ms": 0,
                "checked_at": now.isoformat(),
                "web_port_open": web_port_open, "sdk_port_open": sdk_port_open,
            }
            try:
                await repo.update(device_id, last_poll_at=now, last_health_json={
                    "cameras": [], "disks": [], "health": health_json,
                })
                health_log_repo = DeviceHealthLogRepository(session)
                await health_log_repo.insert(
                    device_id=device_id, reachable=False,
                    camera_count=0, online_cameras=0,
                    offline_cameras=0, disk_ok=False, response_time_ms=0,
                )
                await session.commit()
            except Exception:
                pass
            return

        # ── ISAPI path ──
        if transport_used == "isapi" and driver is not None:
            start_t = _time.monotonic()

            # Step 4: Device info
            device_info = None
            yield _sse("device_info", "running", "Getting device info...")
            try:
                device_info = await driver.get_device_info()
                detail = f"{device_info.model}" if device_info and device_info.model else "OK"
                yield _sse("device_info", "success", detail)
            except Exception as exc:
                yield _sse("device_info", "error", str(exc))

            # Step 5: Cameras
            cameras_raw = []
            yield _sse("cameras", "running", "Getting camera statuses...")
            try:
                cameras_raw = await driver.get_camera_statuses()
                online = sum(1 for c in cameras_raw if c.status.value == "online")
                yield _sse("cameras", "success",
                           f"{len(cameras_raw)} cameras, {online} online")
            except Exception as exc:
                yield _sse("cameras", "error", str(exc))

            # Step 6: Disks
            disks_raw = []
            yield _sse("disks", "running", "Getting disk statuses...")
            try:
                disks_raw = await driver.get_disk_statuses()
                all_ok = all(d.status.value == "ok" for d in disks_raw) if disks_raw else True
                smart_count = sum(1 for d in disks_raw if d.smart_status is not None)
                smart_info = f", SMART: {smart_count}/{len(disks_raw)}" if disks_raw else ""
                yield _sse("disks", "success",
                           f"{len(disks_raw)} disks, {'all OK' if all_ok else 'ERROR'}{smart_info}")
            except Exception as exc:
                yield _sse("disks", "error", str(exc))

            # Step 7: Recording status — prefer SDK FindFile over ISAPI search
            rec_map: dict[str, str] = {}
            yield _sse("recording", "running", "Checking recording status...")
            try:
                if device.sdk_port and settings.HCNETSDK_LIB_PATH:
                    from cctv_monitor.polling.sdk_subprocess import poll_device_via_sdk_recordings
                    sdk_recs = await poll_device_via_sdk_recordings(
                        host=device.host, port=device.sdk_port,
                        username=device.username, password=password,
                        lib_path=settings.HCNETSDK_LIB_PATH,
                        channels=[c.channel_id for c in cameras_raw],
                    )
                    for r in sdk_recs:
                        rec_map[r.get("channel_id", "")] = r.get("recording", "unknown")
                if not rec_map:
                    rec_statuses = await driver.get_recording_statuses()
                    for r in rec_statuses:
                        rec_map[r.channel_id] = r.status.value
                recording_count = sum(1 for v in rec_map.values() if v == "recording")
                yield _sse("recording", "success",
                           f"{recording_count}/{len(rec_map)} recording" if rec_map else "Not available")
            except Exception as exc:
                yield _sse("recording", "error", str(exc))

            # Step 8: Time check
            time_check_data: dict | None = None
            yield _sse("time_check", "running", "Checking device time...")
            try:
                time_check_data = await driver.get_device_time()
                if time_check_data:
                    drift = time_check_data["drift_seconds"]
                    abs_drift = abs(drift)
                    if abs_drift < 5:
                        drift_label = "synced"
                    elif abs_drift < 30:
                        drift_label = f"{drift:+d}s"
                    elif abs_drift < 3600:
                        drift_label = f"{drift:+d}s ({abs_drift // 60}m)"
                    else:
                        drift_label = f"{drift:+d}s ({abs_drift // 3600}h {(abs_drift % 3600) // 60}m)"
                    mode = time_check_data.get("time_mode", "")
                    mode_label = f", {mode}" if mode else ""
                    status = "success" if abs_drift < 30 else ("error" if abs_drift > 300 else "success")
                    yield _sse("time_check", status, f"{drift_label}{mode_label}")
                else:
                    yield _sse("time_check", "skipped", "Not supported")
            except Exception as exc:
                yield _sse("time_check", "skipped", str(exc))

            response_time = (_time.monotonic() - start_t) * 1000
            now = datetime.now(timezone.utc)
            online = sum(1 for c in cameras_raw if c.status.value == "online")
            disk_ok = all(d.status.value == "ok" for d in disks_raw) if disks_raw else True

            # Save results
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
            health_json = {
                "reachable": True, "camera_count": len(cameras_raw),
                "online_cameras": online, "offline_cameras": len(cameras_raw) - online,
                "disk_ok": disk_ok, "response_time_ms": response_time,
                "checked_at": now.isoformat(),
                "web_port_open": web_port_open, "sdk_port_open": sdk_port_open,
                "time_check": time_check_data,
            }
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

            await repo.update(device_id, **update_fields)
            health_log_repo = DeviceHealthLogRepository(session)
            await health_log_repo.insert(
                device_id=device_id, reachable=True,
                camera_count=len(cameras_raw), online_cameras=online,
                offline_cameras=len(cameras_raw) - online,
                disk_ok=disk_ok, response_time_ms=response_time,
            )
            await session.commit()

            try:
                await driver.disconnect()
            except Exception:
                pass

            yield _sse("done", "success",
                        f"Poll complete — {len(cameras_raw)} cameras, {online} online, "
                        f"disks {'OK' if disk_ok else 'ERROR'}, {round(response_time)}ms",
                        response_time_ms=round(response_time, 1))

        # ── SDK path ──
        elif transport_used == "sdk":
            from cctv_monitor.polling.sdk_subprocess import poll_device_via_sdk

            yield _sse("device_info", "running", "Polling via SDK subprocess...")

            result = await poll_device_via_sdk(
                host=device.host, port=device.sdk_port,
                username=device.username, password=password,
                lib_path=settings.HCNETSDK_LIB_PATH,
            )

            if not result["success"]:
                err_msg = result.get("error", "SDK failed")
                yield _sse("device_info", "error", err_msg)
                yield _sse("cameras", "skipped", "")
                yield _sse("disks", "skipped", "")

                # Save failure
                now = datetime.now(timezone.utc)
                sdk_port_open = sdk_port_open if sdk_port_open is not None else False
                health_json = {
                    "reachable": False, "camera_count": 0,
                    "online_cameras": 0, "offline_cameras": 0,
                    "disk_ok": False, "response_time_ms": 0,
                    "checked_at": now.isoformat(),
                    "web_port_open": web_port_open, "sdk_port_open": sdk_port_open,
                }
                try:
                    await repo.update(device_id, last_poll_at=now, last_health_json={
                        "cameras": [], "disks": [], "health": health_json,
                    })
                    health_log_repo = DeviceHealthLogRepository(session)
                    await health_log_repo.insert(
                        device_id=device_id, reachable=False,
                        camera_count=0, online_cameras=0,
                        offline_cameras=0, disk_ok=False, response_time_ms=0,
                    )
                    await session.commit()
                except Exception:
                    pass

                yield _sse("done", "error", f"SDK poll failed: {err_msg}")
                return

            # SDK success
            dev_info = result.get("device_info")
            if dev_info and isinstance(dev_info, dict) and dev_info.get("model"):
                yield _sse("device_info", "success", dev_info["model"])
            else:
                yield _sse("device_info", "success", "Connected")

            cameras = result.get("cameras", [])
            online = sum(1 for c in cameras if c.get("online") is True)
            yield _sse("cameras", "success", f"{len(cameras)} cameras, {online} online")

            disks = result.get("disks", [])
            disk_ok = all(d.get("status") in ("normal", "ok") for d in disks) if disks else True
            smart_count = sum(1 for d in disks if d.get("smart_status") is not None)
            smart_info = f", SMART: {smart_count}/{len(disks)}" if disks else ""
            yield _sse("disks", "success",
                        f"{len(disks)} disks, {'all OK' if disk_ok else 'ERROR'}{smart_info}")

            # Recording status from SDK worker
            recordings = result.get("recordings", [])
            rec_map: dict[str, str] = {}
            for r in recordings:
                rec_map[r.get("channel_id", "")] = r.get("recording", "unknown")
            if rec_map:
                recording_count = sum(1 for v in rec_map.values() if v == "recording")
                yield _sse("recording", "success", f"{recording_count}/{len(rec_map)} recording")
            else:
                yield _sse("recording", "skipped", "Not available via SDK")

            # Time check from SDK worker
            time_check_data = result.get("time_check")
            if time_check_data:
                drift = time_check_data.get("drift_seconds", 0)
                abs_drift = abs(drift)
                if abs_drift < 5:
                    drift_label = "synced"
                elif abs_drift < 30:
                    drift_label = f"{drift:+d}s"
                elif abs_drift < 3600:
                    drift_label = f"{drift:+d}s ({abs_drift // 60}m)"
                else:
                    drift_label = f"{drift:+d}s ({abs_drift // 3600}h {(abs_drift % 3600) // 60}m)"
                mode = time_check_data.get("time_mode", "")
                mode_label = f", {mode}" if mode else ""
                status = "success" if abs_drift < 30 else ("error" if abs_drift > 300 else "success")
                yield _sse("time_check", status, f"{drift_label}{mode_label}")
            else:
                yield _sse("time_check", "skipped", "Not available")

            now = datetime.now(timezone.utc)
            response_time = result.get("response_time_ms", 0)
            sdk_port_open = True  # SDK worked

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
            health_json = {
                "reachable": True, "camera_count": len(cameras),
                "online_cameras": online, "offline_cameras": len(cameras) - online,
                "disk_ok": disk_ok, "response_time_ms": response_time,
                "checked_at": now.isoformat(),
                "web_port_open": web_port_open, "sdk_port_open": sdk_port_open,
                "time_check": time_check_data,
            }
            update_fields: dict = {
                "last_poll_at": now,
                "last_health_json": {
                    "cameras": cameras_json, "disks": disks_json, "health": health_json,
                },
            }
            if dev_info and isinstance(dev_info, dict):
                if dev_info.get("model"):
                    update_fields["model"] = dev_info["model"]
                if dev_info.get("serial_number"):
                    update_fields["serial_number"] = dev_info["serial_number"]
                if dev_info.get("firmware_version"):
                    update_fields["firmware_version"] = dev_info["firmware_version"]

            await repo.update(device_id, **update_fields)
            health_log_repo = DeviceHealthLogRepository(session)
            await health_log_repo.insert(
                device_id=device_id, reachable=True,
                camera_count=len(cameras), online_cameras=online,
                offline_cameras=len(cameras) - online,
                disk_ok=disk_ok, response_time_ms=response_time,
            )
            await session.commit()

            yield _sse("done", "success",
                        f"Poll complete — {len(cameras)} cameras, {online} online, "
                        f"disks {'OK' if disk_ok else 'ERROR'}, {round(response_time)}ms",
                        response_time_ms=round(response_time, 1))

    return StreamingResponse(
        _event_stream(),
        media_type="text/event-stream",
        headers={"Cache-Control": "no-cache", "X-Accel-Buffering": "no"},
    )


# Limit concurrent snapshot requests per device to avoid overwhelming NVRs
_snapshot_semaphores: dict[str, asyncio.Semaphore] = {}
_SNAPSHOT_CONCURRENCY = 4  # max parallel snapshot fetches per device


def _get_snapshot_semaphore(device_id: str) -> asyncio.Semaphore:
    if device_id not in _snapshot_semaphores:
        _snapshot_semaphores[device_id] = asyncio.Semaphore(_SNAPSHOT_CONCURRENCY)
    return _snapshot_semaphores[device_id]


@router.get("/devices/{device_id}/snapshot/{channel_id}")
async def get_snapshot(
    device_id: str,
    channel_id: str,
    session: AsyncSession = Depends(get_session),
    settings: Settings = Depends(get_settings),
    http_client: HttpClientManager = Depends(get_http_client),
):
    repo = DeviceRepository(session)
    device = await repo.get_by_id(device_id)
    if device is None:
        raise HTTPException(status_code=404, detail="Device not found")

    password = decrypt_value(device.password_encrypted, settings.ENCRYPTION_KEY)
    sdk_available = device.sdk_port and settings.HCNETSDK_LIB_PATH is not None
    prefer_sdk = device.transport_mode == DeviceTransport.SDK

    semaphore = _get_snapshot_semaphore(device_id)
    try:
        async with semaphore:
            image_data = None
            errors: list[str] = []

            # Build attempt order based on transport_mode (try both, preferred first)
            attempts: list[str] = []
            if prefer_sdk:
                if sdk_available:
                    attempts.append("sdk")
                if device.web_port:
                    attempts.append("isapi")
            else:
                if device.web_port:
                    attempts.append("isapi")
                if sdk_available:
                    attempts.append("sdk")

            for method in attempts:
                if image_data is not None:
                    break

                if method == "sdk":
                    try:
                        image_data = await _sdk_snapshot_subprocess(
                            device.host, device.sdk_port, device.username, password,
                            int(channel_id), settings.HCNETSDK_LIB_PATH,
                        )
                    except Exception as sdk_exc:
                        errors.append(f"SDK: {sdk_exc}")
                        logger.warning(
                            "SDK snapshot failed for device=%s channel=%s: %s",
                            device_id, channel_id, sdk_exc,
                        )

                elif method == "isapi":
                    transport = IsapiTransport(http_client)
                    try:
                        await transport.connect(device.host, device.web_port, device.username, password)
                        image_data = await transport.get_snapshot(channel_id)
                    except Exception as isapi_exc:
                        errors.append(f"ISAPI: {isapi_exc}")
                        logger.warning(
                            "ISAPI snapshot failed for device=%s channel=%s: %s",
                            device_id, channel_id, isapi_exc,
                        )
                    finally:
                        try:
                            await transport.disconnect()
                        except Exception:
                            pass

            if image_data is None:
                if not attempts:
                    raise HTTPException(status_code=400, detail="No port configured for snapshots")
                raise HTTPException(status_code=502, detail=f"All snapshot methods failed: {'; '.join(errors)}")
    except HTTPException:
        raise
    except Exception as exc:
        logger.warning("Snapshot failed for device=%s channel=%s: %s", device_id, channel_id, exc)
        raise HTTPException(status_code=502, detail=f"Failed to get snapshot: {exc}")

    return StreamingResponse(
        io.BytesIO(image_data),
        media_type="image/jpeg",
        headers={"Cache-Control": "public, max-age=30"},
    )


async def _sdk_snapshot_subprocess(
    host: str, port: int, username: str, password: str,
    channel: int, lib_path: str,
) -> bytes:
    """Run SDK snapshot capture in an isolated subprocess."""
    import sys

    cmd = [
        sys.executable, "-m", "cctv_monitor.polling.sdk_worker",
        "--host", host,
        "--port", str(port),
        "--user", username,
        "--password", password,
        "--lib-path", lib_path,
        "--snapshot",
        "--channel", str(channel),
    ]

    proc = await asyncio.create_subprocess_exec(
        *cmd,
        stdout=asyncio.subprocess.PIPE,
        stderr=asyncio.subprocess.PIPE,
    )

    try:
        stdout, stderr = await asyncio.wait_for(proc.communicate(), timeout=15)
    except asyncio.TimeoutError:
        proc.kill()
        await proc.wait()
        raise Exception("SDK snapshot subprocess timed out")

    if proc.returncode != 0:
        err_msg = stderr.decode(errors="replace").strip() if stderr else ""
        if not err_msg:
            # Crashed subprocess (e.g. 0xC0000005 = access violation)
            err_msg = f"subprocess crashed (exit code {proc.returncode})"
        raise Exception(f"SDK snapshot failed: {err_msg}")

    if not stdout or len(stdout) < 2:
        raise Exception("SDK snapshot returned empty data")

    # Validate JPEG header
    if stdout[:2] != b"\xff\xd8":
        raise Exception("SDK snapshot returned invalid JPEG data")

    return stdout
