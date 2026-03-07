from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.exc import IntegrityError
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.api.deps import get_session, get_settings, get_driver_registry, get_http_client
from cctv_monitor.api.schemas import (
    DeviceCreate, DeviceOut, DeviceDetailOut, PollResultOut,
    HealthSummaryOut, AlertOut,
)
from cctv_monitor.core.config import Settings
from cctv_monitor.core.crypto import encrypt_value, decrypt_value
from cctv_monitor.core.types import DeviceTransport, DeviceVendor
from cctv_monitor.drivers.hikvision.transports.isapi import IsapiTransport
from cctv_monitor.drivers.registry import DriverRegistry
from cctv_monitor.core.http_client import HttpClientManager
from cctv_monitor.models.device import DeviceConfig
from cctv_monitor.storage.repositories import DeviceRepository, AlertRepository
from cctv_monitor.storage.tables import DeviceTable

router = APIRouter(tags=["devices"])


@router.get("/devices", response_model=list[DeviceOut])
async def list_devices(session: AsyncSession = Depends(get_session)):
    repo = DeviceRepository(session)
    devices = await repo.list_all()
    return [
        DeviceOut(
            device_id=d.device_id, name=d.name, vendor=d.vendor,
            host=d.host, port=d.port, is_active=d.is_active, last_health=None,
        )
        for d in devices
    ]


@router.post("/devices", response_model=DeviceOut, status_code=201)
async def create_device(
    body: DeviceCreate,
    session: AsyncSession = Depends(get_session),
    settings: Settings = Depends(get_settings),
):
    encrypted_password = encrypt_value(body.password, settings.ENCRYPTION_KEY)
    device = DeviceTable(
        device_id=body.device_id, name=body.name, vendor=body.vendor,
        host=body.host, port=body.port, username=body.username,
        password_encrypted=encrypted_password, transport_mode="isapi", is_active=True,
    )
    repo = DeviceRepository(session)
    try:
        await repo.create(device)
        await session.commit()
    except IntegrityError:
        await session.rollback()
        raise HTTPException(status_code=409, detail=f"Device '{body.device_id}' already exists")
    return DeviceOut(
        device_id=device.device_id, name=device.name, vendor=device.vendor,
        host=device.host, port=device.port, is_active=device.is_active, last_health=None,
    )


@router.delete("/devices/{device_id}", status_code=204)
async def delete_device(device_id: str, session: AsyncSession = Depends(get_session)):
    repo = DeviceRepository(session)
    deleted = await repo.delete(device_id)
    if not deleted:
        raise HTTPException(status_code=404, detail="Device not found")
    await session.commit()


@router.get("/devices/{device_id}", response_model=DeviceDetailOut)
async def get_device_detail(device_id: str, session: AsyncSession = Depends(get_session)):
    repo = DeviceRepository(session)
    device = await repo.get_by_id(device_id)
    if device is None:
        raise HTTPException(status_code=404, detail="Device not found")
    alert_repo = AlertRepository(session)
    alerts = await alert_repo.get_active_alerts(device_id)
    return DeviceDetailOut(
        device=DeviceOut(
            device_id=device.device_id, name=device.name, vendor=device.vendor,
            host=device.host, port=device.port, is_active=device.is_active, last_health=None,
        ),
        cameras=[], disks=[],
        alerts=[
            AlertOut(
                id=a.id, alert_type=a.alert_type, severity=a.severity,
                message=a.message, status=a.status, created_at=a.created_at,
                resolved_at=a.resolved_at,
            ) for a in alerts
        ],
    )


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

    vendor = DeviceVendor(device.vendor)
    driver_cls = registry.get(vendor)
    transport = IsapiTransport(http_client)
    driver = driver_cls(transport)

    password = decrypt_value(device.password_encrypted, settings.ENCRYPTION_KEY)
    config = DeviceConfig(
        device_id=device.device_id, name=device.name, vendor=vendor,
        host=device.host, port=device.port, username=device.username,
        password=password, transport_mode=DeviceTransport(device.transport_mode),
        polling_policy_id=device.polling_policy_id, is_active=device.is_active,
    )

    try:
        await driver.connect(config)
        health = await driver.check_health()
    finally:
        try:
            await driver.disconnect()
        except Exception:
            pass

    return PollResultOut(
        device_id=device_id,
        health=HealthSummaryOut(
            reachable=health.reachable, camera_count=health.camera_count,
            online_cameras=health.online_cameras, offline_cameras=health.offline_cameras,
            disk_ok=health.disk_ok, response_time_ms=health.response_time_ms,
            checked_at=health.checked_at,
        ),
    )
