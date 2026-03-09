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
