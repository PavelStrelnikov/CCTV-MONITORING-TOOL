from collections import defaultdict

import structlog

logger = structlog.get_logger()


class MetricsCollector:
    def __init__(self) -> None:
        self._total_polls = 0
        self._successful_polls = 0
        self._failed_polls = 0
        self._response_times: dict[str, list[float]] = defaultdict(list)

    def record_poll_result(self, device_id: str, check_type: str, success: bool) -> None:
        self._total_polls += 1
        if success:
            self._successful_polls += 1
        else:
            self._failed_polls += 1
        logger.debug("poll.result", device_id=device_id, check_type=check_type, success=success)

    def record_poll_duration(self, device_id: str, duration_ms: float) -> None:
        logger.debug("poll.duration", device_id=device_id, duration_ms=duration_ms)

    def record_device_response_time(self, device_id: str, ms: float) -> None:
        self._response_times[device_id].append(ms)
        logger.debug("device.response_time", device_id=device_id, response_time_ms=ms)

    def get_summary(self) -> dict:
        devices = {}
        for device_id, times in self._response_times.items():
            devices[device_id] = {
                "avg_response_ms": sum(times) / len(times) if times else 0,
                "poll_count": len(times),
            }
        return {
            "total_polls": self._total_polls,
            "successful_polls": self._successful_polls,
            "failed_polls": self._failed_polls,
            "devices": devices,
        }
