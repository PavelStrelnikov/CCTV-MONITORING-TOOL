from datetime import datetime
from pydantic import BaseModel

class DeviceCreate(BaseModel):
    device_id: str
    name: str
    vendor: str
    host: str
    port: int = 80
    username: str
    password: str

class HealthSummaryOut(BaseModel):
    reachable: bool
    camera_count: int
    online_cameras: int
    offline_cameras: int
    disk_ok: bool
    response_time_ms: float
    checked_at: datetime

class DeviceOut(BaseModel):
    device_id: str
    name: str
    vendor: str
    host: str
    port: int
    is_active: bool
    last_health: HealthSummaryOut | None = None

class CameraChannelOut(BaseModel):
    channel_id: str
    channel_name: str
    status: str
    ip_address: str | None = None
    checked_at: datetime

class DiskOut(BaseModel):
    disk_id: str
    status: str
    capacity_bytes: int
    free_bytes: int
    health_status: str
    checked_at: datetime

class AlertOut(BaseModel):
    id: int
    alert_type: str
    severity: str
    message: str
    status: str
    created_at: datetime
    resolved_at: datetime | None = None

class DeviceDetailOut(BaseModel):
    device: DeviceOut
    cameras: list[CameraChannelOut] = []
    disks: list[DiskOut] = []
    alerts: list[AlertOut] = []

class PollResultOut(BaseModel):
    device_id: str
    health: HealthSummaryOut

class OverviewOut(BaseModel):
    total_devices: int
    reachable_devices: int
    total_cameras: int
    online_cameras: int
    disks_ok: bool
