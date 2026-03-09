"""Application entry point."""

import asyncio
import faulthandler
import sys

import structlog
import uvicorn

# Enable faulthandler so native DLL crashes (SIGSEGV etc.) produce a traceback
faulthandler.enable(file=sys.stderr, all_threads=True)

from cctv_monitor.api.app import create_app
from cctv_monitor.core.config import Settings
from cctv_monitor.core.http_client import HttpClientManager
from cctv_monitor.core.types import DeviceVendor
from cctv_monitor.drivers.hikvision.driver import HikvisionDriver
from cctv_monitor.drivers.registry import DriverRegistry
from cctv_monitor.metrics.collector import MetricsCollector
from cctv_monitor.polling.scheduler import create_scheduler
from cctv_monitor.storage.database import create_engine, create_session_factory
from cctv_monitor.storage.tables import PollingPolicyTable

logger = structlog.get_logger()


async def _ensure_default_policy(session_factory) -> None:
    """Insert default polling policy if it doesn't exist."""
    from sqlalchemy import select
    async with session_factory() as session:
        result = await session.execute(
            select(PollingPolicyTable).where(PollingPolicyTable.name == "standard")
        )
        if result.scalar_one_or_none() is None:
            session.add(PollingPolicyTable(name="standard"))
            await session.commit()
            logger.info("seed.default_policy_created")


async def main() -> None:
    structlog.configure(
        wrapper_class=structlog.make_filtering_bound_logger(20),
    )

    logger.info("cctv_monitor.starting")

    settings = Settings()  # type: ignore[call-arg]
    http_client = HttpClientManager()
    metrics = MetricsCollector()

    # Database
    engine = create_engine(settings.database_url)
    session_factory = create_session_factory(engine)

    # Ensure default polling policy exists
    await _ensure_default_policy(session_factory)

    # Driver registry
    registry = DriverRegistry()
    registry.register(DeviceVendor.HIKVISION, HikvisionDriver)

    # Scheduler
    scheduler = create_scheduler()
    scheduler.start()

    # API
    app = create_app()

    # Store shared state on app for dependency injection
    app.state.settings = settings
    app.state.session_factory = session_factory
    app.state.driver_registry = registry
    app.state.http_client = http_client
    app.state.metrics = metrics
    app.state.scheduler = scheduler
    # SDK is initialized lazily on first SDK poll request (see deps.py).
    # Loading the DLL eagerly spawns internal threads that can destabilize
    # the process even during ISAPI-only polls.
    app.state.sdk_binding = None
    app.state.sdk_lib_path = settings.HCNETSDK_LIB_PATH

    # Background polling job
    from cctv_monitor.polling.background import poll_all_devices

    def _get_sdk_binding():
        return getattr(app.state, "sdk_binding", None)

    scheduler.add_job(
        poll_all_devices,
        "interval",
        seconds=30,
        args=[session_factory, settings, registry, http_client, _get_sdk_binding],
        id="poll_all_devices",
        replace_existing=True,
    )
    logger.info("scheduler.job_registered", job="poll_all_devices", interval_sec=30)

    logger.info("cctv_monitor.started")

    config = uvicorn.Config(app, host="0.0.0.0", port=8001, log_level="info")
    server = uvicorn.Server(config)
    try:
        await server.serve()
    finally:
        # NOTE: SDK cleanup is intentionally skipped — see sdk_bindings.py.
        scheduler.shutdown()
        await http_client.close()
        await engine.dispose()
        logger.info("cctv_monitor.stopped")


if __name__ == "__main__":
    asyncio.run(main())
