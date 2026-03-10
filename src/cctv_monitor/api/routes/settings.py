from fastapi import APIRouter, Depends
from pydantic import BaseModel
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.api.deps import get_session
from cctv_monitor.storage.repositories import SystemSettingsRepository

router = APIRouter(tags=["settings"])


class SystemSettingsOut(BaseModel):
    default_poll_interval: int
    polling_enabled: bool


class SystemSettingsUpdate(BaseModel):
    default_poll_interval: int | None = None
    polling_enabled: bool | None = None


@router.get("/settings", response_model=SystemSettingsOut)
async def get_settings(session: AsyncSession = Depends(get_session)):
    repo = SystemSettingsRepository(session)
    all_settings = await repo.get_all()
    return SystemSettingsOut(
        default_poll_interval=int(all_settings.get("default_poll_interval", "900")),
        polling_enabled=all_settings.get("polling_enabled", "true") != "false",
    )


@router.put("/settings", response_model=SystemSettingsOut)
async def update_settings(
    body: SystemSettingsUpdate,
    session: AsyncSession = Depends(get_session),
):
    repo = SystemSettingsRepository(session)
    if body.default_poll_interval is not None:
        await repo.set("default_poll_interval", str(body.default_poll_interval))
    if body.polling_enabled is not None:
        await repo.set("polling_enabled", str(body.polling_enabled).lower())
    await session.commit()

    all_settings = await repo.get_all()
    return SystemSettingsOut(
        default_poll_interval=int(all_settings.get("default_poll_interval", "900")),
        polling_enabled=all_settings.get("polling_enabled", "true") != "false",
    )
