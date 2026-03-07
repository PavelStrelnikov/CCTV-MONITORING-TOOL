from datetime import datetime, timezone
from cctv_monitor.api.schemas import (
    DeviceCreate, DeviceOut, HealthSummaryOut, CameraChannelOut,
    DiskOut, DeviceDetailOut, PollResultOut, OverviewOut,
)

def test_device_create_valid():
    d = DeviceCreate(device_id="nvr-01", name="Test NVR", vendor="hikvision",
        host="192.168.1.100", port=80, username="admin", password="secret")
    assert d.device_id == "nvr-01"
    assert d.port == 80

def test_device_create_default_port():
    d = DeviceCreate(device_id="nvr-01", name="Test", vendor="hikvision",
        host="10.0.0.1", username="admin", password="pass")
    assert d.port == 80

def test_device_out_with_health():
    now = datetime.now(timezone.utc)
    h = HealthSummaryOut(reachable=True, camera_count=4, online_cameras=3,
        offline_cameras=1, disk_ok=True, response_time_ms=120.5, checked_at=now)
    d = DeviceOut(device_id="nvr-01", name="Test", vendor="hikvision",
        host="10.0.0.1", port=80, is_active=True, last_health=h)
    assert d.last_health.camera_count == 4

def test_device_out_without_health():
    d = DeviceOut(device_id="nvr-01", name="Test", vendor="hikvision",
        host="10.0.0.1", port=80, is_active=True, last_health=None)
    assert d.last_health is None

def test_overview_out():
    o = OverviewOut(total_devices=7, reachable_devices=5, total_cameras=20,
        online_cameras=18, disks_ok=True)
    assert o.total_devices == 7
