from dataclasses import dataclass
from datetime import datetime
from cctv_monitor.core.types import DeviceId, ChannelId


@dataclass
class SnapshotResult:
    device_id: DeviceId
    channel_id: ChannelId
    success: bool
    checked_at: datetime
    file_path: str | None = None
    file_size_bytes: int | None = None
    error: str | None = None
