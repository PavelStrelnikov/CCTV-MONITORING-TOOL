"""Tests for background polling job."""

from contextlib import asynccontextmanager
from unittest.mock import AsyncMock, MagicMock

import pytest

from cctv_monitor.polling.background import poll_all_devices


@pytest.mark.asyncio
async def test_poll_all_devices_no_active_devices():
    """poll_all_devices completes without error when there are no active devices."""

    mock_session = AsyncMock()
    mock_session.execute = AsyncMock(
        return_value=MagicMock(
            scalars=MagicMock(return_value=MagicMock(all=MagicMock(return_value=[])))
        )
    )

    @asynccontextmanager
    async def session_factory():
        yield mock_session

    settings = MagicMock()
    registry = MagicMock()
    http_client = MagicMock()
    sdk_binding_getter = MagicMock(return_value=None)

    # Should complete without raising
    await poll_all_devices(session_factory, settings, registry, http_client, sdk_binding_getter)
