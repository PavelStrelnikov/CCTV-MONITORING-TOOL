import pytest
from unittest.mock import AsyncMock
from pathlib import Path
from cctv_monitor.drivers.hikvision.driver import HikvisionDriver
from cctv_monitor.models.device import DeviceConfig
from cctv_monitor.core.types import DeviceVendor, DeviceTransport, CameraStatus, DiskStatus

FIXTURES = Path(__file__).parent.parent.parent.parent / "fixtures" / "hikvision"


@pytest.fixture
def device_config():
    return DeviceConfig(
        device_id="nvr-01",
        name="Test NVR",
        vendor=DeviceVendor.HIKVISION,
        host="192.168.1.100",
        port=80,
        username="admin",
        password="password",
        transport_mode=DeviceTransport.ISAPI,
        polling_policy_id="standard",
        is_active=True,
    )


@pytest.fixture
def mock_transport():
    transport = AsyncMock()
    transport.get_device_info.return_value = {
        "raw_xml": (FIXTURES / "device_info.xml").read_text()
    }
    transport.get_channels_status.return_value = [
        {"raw_xml": (FIXTURES / "channels_status.xml").read_text()}
    ]
    transport.get_disk_status.return_value = [
        {"raw_xml": (FIXTURES / "hdd_status.xml").read_text()}
    ]
    transport.get_snapshot.return_value = b"\xff\xd8\xff\xe0"
    return transport


@pytest.mark.asyncio
async def test_get_device_info(device_config, mock_transport):
    driver = HikvisionDriver(mock_transport)
    await driver.connect(device_config)
    info = await driver.get_device_info()
    assert info.model == "DS-7608NI-K2"
    assert info.device_id == "nvr-01"


@pytest.mark.asyncio
async def test_get_camera_statuses(device_config, mock_transport):
    driver = HikvisionDriver(mock_transport)
    await driver.connect(device_config)
    statuses = await driver.get_camera_statuses()
    assert len(statuses) == 2
    assert statuses[0].status == CameraStatus.ONLINE
    assert statuses[1].status == CameraStatus.OFFLINE


@pytest.mark.asyncio
async def test_get_disk_statuses(device_config, mock_transport):
    driver = HikvisionDriver(mock_transport)
    await driver.connect(device_config)
    disks = await driver.get_disk_statuses()
    assert len(disks) == 1
    assert disks[0].health_status == "ok"


@pytest.mark.asyncio
async def test_get_snapshot(device_config, mock_transport):
    driver = HikvisionDriver(mock_transport)
    await driver.connect(device_config)
    result = await driver.get_snapshot("101")
    assert result.success is True
    assert result.device_id == "nvr-01"


@pytest.mark.asyncio
async def test_check_health(device_config, mock_transport):
    driver = HikvisionDriver(mock_transport)
    await driver.connect(device_config)
    health = await driver.check_health()
    assert health.reachable is True
    assert health.camera_count == 2
    assert health.online_cameras == 1
    assert health.offline_cameras == 1
    assert health.disk_ok is True


@pytest.mark.asyncio
async def test_detect_capabilities(device_config, mock_transport):
    driver = HikvisionDriver(mock_transport)
    await driver.connect(device_config)
    caps = await driver.detect_capabilities()
    assert caps.supports_isapi is True
    assert caps.supports_disk_status is True
    assert caps.model == "DS-7608NI-K2"


@pytest.mark.asyncio
async def test_disconnect(device_config, mock_transport):
    driver = HikvisionDriver(mock_transport)
    await driver.connect(device_config)
    await driver.disconnect()
    mock_transport.disconnect.assert_called_once()
