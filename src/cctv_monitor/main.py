"""Application entry point."""

import asyncio

import structlog
import uvicorn

from cctv_monitor.api.app import create_app
from cctv_monitor.core.config import Settings
from cctv_monitor.core.http_client import HttpClientManager
from cctv_monitor.core.types import DeviceVendor
from cctv_monitor.drivers.hikvision.driver import HikvisionDriver
from cctv_monitor.drivers.registry import DriverRegistry
from cctv_monitor.metrics.collector import MetricsCollector
from cctv_monitor.polling.scheduler import create_scheduler
from cctv_monitor.storage.database import create_engine, create_session_factory

logger = structlog.get_logger()


async def main() -> None:
    structlog.configure(
        wrapper_class=structlog.make_filtering_bound_logger(20),
    )

    logger.info("cctv_monitor.starting")

    settings = Settings()  # type: ignore[call-arg]
    http_client = HttpClientManager()
    metrics = MetricsCollector()  # noqa: F841 — used by polling jobs

    # Database
    engine = create_engine(settings.database_url)
    session_factory = create_session_factory(engine)  # noqa: F841 — used by repositories

    # Driver registry
    registry = DriverRegistry()
    registry.register(DeviceVendor.HIKVISION, HikvisionDriver)

    # Scheduler
    scheduler = create_scheduler()
    scheduler.start()

    # API
    app = create_app()

    logger.info("cctv_monitor.started")

    config = uvicorn.Config(app, host="0.0.0.0", port=8000, log_level="info")
    server = uvicorn.Server(config)
    try:
        await server.serve()
    finally:
        scheduler.shutdown()
        await http_client.close()
        await engine.dispose()
        logger.info("cctv_monitor.stopped")


if __name__ == "__main__":
    asyncio.run(main())
