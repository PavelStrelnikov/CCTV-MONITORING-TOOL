import pytest
from datetime import datetime, timezone
from cctv_monitor.alerts.engine import AlertEngine
from cctv_monitor.models.device_health import DeviceHealthSummary
from cctv_monitor.models.alert import AlertEvent
from cctv_monitor.core.types import AlertType, AlertSeverity, AlertStatus


@pytest.fixture
def engine():
    return AlertEngine()


def _make_health(reachable=True, online=4, offline=0, disk_ok=True, recording_ok=True):
    return DeviceHealthSummary(
        device_id="nvr-01", reachable=reachable, camera_count=online + offline,
        online_cameras=online, offline_cameras=offline, disk_ok=disk_ok,
        recording_ok=recording_ok, response_time_ms=100.0,
        checked_at=datetime.now(timezone.utc),
    )


def test_no_alerts_when_healthy(engine):
    new_alerts, resolved = engine.evaluate(_make_health(), active_alerts=[])
    assert len(new_alerts) == 0
    assert len(resolved) == 0


def test_alert_on_device_unreachable(engine):
    new_alerts, resolved = engine.evaluate(_make_health(reachable=False), active_alerts=[])
    assert len(new_alerts) == 1
    assert new_alerts[0].alert_type == AlertType.DEVICE_UNREACHABLE


def test_alert_on_camera_offline(engine):
    new_alerts, resolved = engine.evaluate(_make_health(offline=2), active_alerts=[])
    assert any(a.alert_type == AlertType.CAMERA_OFFLINE for a in new_alerts)


def test_no_duplicate_alert(engine):
    existing = AlertEvent(
        id=1, device_id="nvr-01", alert_type=AlertType.DEVICE_UNREACHABLE,
        severity=AlertSeverity.CRITICAL, message="Device unreachable",
        source="polling", status=AlertStatus.ACTIVE, created_at=datetime.now(timezone.utc),
    )
    new_alerts, resolved = engine.evaluate(_make_health(reachable=False), active_alerts=[existing])
    assert len(new_alerts) == 0
    assert len(resolved) == 0


def test_resolve_alert_when_recovered(engine):
    existing = AlertEvent(
        id=1, device_id="nvr-01", alert_type=AlertType.DEVICE_UNREACHABLE,
        severity=AlertSeverity.CRITICAL, message="Device unreachable",
        source="polling", status=AlertStatus.ACTIVE, created_at=datetime.now(timezone.utc),
    )
    new_alerts, resolved = engine.evaluate(_make_health(reachable=True), active_alerts=[existing])
    assert len(resolved) == 1
    assert resolved[0].id == 1


def test_disk_error_alert(engine):
    new_alerts, resolved = engine.evaluate(_make_health(disk_ok=False), active_alerts=[])
    assert any(a.alert_type == AlertType.DISK_ERROR for a in new_alerts)
