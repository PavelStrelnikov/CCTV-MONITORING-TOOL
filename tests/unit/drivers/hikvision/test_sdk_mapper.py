"""Tests for Hikvision SDK data mappers."""

from datetime import datetime, timezone

from cctv_monitor.core.types import CameraStatus, DiskStatus
from cctv_monitor.drivers.hikvision.mappers import HikvisionMapper


class TestParseDeviceInfoSdk:
    """Tests for HikvisionMapper.parse_device_info_sdk."""

    def test_parse_device_info_from_sdk(self) -> None:
        sdk_data = {
            "device_type_name": "DS-7608NI-K2",
            "serial_number": "DS-7608NI-K2202099999",
            "firmware_version": "V4.30.085",
            "device_type": 76,
            "ip_chan_num": 8,
            "chan_num": 0,
        }
        result = HikvisionMapper.parse_device_info_sdk(sdk_data, "nvr-sdk-01")

        assert result.device_id == "nvr-sdk-01"
        assert result.model == "DS-7608NI-K2"
        assert result.serial_number == "DS-7608NI-K2202099999"
        assert result.firmware_version == "V4.30.085"
        assert result.device_type == "76"
        assert result.mac_address is None
        assert result.channels_count == 8


class TestParseChannelsSdk:
    """Tests for HikvisionMapper.parse_channels_sdk."""

    def test_parse_channels_from_sdk_no_online_field(self) -> None:
        now = datetime.now(timezone.utc)
        sdk_channels = [
            {"channel_id": "1", "channel_name": "Camera 1"},
            {"channel_id": "2", "channel_name": "Camera 2"},
        ]
        result = HikvisionMapper.parse_channels_sdk(sdk_channels, "nvr-sdk-01", now)

        assert len(result) == 2
        assert result[0].channel_id == "1"
        assert result[0].channel_name == "Camera 1"
        assert result[0].status == CameraStatus.UNKNOWN
        assert result[0].ip_address is None
        assert result[0].protocol is None
        assert result[1].channel_id == "2"
        assert result[1].status == CameraStatus.UNKNOWN

    def test_parse_channels_with_online_status(self) -> None:
        now = datetime.now(timezone.utc)
        sdk_channels = [
            {"channel_id": "1", "channel_name": "Camera 1", "online": True},
            {"channel_id": "2", "channel_name": "Camera 2", "online": False},
            {"channel_id": "3", "channel_name": "Camera 3", "online": None},
        ]
        result = HikvisionMapper.parse_channels_sdk(sdk_channels, "nvr-sdk-01", now)

        assert len(result) == 3
        assert result[0].status == CameraStatus.ONLINE
        assert result[1].status == CameraStatus.OFFLINE
        assert result[2].status == CameraStatus.UNKNOWN


class TestParseDiskStatusSdk:
    """Tests for HikvisionMapper.parse_disk_status_sdk."""

    def test_parse_disk_status_from_sdk(self) -> None:
        now = datetime.now(timezone.utc)
        sdk_disks = [
            {
                "disk_id": "1",
                "status_name": "normal",
                "capacity_mb": 1024,
                "free_space_mb": 512,
            },
        ]
        result = HikvisionMapper.parse_disk_status_sdk(sdk_disks, "nvr-sdk-01", now)

        assert len(result) == 1
        assert result[0].disk_id == "1"
        assert result[0].status == DiskStatus.OK
        assert result[0].capacity_bytes == 1024 * 1024 * 1024
        assert result[0].free_bytes == 512 * 1024 * 1024
        assert result[0].health_status == "normal"

    def test_parse_disk_error_status(self) -> None:
        now = datetime.now(timezone.utc)
        sdk_disks = [
            {
                "disk_id": "2",
                "status_name": "error",
                "capacity_mb": 2048,
                "free_space_mb": 0,
            },
        ]
        result = HikvisionMapper.parse_disk_status_sdk(sdk_disks, "nvr-sdk-01", now)

        assert len(result) == 1
        assert result[0].status == DiskStatus.ERROR
        assert result[0].capacity_bytes == 2048 * 1024 * 1024
        assert result[0].free_bytes == 0
