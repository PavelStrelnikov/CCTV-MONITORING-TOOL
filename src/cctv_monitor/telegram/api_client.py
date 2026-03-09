"""Internal API client for Telegram handlers."""

from __future__ import annotations

import httpx


class TelegramApiClient:
    """Small HTTP client wrapper for backend API calls."""

    def __init__(self, base_url: str, internal_token: str | None = None) -> None:
        self._base_url = base_url.rstrip("/")
        self._internal_token = internal_token

    def _headers(self) -> dict[str, str]:
        headers: dict[str, str] = {}
        if self._internal_token:
            headers["X-Internal-Token"] = self._internal_token
        return headers

    async def get_overview(self) -> dict:
        async with httpx.AsyncClient(timeout=15.0) as client:
            response = await client.get(f"{self._base_url}/api/overview", headers=self._headers())
            response.raise_for_status()
            return response.json()

    async def check_access(self, telegram_user_id: int) -> dict:
        async with httpx.AsyncClient(timeout=10.0) as client:
            response = await client.get(
                f"{self._base_url}/api/telegram/access/{telegram_user_id}",
                headers=self._headers(),
            )
            response.raise_for_status()
            return response.json()

    async def write_audit(
        self,
        *,
        telegram_user_id: int,
        telegram_chat_id: int | None,
        command: str,
        status: str,
        args_json: dict | None = None,
        error_message: str | None = None,
    ) -> None:
        payload = {
            "telegram_user_id": telegram_user_id,
            "telegram_chat_id": telegram_chat_id,
            "command": command,
            "args_json": args_json,
            "status": status,
            "error_message": error_message,
        }
        async with httpx.AsyncClient(timeout=10.0) as client:
            response = await client.post(
                f"{self._base_url}/api/telegram/audit",
                headers=self._headers(),
                json=payload,
            )
            response.raise_for_status()
