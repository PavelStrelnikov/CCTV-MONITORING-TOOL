from fastapi import APIRouter, Depends
from pydantic import BaseModel
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.api.deps import get_session
from cctv_monitor.storage.repositories import SystemSettingsRepository

router = APIRouter(tags=["settings"])


class SystemSettingsOut(BaseModel):
    default_poll_interval: int


class SystemSettingsUpdate(BaseModel):
    default_poll_interval: int | None = None


@router.get("/settings", response_model=SystemSettingsOut)
async def get_settings(session: AsyncSession = Depends(get_session)):
    repo = SystemSettingsRepository(session)
    all_settings = await repo.get_all()
    return SystemSettingsOut(
        default_poll_interval=int(all_settings.get("default_poll_interval", "900")),
    )


@router.put("/settings", response_model=SystemSettingsOut)
async def update_settings(
    body: SystemSettingsUpdate,
    session: AsyncSession = Depends(get_session),
):
    repo = SystemSettingsRepository(session)
    if body.default_poll_interval is not None:
        await repo.set("default_poll_interval", str(body.default_poll_interval))
    await session.commit()

    all_settings = await repo.get_all()
    return SystemSettingsOut(
        default_poll_interval=int(all_settings.get("default_poll_interval", "900")),
    )
