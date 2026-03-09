from datetime import datetime, timezone
from cctv_monitor.api.schemas import (
    DeviceOut, HealthSummaryOut, HealthLogEntryOut, TagCreate, DeviceDetailOut,
)


def test_device_out_with_v2_fields():
    now = datetime.now(timezone.utc)
    d = DeviceOut(
        device_id="nvr-01", name="Test NVR", vendor="hikvision",
        host="10.0.0.1", web_port=80, sdk_port=8000,
        transport_mode="isapi", is_active=True,
        model="DS-7608NI-K2", serial_number="SN123456",
        firmware_version="V4.30.085", last_poll_at=now,
        tags=["office", "main"],
    )
    assert d.model == "DS-7608NI-K2"
    assert d.serial_number == "SN123456"
    assert d.firmware_version == "V4.30.085"
    assert d.last_poll_at == now
    assert d.tags == ["office", "main"]


def test_device_out_v2_defaults():
    d = DeviceOut(
        device_id="nvr-02", name="Test", vendor="hikvision",
        host="10.0.0.2", transport_mode="isapi", is_active=True,
    )
    assert d.model is None
    assert d.serial_number is None
    assert d.firmware_version is None
    assert d.last_poll_at is None
    assert d.tags == []


def test_health_log_entry_out():
    now = datetime.now(timezone.utc)
    entry = HealthLogEntryOut(
        reachable=True, camera_count=8, online_cameras=7,
        offline_cameras=1, disk_ok=True, response_time_ms=55.3,
        checked_at=now,
    )
    assert entry.reachable is True
    assert entry.camera_count == 8
    assert entry.online_cameras == 7
    assert entry.offline_cameras == 1
    assert entry.disk_ok is True
    assert entry.response_time_ms == 55.3
    assert entry.checked_at == now


def test_tag_create():
    t = TagCreate(tag="office")
    assert t.tag == "office"


def test_device_detail_out_with_health():
    now = datetime.now(timezone.utc)
    health = HealthSummaryOut(
        reachable=True, camera_count=4, online_cameras=4,
        offline_cameras=0, disk_ok=True, response_time_ms=42.0,
        checked_at=now,
    )
    detail = DeviceDetailOut(
        device=DeviceOut(
            device_id="nvr-01", name="Test", vendor="hikvision",
            host="10.0.0.1", transport_mode="isapi", is_active=True,
        ),
        health=health,
    )
    assert detail.health is not None
    assert detail.health.reachable is True


def test_device_detail_out_health_default_none():
    detail = DeviceDetailOut(
        device=DeviceOut(
            device_id="nvr-01", name="Test", vendor="hikvision",
            host="10.0.0.1", transport_mode="isapi", is_active=True,
        ),
    )
    assert detail.health is None
