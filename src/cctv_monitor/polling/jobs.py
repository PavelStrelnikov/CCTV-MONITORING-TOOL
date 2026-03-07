import time
from datetime import datetime, timezone

import structlog

from cctv_monitor.models.device import DeviceConfig
from cctv_monitor.models.check_result import DeviceCheckResult
from cctv_monitor.core.types import CheckType
from cctv_monitor.metrics.collector import MetricsCollector

logger = structlog.get_logger()


async def poll_device_health(driver, config: DeviceConfig, metrics: MetricsCollector) -> DeviceCheckResult:
    start = time.monotonic()
    now = datetime.now(timezone.utc)
    try:
        await driver.connect(config)
        health = await driver.check_health()
        duration = (time.monotonic() - start) * 1000

        metrics.record_poll_result(config.device_id, "health_check", success=True)
        metrics.record_device_response_time(config.device_id, health.response_time_ms)
        metrics.record_poll_duration(config.device_id, duration)

        logger.info("poll.health.ok", device_id=config.device_id, reachable=health.reachable,
                     cameras=health.camera_count, online=health.online_cameras, duration_ms=round(duration, 1))

        return DeviceCheckResult(
            device_id=config.device_id, check_type=CheckType.DEVICE_INFO,
            success=True, duration_ms=duration, checked_at=now,
        )
    except Exception as exc:
        duration = (time.monotonic() - start) * 1000
        metrics.record_poll_result(config.device_id, "health_check", success=False)

        logger.error("poll.health.error", device_id=config.device_id, error=str(exc), duration_ms=round(duration, 1))

        return DeviceCheckResult(
            device_id=config.device_id, check_type=CheckType.DEVICE_INFO,
            success=False, error_type=type(exc).__name__, duration_ms=duration, checked_at=now,
        )
    finally:
        try:
            await driver.disconnect()
        except Exception:
            pass
