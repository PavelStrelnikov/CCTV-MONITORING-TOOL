import pytest
from unittest.mock import AsyncMock
from datetime import datetime, timezone
from cctv_monitor.polling.jobs import poll_device_health
from cctv_monitor.models.device import DeviceConfig
from cctv_monitor.models.device_health import DeviceHealthSummary
from cctv_monitor.core.types import DeviceVendor, DeviceTransport, CheckType
from cctv_monitor.metrics.collector import MetricsCollector


@pytest.fixture
def device_config():
    return DeviceConfig(
        device_id="nvr-01", name="Test NVR", vendor=DeviceVendor.HIKVISION,
        host="192.168.1.100", port=80, username="admin", password="password",
        transport_mode=DeviceTransport.ISAPI, polling_policy_id="standard", is_active=True,
    )


@pytest.fixture
def mock_driver():
    driver = AsyncMock()
    driver.check_health.return_value = DeviceHealthSummary(
        device_id="nvr-01", reachable=True, camera_count=4, online_cameras=4,
        offline_cameras=0, disk_ok=True, recording_ok=True, response_time_ms=50.0,
        checked_at=datetime.now(timezone.utc),
    )
    return driver


@pytest.fixture
def metrics():
    return MetricsCollector()


async def test_poll_device_health_success(device_config, mock_driver, metrics):
    result = await poll_device_health(mock_driver, device_config, metrics)
    assert result.success is True
    assert result.check_type == CheckType.DEVICE_INFO
    assert result.device_id == "nvr-01"
    mock_driver.connect.assert_called_once_with(device_config)
    mock_driver.disconnect.assert_called_once()


async def test_poll_device_health_failure(device_config, metrics):
    driver = AsyncMock()
    driver.connect.side_effect = ConnectionError("unreachable")
    result = await poll_device_health(driver, device_config, metrics)
    assert result.success is False
    assert result.error_type == "ConnectionError"
    driver.disconnect.assert_called_once()


async def test_poll_records_metrics(device_config, mock_driver, metrics):
    await poll_device_health(mock_driver, device_config, metrics)
    summary = metrics.get_summary()
    assert summary["total_polls"] == 1
    assert summary["successful_polls"] == 1
