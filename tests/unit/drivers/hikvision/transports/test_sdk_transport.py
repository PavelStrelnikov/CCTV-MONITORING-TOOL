"""Tests for SdkTransport (sdk.py)."""

from __future__ import annotations

from unittest.mock import MagicMock, patch

import pytest

from cctv_monitor.drivers.hikvision.transports.sdk import SdkTransport


@pytest.fixture(autouse=True)
def _skip_tcp_probe():
    """Skip TCP probe in all SDK transport tests."""
    with patch.object(SdkTransport, "_tcp_probe", return_value=True):
        yield


@pytest.fixture()
def login_info() -> dict:
    return {
        "serial_number": "DS-1234567890",
        "disk_num": 2,
        "chan_num": 0,
        "ip_chan_num": 8,
        "start_chan": 1,
        "start_dchan": 33,
        "dvr_type": 0,
        "alarm_in_num": 0,
        "alarm_out_num": 0,
        "audio_chan_num": 0,
        "zero_chan_num": 0,
        "password_level": 1,
    }


@pytest.fixture()
def mock_binding(login_info: dict) -> MagicMock:
    binding = MagicMock()
    binding.login.return_value = (1, login_info)
    binding.logout.return_value = None
    binding.get_device_config.return_value = {
        "device_name": "TestNVR",
        "serial_number": "DS-1234567890",
        "software_version": "4.1.0",
        "software_build_date": "2024-01-15",
        "dsp_version": "1.0.0",
        "dsp_build_date": "2024-01-15",
        "panel_version": 0,
        "hardware_version": 0,
        "disk_num": 2,
        "chan_num": 0,
        "ip_chan_num": 8,
        "start_chan": 1,
        "dvr_type": 0,
        "device_type_name": "NVR",
    }
    binding.get_hdd_config.return_value = [
        {
            "hd_no": 1,
            "capacity_mb": 1000000,
            "free_space_mb": 500000,
            "status_code": 0,
            "status": "normal",
            "hd_attr": 0,
            "hd_type": 0,
            "recycling": True,
            "storage_type": 0,
        },
        {
            "hd_no": 2,
            "capacity_mb": 2000000,
            "free_space_mb": 1500000,
            "status_code": 0,
            "status": "normal",
            "hd_attr": 0,
            "hd_type": 0,
            "recycling": False,
            "storage_type": 0,
        },
    ]
    return binding


@pytest.mark.asyncio
async def test_connect_calls_login(mock_binding: MagicMock) -> None:
    transport = SdkTransport(binding=mock_binding)
    await transport.connect("192.168.1.100", 8000, "admin", "password123")

    mock_binding.login.assert_called_once_with(
        "192.168.1.100", 8000, "admin", "password123"
    )
    assert transport._user_id == 1
    assert transport._login_info["serial_number"] == "DS-1234567890"


@pytest.mark.asyncio
async def test_disconnect_calls_logout(mock_binding: MagicMock) -> None:
    transport = SdkTransport(binding=mock_binding)
    await transport.connect("192.168.1.100", 8000, "admin", "password123")
    await transport.disconnect()

    mock_binding.logout.assert_called_once_with(1)
    assert transport._user_id == -1


@pytest.mark.asyncio
async def test_disconnect_skips_if_not_connected(mock_binding: MagicMock) -> None:
    transport = SdkTransport(binding=mock_binding)
    await transport.disconnect()

    mock_binding.logout.assert_not_called()


@pytest.mark.asyncio
async def test_get_device_info_returns_sdk_data(mock_binding: MagicMock) -> None:
    transport = SdkTransport(binding=mock_binding)
    await transport.connect("192.168.1.100", 8000, "admin", "password123")

    result = await transport.get_device_info()

    assert "sdk_data" in result
    assert result["sdk_data"]["device_name"] == "TestNVR"
    assert result["sdk_data"]["serial_number"] == "DS-1234567890"
    mock_binding.get_device_config.assert_called_once_with(1)


