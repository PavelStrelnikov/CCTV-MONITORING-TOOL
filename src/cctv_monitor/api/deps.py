import hmac
from collections.abc import AsyncGenerator

from fastapi import Depends, Header, HTTPException, Request
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


def get_sdk_binding(request: Request):
    """Return SDK binding, initializing lazily on first call."""
    binding = getattr(request.app.state, "sdk_binding", None)
    if binding is not None:
        return binding

    lib_path = getattr(request.app.state, "sdk_lib_path", None)
    if not lib_path:
        return None

    import structlog
    from cctv_monitor.drivers.hikvision.transports.sdk_bindings import HCNetSDKBinding

    log = structlog.get_logger()
    try:
        binding = HCNetSDKBinding(lib_path=lib_path)
        binding.init()
        request.app.state.sdk_binding = binding
        log.info("sdk.initialized_lazy", path=lib_path)
    except Exception as exc:
        log.warning("sdk.init_failed", error=str(exc))
        # Clear lib_path so we don't retry on every request
        request.app.state.sdk_lib_path = None
        return None

    return binding


def get_device_repo(session: AsyncSession = Depends(get_session)) -> DeviceRepository:
    return DeviceRepository(session)


def verify_internal_api_token(
    request: Request,
    x_internal_token: str | None = Header(default=None, alias="X-Internal-Token"),
) -> None:
    settings: Settings = request.app.state.settings
    expected = settings.INTERNAL_API_TOKEN
    if not expected:
        raise HTTPException(status_code=503, detail="Internal API token is not configured")
    if not hmac.compare_digest(x_internal_token or "", expected):
        raise HTTPException(status_code=401, detail="Invalid internal token")
