from fastapi import APIRouter, Depends, HTTPException
from pydantic import BaseModel
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.api.deps import get_session, verify_internal_api_token
from cctv_monitor.storage.repositories import (
    TelegramAuditRepository,
    TelegramSubscriptionRepository,
    TelegramUserRepository,
)

router = APIRouter(tags=["telegram"], dependencies=[Depends(verify_internal_api_token)])


class TelegramAccessOut(BaseModel):
    allowed: bool
    role: str | None = None


class TelegramCommandAuditIn(BaseModel):
    telegram_user_id: int
    telegram_chat_id: int | None = None
    command: str
    args_json: dict | None = None
    status: str
    error_message: str | None = None


class TelegramSubscriptionIn(BaseModel):
    enabled: bool
    timezone: str = "Asia/Jerusalem"
    schedule_cron: str | None = None


class TelegramSubscriptionOut(BaseModel):
    telegram_user_id: int
    subscription_type: str
    is_enabled: bool
    timezone: str
    schedule_cron: str | None = None


class TelegramUserUpsertIn(BaseModel):
    telegram_user_id: int
    username: str | None = None
    display_name: str | None = None
    role: str = "viewer"
    is_active: bool = True


class TelegramUserOut(BaseModel):
    telegram_user_id: int
    username: str | None = None
    display_name: str | None = None
    role: str
    is_active: bool


@router.get("/telegram/access/{telegram_user_id}", response_model=TelegramAccessOut)
async def telegram_access_check(
    telegram_user_id: int,
    session: AsyncSession = Depends(get_session),
):
    repo = TelegramUserRepository(session)
    user = await repo.get_by_telegram_user_id(telegram_user_id)
    if user is None or not user.is_active:
        return TelegramAccessOut(allowed=False, role=None)
    return TelegramAccessOut(allowed=True, role=user.role)


@router.put("/telegram/users/{telegram_user_id}", response_model=TelegramUserOut)
async def upsert_telegram_user(
    telegram_user_id: int,
    body: TelegramUserUpsertIn,
    session: AsyncSession = Depends(get_session),
):
    if telegram_user_id != body.telegram_user_id:
        raise HTTPException(status_code=400, detail="telegram_user_id mismatch")

    repo = TelegramUserRepository(session)
    row = await repo.upsert_user(
        telegram_user_id=body.telegram_user_id,
        username=body.username,
        display_name=body.display_name,
        role=body.role,
        is_active=body.is_active,
    )
    await session.commit()
    return TelegramUserOut(
        telegram_user_id=row.telegram_user_id,
        username=row.username,
        display_name=row.display_name,
        role=row.role,
        is_active=row.is_active,
    )


@router.post("/telegram/audit", status_code=201)
async def create_telegram_audit(
    body: TelegramCommandAuditIn,
    session: AsyncSession = Depends(get_session),
):
    repo = TelegramAuditRepository(session)
    await repo.log_command(
        telegram_user_id=body.telegram_user_id,
        telegram_chat_id=body.telegram_chat_id,
        command=body.command,
        args_json=body.args_json,
        status=body.status,
        error_message=body.error_message,
    )
    await session.commit()
    return {"status": "ok"}


@router.put(
    "/telegram/subscriptions/{telegram_user_id}/{subscription_type}",
    response_model=TelegramSubscriptionOut,
)
async def upsert_telegram_subscription(
    telegram_user_id: int,
    subscription_type: str,
    body: TelegramSubscriptionIn,
    session: AsyncSession = Depends(get_session),
):
    user_repo = TelegramUserRepository(session)
    user = await user_repo.get_by_telegram_user_id(telegram_user_id)
    if user is None or not user.is_active:
        raise HTTPException(status_code=404, detail="Active telegram user not found")

    repo = TelegramSubscriptionRepository(session)
    row = await repo.set_subscription(
        telegram_user_id=telegram_user_id,
        subscription_type=subscription_type,
        enabled=body.enabled,
        timezone_name=body.timezone,
        schedule_cron=body.schedule_cron,
    )
    await session.commit()
    return TelegramSubscriptionOut(
        telegram_user_id=row.telegram_user_id,
        subscription_type=row.subscription_type,
        is_enabled=row.is_enabled,
        timezone=row.timezone,
        schedule_cron=row.schedule_cron,
    )


@router.get(
    "/telegram/subscriptions/{telegram_user_id}/{subscription_type}",
    response_model=TelegramSubscriptionOut,
)
async def get_telegram_subscription(
    telegram_user_id: int,
    subscription_type: str,
    session: AsyncSession = Depends(get_session),
):
    repo = TelegramSubscriptionRepository(session)
    row = await repo.get_subscription(
        telegram_user_id=telegram_user_id,
        subscription_type=subscription_type,
    )
    if row is None:
        raise HTTPException(status_code=404, detail="Subscription not found")
    return TelegramSubscriptionOut(
        telegram_user_id=row.telegram_user_id,
        subscription_type=row.subscription_type,
        is_enabled=row.is_enabled,
        timezone=row.timezone,
        schedule_cron=row.schedule_cron,
    )
