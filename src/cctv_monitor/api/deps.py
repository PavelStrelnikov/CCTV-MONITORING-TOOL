from collections.abc import AsyncGenerator

from fastapi import Depends, Request
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.core.config import Settings
from cctv_monitor.drivers.registry import DriverRegistry
from cctv_monitor.core.http_client import HttpClientManager
from cctv_monitor.storage.repositories import DeviceRepository


async def get_session(request: Request) -> AsyncGenerator[AsyncSession, None]:
    session_factory = request.app.state.session_factory
    async with session_factory() as session:
        yield session


def get_settings(request: Request) -> Settings:
    return request.app.state.settings


def get_driver_registry(request: Request) -> DriverRegistry:
    return request.app.state.driver_registry


def get_http_client(request: Request) -> HttpClientManager:
    return request.app.state.http_client


def get_device_repo(session: AsyncSession = Depends(get_session)) -> DeviceRepository:
    return DeviceRepository(session)
