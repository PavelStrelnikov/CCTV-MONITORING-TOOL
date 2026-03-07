from dataclasses import dataclass
from datetime import datetime
from cctv_monitor.core.types import DeviceId


@dataclass
class DeviceHealthSummary:
    device_id: DeviceId
    reachable: bool
    camera_count: int
    online_cameras: int
    offline_cameras: int
    disk_ok: bool
    recording_ok: bool
    response_time_ms: float
    checked_at: datetime
