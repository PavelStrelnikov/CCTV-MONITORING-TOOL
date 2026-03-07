"""Application entry point."""

import asyncio
import structlog

logger = structlog.get_logger()


async def main() -> None:
    logger.info("cctv_monitor.starting")
    # TODO: initialize components
    logger.info("cctv_monitor.started")


if __name__ == "__main__":
    asyncio.run(main())
