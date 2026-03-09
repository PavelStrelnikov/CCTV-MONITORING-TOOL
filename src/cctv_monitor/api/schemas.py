from datetime import datetime
from pydantic import BaseModel

class DeviceCreate(BaseModel):
    device_id: str
    name: str
    vendor: str
    host: str
    web_port: int | None = None
    sdk_port: int | None = None
    username: str
    password: str
    transport_mode: str = "isapi"
    poll_interval_seconds: int | None = None

class DeviceUpdate(BaseModel):
    name: str | None = None
    host: str | None = None
    web_port: int | None = None
    sdk_port: int | None = None
    username: str | None = None
    password: str | None = None
    is_active: bool | None = None
    transport_mode: str | None = None
    poll_interval_seconds: int | None = None


class HealthSummaryOut(BaseModel):
    reachable: bool
    camera_count: int
    online_cameras: int
    offline_cameras: int
    disk_ok: bool
    response_time_ms: float
    checked_at: datetime
    web_port_open: bool | None = None
    sdk_port_open: bool | None = None
    time_check: dict | None = None

class DeviceOut(BaseModel):
    device_id: str
    name: str
    vendor: str
    host: str
    web_port: int | None = None
    sdk_port: int | None = None
    transport_mode: str
    is_active: bool
    last_health: HealthSummaryOut | None = None
    model: str | None = None
    serial_number: str | None = None
    firmware_version: str | None = None
    last_poll_at: datetime | None = None
    poll_interval_seconds: int | None = None
    tags: list[TagOut] = []
    ignored_channels: list[str] = []

class CameraChannelOut(BaseModel):
    channel_id: str
    channel_name: str
    status: str
    ip_address: str | None = None
    recording: str | None = None
    checked_at: datetime

class DiskOut(BaseModel):
    disk_id: str
    status: str
    capacity_bytes: int
    free_bytes: int
    health_status: str
    checked_at: datetime
    temperature: int | None = None
    power_on_hours: int | None = None
    smart_status: str | None = None

class AlertOut(BaseModel):
    id: int
    device_id: str
    device_name: str = ""
    alert_type: str
    severity: str
    message: str
    status: str
    created_at: datetime
    resolved_at: datetime | None = None

class HealthLogEntryOut(BaseModel):
    reachable: bool
    camera_count: int
    online_cameras: int
    offline_cameras: int
    disk_ok: bool
    response_time_ms: float
    checked_at: datetime

class TagCreate(BaseModel):
    tag: str

class TagOut(BaseModel):
    name: str
    color: str = "#6366F1"

class TagDefinitionUpdate(BaseModel):
    name: str | None = None
    color: str | None = None

class DeviceDetailOut(BaseModel):
    device: DeviceOut
    cameras: list[CameraChannelOut] = []
    disks: list[DiskOut] = []
    alerts: list[AlertOut] = []
    health: HealthSummaryOut | None = None

class PollResultOut(BaseModel):
    device_id: str
    health: HealthSummaryOut

class PollLogEntryOut(BaseModel):
    device_id: str
    device_name: str
    reachable: bool
    camera_count: int
    online_cameras: int
    offline_cameras: int
    disk_ok: bool
    response_time_ms: float
    checked_at: datetime


class OverviewDeviceSummary(BaseModel):
    device_id: str
    name: str
    reachable: bool
    camera_count: int
    online_cameras: int
    offline_cameras: int
    disk_ok: bool
    recording_total: int
    recording_ok: int
    time_drift: int | None = None
    last_poll_at: datetime | None = None

class OverviewOut(BaseModel):
    total_devices: int
    reachable_devices: int
    unreachable_devices: int
    total_cameras: int
    online_cameras: int
    offline_cameras: int
    total_disks: int
    disks_ok_count: int
    disks_error_count: int
    disks_ok: bool
    recording_total: int
    recording_ok: int
    time_drift_issues: int
    devices: list[OverviewDeviceSummary] = []
