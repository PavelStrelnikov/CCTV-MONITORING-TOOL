from dataclasses import dataclass
from datetime import datetime
from cctv_monitor.core.types import DeviceId, ChannelId, AlertSeverity


@dataclass
class AlertEvent:
    device_id: DeviceId
    alert_type: str
    severity: AlertSeverity
    message: str
    source: str
    status: str  # "active" or "resolved"
    created_at: datetime
    id: int | None = None
    channel_id: ChannelId | None = None
    resolved_at: datetime | None = None
