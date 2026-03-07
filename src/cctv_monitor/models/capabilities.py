from dataclasses import dataclass
from datetime import datetime
from cctv_monitor.core.types import DeviceId


@dataclass
class DeviceCapabilities:
    device_id: DeviceId
    model: str
    firmware_version: str
    supports_isapi: bool
    supports_sdk: bool
    supports_snapshot: bool
    supports_recording_status: bool
    supports_disk_status: bool
    detected_at: datetime
