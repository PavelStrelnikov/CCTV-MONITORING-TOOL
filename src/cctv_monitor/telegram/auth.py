"""Authorization helpers for Telegram commands."""

from cctv_monitor.telegram.api_client import TelegramApiClient


async def get_access(api_client: TelegramApiClient, telegram_user_id: int) -> tuple[bool, str | None]:
    payload = await api_client.check_access(telegram_user_id)
    return bool(payload.get("allowed", False)), payload.get("role")
