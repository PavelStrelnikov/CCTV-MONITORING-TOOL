from dataclasses import dataclass, field
from datetime import datetime
from cctv_monitor.core.types import DeviceId


@dataclass
class DeviceCheckResult:
    device_id: DeviceId
    check_type: str
    success: bool
    duration_ms: float
    checked_at: datetime
    id: int | None = None
    error_type: str | None = None
    payload_json: dict | None = field(default=None)
