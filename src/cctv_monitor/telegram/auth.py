"""Authorization helpers for Telegram commands."""

from cctv_monitor.storage.repositories import TelegramUserRepository


async def is_user_allowed(repo: TelegramUserRepository, telegram_user_id: int) -> bool:
    user = await repo.get_by_telegram_user_id(telegram_user_id)
    return bool(user and user.is_active)
