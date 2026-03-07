from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.api.deps import get_session
from cctv_monitor.api.schemas import OverviewOut
from cctv_monitor.storage.repositories import DeviceRepository

router = APIRouter(tags=["status"])


@router.get("/overview", response_model=OverviewOut)
async def get_overview(session: AsyncSession = Depends(get_session)):
    repo = DeviceRepository(session)
    devices = await repo.list_all()
    return OverviewOut(
        total_devices=len(devices), reachable_devices=0,
        total_cameras=0, online_cameras=0, disks_ok=True,
    )
