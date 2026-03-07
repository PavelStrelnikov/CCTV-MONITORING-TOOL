from dataclasses import dataclass


@dataclass
class PollingPolicy:
    name: str
    device_info_interval: int
    camera_status_interval: int
    disk_status_interval: int
    snapshot_interval: int
