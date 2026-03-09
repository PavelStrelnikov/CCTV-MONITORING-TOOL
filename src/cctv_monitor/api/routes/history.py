from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.api.deps import get_session
from cctv_monitor.api.schemas import HealthLogEntryOut, PollLogEntryOut
from cctv_monitor.storage.repositories import DeviceRepository, DeviceHealthLogRepository

router = APIRouter(tags=["history"])


@router.get("/devices/{device_id}/history", response_model=list[HealthLogEntryOut])
async def get_device_history(
    device_id: str,
    hours: int = 24,
    session: AsyncSession = Depends(get_session),
):
    repo = DeviceRepository(session)
    device = await repo.get_by_id(device_id)
    if device is None:
        raise HTTPException(status_code=404, detail="Device not found")

    health_log_repo = DeviceHealthLogRepository(session)
    entries = await health_log_repo.get_history(device_id, hours=hours)

    return [
        HealthLogEntryOut(
            reachable=e.reachable,
            camera_count=e.camera_count,
            online_cameras=e.online_cameras,
            offline_cameras=e.offline_cameras,
            disk_ok=e.disk_ok,
            response_time_ms=e.response_time_ms,
            checked_at=e.checked_at,
        )
        for e in entries
    ]


@router.get("/poll-logs", response_model=list[PollLogEntryOut])
async def get_poll_logs(
    hours: int = 24,
    limit: int = 500,
    session: AsyncSession = Depends(get_session),
):
    """Cross-device poll history — newest first."""
    device_repo = DeviceRepository(session)
    devices = await device_repo.list_all()
    name_map = {d.device_id: d.name for d in devices}

    health_log_repo = DeviceHealthLogRepository(session)
    entries = await health_log_repo.get_all_history(hours=hours, limit=limit)

    return [
        PollLogEntryOut(
            device_id=e.device_id,
            device_name=name_map.get(e.device_id, e.device_id),
            reachable=e.reachable,
            camera_count=e.camera_count,
            online_cameras=e.online_cameras,
            offline_cameras=e.offline_cameras,
            disk_ok=e.disk_ok,
            response_time_ms=e.response_time_ms,
            checked_at=e.checked_at,
        )
        for e in entries
    ]
