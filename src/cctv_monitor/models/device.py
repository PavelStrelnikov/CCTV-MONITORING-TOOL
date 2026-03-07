from dataclasses import dataclass
from cctv_monitor.core.types import DeviceVendor, DeviceTransport, DeviceId


@dataclass
class DeviceConfig:
    device_id: DeviceId
    name: str
    vendor: DeviceVendor
    host: str
    port: int
    username: str
    password: str  # encrypted
    transport_mode: DeviceTransport
    polling_policy_id: str
    is_active: bool


@dataclass
class DeviceInfo:
    device_id: DeviceId
    model: str
    serial_number: str
    firmware_version: str
    device_type: str
    mac_address: str | None
    channels_count: int
