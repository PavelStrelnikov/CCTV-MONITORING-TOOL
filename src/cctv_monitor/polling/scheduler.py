import structlog
from apscheduler.schedulers.asyncio import AsyncIOScheduler

logger = structlog.get_logger()


def create_scheduler() -> AsyncIOScheduler:
    scheduler = AsyncIOScheduler(
        job_defaults={
            "coalesce": True,
            "max_instances": 1,
            "misfire_grace_time": 60,
        }
    )
    return scheduler
