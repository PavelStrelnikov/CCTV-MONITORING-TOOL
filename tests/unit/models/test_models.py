from datetime import datetime, timezone
from cctv_monitor.models.device import DeviceConfig, DeviceInfo
from cctv_monitor.models.status import CameraChannelStatus, DiskHealthStatus, ChannelRecordingStatus
from cctv_monitor.models.device_health import DeviceHealthSummary
from cctv_monitor.models.check_result import DeviceCheckResult
from cctv_monitor.models.capabilities import DeviceCapabilities
from cctv_monitor.models.polling_policy import PollingPolicy
from cctv_monitor.models.snapshot import SnapshotResult
from cctv_monitor.models.alert import AlertEvent
from cctv_monitor.core.types import (
    DeviceVendor, DeviceTransport, CameraStatus, DiskStatus,
    RecordingStatus, AlertSeverity,
)


def test_device_config_creation():
    config = DeviceConfig(
        device_id="nvr-01",
        name="Office NVR",
        vendor=DeviceVendor.HIKVISION,
        host="192.168.1.100",
        web_port=80,
        sdk_port=None,
        username="admin",
        password="encrypted_password",
        transport_mode=DeviceTransport.ISAPI,
        polling_policy_id="standard",
        is_active=True,
    )
    assert config.device_id == "nvr-01"
    assert config.vendor == DeviceVendor.HIKVISION
    assert config.transport_mode == DeviceTransport.ISAPI


def test_device_info_creation():
    info = DeviceInfo(
        device_id="nvr-01",
        model="DS-7608NI-K2",
        serial_number="ABC123",
        firmware_version="V4.30.085",
        device_type="NVR",
        mac_address="aa:bb:cc:dd:ee:ff",
        channels_count=8,
    )
    assert info.model == "DS-7608NI-K2"
    assert info.mac_address == "aa:bb:cc:dd:ee:ff"


def test_camera_channel_status():
    status = CameraChannelStatus(
        device_id="nvr-01",
        channel_id="101",
        channel_name="Front Door",
        status=CameraStatus.ONLINE,
        ip_address="192.168.1.10",
        protocol="TCP",
        checked_at=datetime.now(timezone.utc),
    )
    assert status.status == CameraStatus.ONLINE
    assert status.channel_id == "101"


def test_disk_health_status():
    disk = DiskHealthStatus(
        device_id="nvr-01",
        disk_id="1",
        status=DiskStatus.OK,
        capacity_bytes=2000 * 1024 * 1024,
        free_bytes=500 * 1024 * 1024,
        health_status="ok",
        checked_at=datetime.now(timezone.utc),
    )
    assert disk.status == DiskStatus.OK
    assert disk.capacity_bytes > disk.free_bytes


def test_channel_recording_status():
    rec = ChannelRecordingStatus(
        device_id="nvr-01",
        channel_id="101",
        status=RecordingStatus.RECORDING,
        record_type="continuous",
        checked_at=datetime.now(timezone.utc),
    )
    assert rec.status == RecordingStatus.RECORDING


def test_device_health_summary():
    summary = DeviceHealthSummary(
        device_id="nvr-01",
        reachable=True,
        camera_count=4,
        online_cameras=3,
        offline_cameras=1,
        disk_ok=True,
        recording_ok=True,
        response_time_ms=150.5,
        checked_at=datetime.now(timezone.utc),
    )
    assert summary.offline_cameras == 1
    assert summary.reachable is True


def test_device_check_result():
    result = DeviceCheckResult(
        device_id="nvr-01",
        check_type="device_info",
        success=True,
        duration_ms=120.5,
        checked_at=datetime.now(timezone.utc),
        payload_json={"model": "DS-7608NI-K2"},
    )
    assert result.success is True
    assert result.error_type is None


def test_device_capabilities():
    caps = DeviceCapabilities(
        device_id="nvr-01",
        model="DS-7608NI-K2",
        firmware_version="V4.30.085",
        supports_isapi=True,
        supports_sdk=False,
        supports_snapshot=True,
        supports_recording_status=False,
        supports_disk_status=True,
        detected_at=datetime.now(timezone.utc),
    )
    assert caps.supports_isapi is True
    assert caps.supports_sdk is False


def test_polling_policy():
    policy = PollingPolicy(
        name="standard",
        device_info_interval=300,
        camera_status_interval=120,
        disk_status_interval=600,
        snapshot_interval=900,
    )
    assert policy.name == "standard"
    assert policy.camera_status_interval == 120


def test_snapshot_result_success():
    snap = SnapshotResult(
        device_id="nvr-01",
        channel_id="101",
        success=True,
        checked_at=datetime.now(timezone.utc),
        file_path="/data/snapshots/nvr-01/101/20260307.jpg",
        file_size_bytes=45000,
    )
    assert snap.success is True
    assert snap.error is None


def test_snapshot_result_failure():
    snap = SnapshotResult(
        device_id="nvr-01",
        channel_id="101",
        success=False,
        checked_at=datetime.now(timezone.utc),
        error="Camera offline",
    )
    assert snap.success is False
    assert snap.file_path is None


def test_alert_event():
    alert = AlertEvent(
        device_id="nvr-01",
        channel_id="101",
        alert_type="camera_offline",
        severity=AlertSeverity.CRITICAL,
        message="Camera Front Door is offline",
        source="polling",
        status="active",
        created_at=datetime.now(timezone.utc),
    )
    assert alert.status == "active"
    assert alert.resolved_at is None
    assert alert.severity == AlertSeverity.CRITICAL
