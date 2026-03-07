from enum import StrEnum
from typing import TypeAlias


# Type aliases
DeviceId: TypeAlias = str
ChannelId: TypeAlias = str


class DeviceVendor(StrEnum):
    HIKVISION = "hikvision"
    DAHUA = "dahua"
    PROVISION = "provision"
    UNKNOWN = "unknown"


class DeviceTransport(StrEnum):
    ISAPI = "isapi"
    SDK = "sdk"
    AUTO = "auto"


class DeviceStatus(StrEnum):
    ONLINE = "online"
    OFFLINE = "offline"
    UNREACHABLE = "unreachable"
    UNKNOWN = "unknown"


class CameraStatus(StrEnum):
    ONLINE = "online"
    OFFLINE = "offline"
    UNKNOWN = "unknown"


class DiskStatus(StrEnum):
    OK = "ok"
    WARNING = "warning"
    ERROR = "error"
    UNKNOWN = "unknown"


class RecordingStatus(StrEnum):
    RECORDING = "recording"
    NOT_RECORDING = "not_recording"
    UNKNOWN = "unknown"


class AlertSeverity(StrEnum):
    INFO = "info"
    WARNING = "warning"
    CRITICAL = "critical"
