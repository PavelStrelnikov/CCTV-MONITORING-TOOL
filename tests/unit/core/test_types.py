from cctv_monitor.core.types import (
    DeviceVendor,
    DeviceTransport,
    DeviceStatus,
    CameraStatus,
    DiskStatus,
    RecordingStatus,
    AlertSeverity,
    DeviceId,
    ChannelId,
)


# --- DeviceVendor ---

def test_device_vendor_values():
    assert DeviceVendor.HIKVISION == "hikvision"
    assert DeviceVendor.DAHUA == "dahua"
    assert DeviceVendor.PROVISION == "provision"
    assert DeviceVendor.UNKNOWN == "unknown"


def test_device_vendor_is_str():
    assert isinstance(DeviceVendor.HIKVISION, str)


# --- DeviceTransport ---

def test_device_transport_values():
    assert DeviceTransport.ISAPI == "isapi"
    assert DeviceTransport.SDK == "sdk"
    assert DeviceTransport.AUTO == "auto"


# --- DeviceStatus ---

def test_device_status_values():
    assert DeviceStatus.ONLINE == "online"
    assert DeviceStatus.OFFLINE == "offline"
    assert DeviceStatus.UNREACHABLE == "unreachable"
    assert DeviceStatus.UNKNOWN == "unknown"


# --- CameraStatus ---

def test_camera_status_values():
    assert CameraStatus.ONLINE == "online"
    assert CameraStatus.OFFLINE == "offline"
    assert CameraStatus.UNKNOWN == "unknown"


# --- DiskStatus ---

def test_disk_status_values():
    assert DiskStatus.OK == "ok"
    assert DiskStatus.WARNING == "warning"
    assert DiskStatus.ERROR == "error"
    assert DiskStatus.UNKNOWN == "unknown"


# --- RecordingStatus ---

def test_recording_status_values():
    assert RecordingStatus.RECORDING == "recording"
    assert RecordingStatus.NOT_RECORDING == "not_recording"
    assert RecordingStatus.UNKNOWN == "unknown"


# --- AlertSeverity ---

def test_alert_severity_values():
    assert AlertSeverity.INFO == "info"
    assert AlertSeverity.WARNING == "warning"
    assert AlertSeverity.CRITICAL == "critical"


# --- Serialization ---

def test_enum_serialization_to_json():
    """Enums should serialize to their string value."""
    import json
    data = {"vendor": DeviceVendor.HIKVISION, "status": DeviceStatus.ONLINE}
    result = json.dumps(data)
    assert '"hikvision"' in result
    assert '"online"' in result


def test_enum_comparison_with_string():
    """Enums should be comparable with plain strings."""
    assert DeviceVendor.HIKVISION == "hikvision"
    assert DeviceStatus.ONLINE == "online"
    assert DiskStatus.ERROR == "error"


# --- Type aliases ---

def test_type_aliases():
    """Type aliases should be str."""
    device_id: DeviceId = "nvr-01"
    channel_id: ChannelId = "101"
    assert isinstance(device_id, str)
    assert isinstance(channel_id, str)
