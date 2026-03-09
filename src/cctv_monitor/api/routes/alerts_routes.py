from fastapi import APIRouter, Depends
from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.api.deps import get_session
from cctv_monitor.api.schemas import AlertOut
from cctv_monitor.storage.repositories import AlertRepository
from cctv_monitor.storage.tables import DeviceTable

router = APIRouter(tags=["alerts"])


@router.get("/alerts", response_model=list[AlertOut])
async def list_alerts(
    status: str | None = None,
    device_id: str | None = None,
    session: AsyncSession = Depends(get_session),
):
    alert_repo = AlertRepository(session)
    alerts = await alert_repo.get_all_alerts(status=status, device_id=device_id)

    # Build device name lookup
    device_ids = {a.device_id for a in alerts}
    name_map: dict[str, str] = {}
    if device_ids:
        result = await session.execute(
            select(DeviceTable.device_id, DeviceTable.name).where(
                DeviceTable.device_id.in_(device_ids)
            )
        )
        name_map = {row.device_id: row.name for row in result}

    return [
        AlertOut(
            id=a.id,
            device_id=a.device_id,
            device_name=name_map.get(a.device_id, a.device_id),
            alert_type=a.alert_type,
            severity=a.severity,
            message=a.message,
            status=a.status,
            created_at=a.created_at,
            resolved_at=a.resolved_at,
        )
        for a in alerts
    ]