@pytest.mark.asyncio
async def test_get_disk_status_returns_list(mock_binding: MagicMock) -> None:
    transport = SdkTransport(binding=mock_binding)
    await transport.connect("192.168.1.100", 8000, "admin", "password123")

    result = await transport.get_disk_status()

    assert isinstance(result, list)
    assert len(result) == 2
    assert "sdk_data" in result[0]
    assert result[0]["sdk_data"]["capacity_mb"] == 1000000
    assert result[1]["sdk_data"]["free_space_mb"] == 1500000
    mock_binding.get_hdd_config.assert_called_once_with(1)


ISAPI_CHANNELS_XML = """\
<InputProxyChannelStatusList>
  <InputProxyChannelStatus>
    <id>1</id><name>Cam-1</name><online>true</online>
    <sourceInputPortDescriptor><ipAddress>192.168.1.10</ipAddress></sourceInputPortDescriptor>
  </InputProxyChannelStatus>
  <InputProxyChannelStatus>
    <id>2</id><name>Cam-2</name><online>true</online>
    <sourceInputPortDescriptor><ipAddress>192.168.1.11</ipAddress></sourceInputPortDescriptor>
  </InputProxyChannelStatus>
  <InputProxyChannelStatus>
    <id>3</id><name>Cam-3</name><online>false</online>
    <sourceInputPortDescriptor><ipAddress>192.168.1.12</ipAddress></sourceInputPortDescriptor>
  </InputProxyChannelStatus>
</InputProxyChannelStatusList>"""


@pytest.mark.asyncio
async def test_get_channels_via_isapi_tunnel(mock_binding: MagicMock) -> None:
    """Primary path: ISAPI tunneled through SDK gives online + IP."""
    mock_binding.std_xml_config.return_value = ISAPI_CHANNELS_XML
    transport = SdkTransport(binding=mock_binding)
    await transport.connect("192.168.1.100", 8000, "admin", "password123")

    result = await transport.get_channels_status()

    assert len(result) == 8  # ip_chan_num=8
    # First 3 channels have ISAPI data
    assert result[0]["sdk_data"]["online"] is True
    assert result[0]["sdk_data"]["ip_address"] == "192.168.1.10"
    assert result[0]["sdk_data"]["channel_name"] == "Cam-1"
    assert result[0]["sdk_data"]["source"] == "isapi_via_sdk"
    assert result[1]["sdk_data"]["online"] is True
    assert result[2]["sdk_data"]["online"] is False
    # Channels 4-8 not in ISAPI response → None
    assert result[3]["sdk_data"]["online"] is None
    mock_binding.std_xml_config.assert_called_once()


@pytest.mark.asyncio
async def test_get_channels_fallback_to_cmd6126(mock_binding: MagicMock) -> None:
    """If ISAPI tunnel fails, fall back to cmd 6126."""
    mock_binding.std_xml_config.side_effect = Exception("not supported")
    mock_binding.get_digital_channel_state.return_value = [
        {"channel_id": str(33 + i), "online": i < 5} for i in range(8)
    ]
    transport = SdkTransport(binding=mock_binding)
    await transport.connect("192.168.1.100", 8000, "admin", "password123")

    result = await transport.get_channels_status()

    assert len(result) == 8
    assert result[0]["sdk_data"]["source"] == "sdk_cmd6126"
    assert result[0]["sdk_data"]["online"] is True
    assert result[5]["sdk_data"]["online"] is False


@pytest.mark.asyncio
async def test_get_channels_all_fallbacks_fail(mock_binding: MagicMock) -> None:
    """If both ISAPI and cmd 6126 fail, online should be None."""
    mock_binding.std_xml_config.side_effect = Exception("fail")
    mock_binding.get_digital_channel_state.side_effect = Exception("fail")
    transport = SdkTransport(binding=mock_binding)
    await transport.connect("192.168.1.100", 8000, "admin", "password123")

    result = await transport.get_channels_status()

    assert len(result) == 8
    for ch in result:
        assert ch["sdk_data"]["online"] is None


@pytest.mark.asyncio
async def test_get_snapshot_raises_not_implemented(mock_binding: MagicMock) -> None:
    transport = SdkTransport(binding=mock_binding)
    with pytest.raises(NotImplementedError):
        await transport.get_snapshot("1")
