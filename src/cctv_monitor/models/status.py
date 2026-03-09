from dataclasses import dataclass
from datetime import datetime
from cctv_monitor.core.types import DeviceId, ChannelId, CameraStatus as CameraStatusEnum, DiskStatus, RecordingStatus as RecordingStatusEnum


@dataclass
class CameraChannelStatus:
    device_id: DeviceId
    channel_id: ChannelId
    channel_name: str
    status: CameraStatusEnum
    ip_address: str | None
    protocol: str | None
    checked_at: datetime


@dataclass
class DiskHealthStatus:
    device_id: DeviceId
    disk_id: str
    status: DiskStatus
    capacity_bytes: int
    free_bytes: int
    health_status: str
    checked_at: datetime
    temperature: int | None = None
    power_on_hours: int | None = None
    smart_status: str | None = None


@dataclass
class ChannelRecordingStatus:
    device_id: DeviceId
    channel_id: ChannelId
    status: RecordingStatusEnum
    record_type: str | None
    checked_at: datetime
