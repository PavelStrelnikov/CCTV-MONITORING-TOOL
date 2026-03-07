from cctv_monitor.models.device_health import DeviceHealthSummary
from cctv_monitor.core.types import AlertType, AlertSeverity


def check_device_unreachable(health: DeviceHealthSummary) -> tuple[AlertType, AlertSeverity, str] | None:
    if not health.reachable:
        return (AlertType.DEVICE_UNREACHABLE, AlertSeverity.CRITICAL, f"Device {health.device_id} is unreachable")
    return None


def check_camera_offline(health: DeviceHealthSummary) -> tuple[AlertType, AlertSeverity, str] | None:
    if health.offline_cameras > 0:
        return (AlertType.CAMERA_OFFLINE, AlertSeverity.WARNING, f"{health.offline_cameras} camera(s) offline on {health.device_id}")
    return None


def check_disk_error(health: DeviceHealthSummary) -> tuple[AlertType, AlertSeverity, str] | None:
    if not health.disk_ok:
        return (AlertType.DISK_ERROR, AlertSeverity.CRITICAL, f"Disk error on {health.device_id}")
    return None


ALL_RULES = [check_device_unreachable, check_camera_offline, check_disk_error]
