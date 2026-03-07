"""Tests for Hikvision ISAPI XML mappers."""

from datetime import datetime, timezone
from pathlib import Path

import pytest

from cctv_monitor.core.types import CameraStatus, DiskStatus
from cctv_monitor.drivers.hikvision.mappers import HikvisionMapper
from cctv_monitor.models.device import DeviceInfo
from cctv_monitor.models.status import CameraChannelStatus, DiskHealthStatus

FIXTURES_DIR = Path(__file__).resolve().parents[3] / "fixtures" / "hikvision"


@pytest.fixture()
def device_info_xml() -> str:
    return (FIXTURES_DIR / "device_info.xml").read_text(encoding="utf-8")


@pytest.fixture()
def channels_status_xml() -> str:
    return (FIXTURES_DIR / "channels_status.xml").read_text(encoding="utf-8")


@pytest.fixture()
def hdd_status_xml() -> str:
    return (FIXTURES_DIR / "hdd_status.xml").read_text(encoding="utf-8")


class TestParseDeviceInfo:
    """Tests for HikvisionMapper.parse_device_info."""

    def test_parses_model(self, device_info_xml: str) -> None:
        result = HikvisionMapper.parse_device_info(device_info_xml, "nvr-01")
        assert result.model == "DS-7608NI-K2"

    def test_parses_serial_number(self, device_info_xml: str) -> None:
        result = HikvisionMapper.parse_device_info(device_info_xml, "nvr-01")
        assert result.serial_number == "DS-7608NI-K2202012345"

    def test_parses_firmware_version(self, device_info_xml: str) -> None:
        result = HikvisionMapper.parse_device_info(device_info_xml, "nvr-01")
        assert result.firmware_version == "V4.30.085 build 200916"

    def test_parses_device_type(self, device_info_xml: str) -> None:
        result = HikvisionMapper.parse_device_info(device_info_xml, "nvr-01")
        assert result.device_type == "NVR"

    def test_parses_mac_address(self, device_info_xml: str) -> None:
        result = HikvisionMapper.parse_device_info(device_info_xml, "nvr-01")
        assert result.mac_address == "aa:bb:cc:dd:ee:ff"

    def test_returns_device_info_instance(self, device_info_xml: str) -> None:
        result = HikvisionMapper.parse_device_info(device_info_xml, "nvr-01")
        assert isinstance(result, DeviceInfo)

    def test_sets_device_id(self, device_info_xml: str) -> None:
        result = HikvisionMapper.parse_device_info(device_info_xml, "nvr-01")
        assert result.device_id == "nvr-01"

    def test_channels_count_defaults_to_zero(self, device_info_xml: str) -> None:
        result = HikvisionMapper.parse_device_info(device_info_xml, "nvr-01")
        assert result.channels_count == 0


class TestParseDeviceInfoStdCgi:
    """Tests for parsing XML with std-cgi namespace (real NVR response)."""

    @pytest.fixture()
    def stdcgi_xml(self) -> str:
        return (FIXTURES_DIR / "device_info_stdcgi.xml").read_text(encoding="utf-8")

    def test_parses_model(self, stdcgi_xml: str) -> None:
        result = HikvisionMapper.parse_device_info(stdcgi_xml, "nvr-02")
        assert result.model == "NVR-216MH-C"

    def test_parses_serial_number(self, stdcgi_xml: str) -> None:
        result = HikvisionMapper.parse_device_info(stdcgi_xml, "nvr-02")
        assert result.serial_number == "NVR-216MH-C1620180824CCRRC45564269WCVU"

    def test_parses_firmware_version(self, stdcgi_xml: str) -> None:
        result = HikvisionMapper.parse_device_info(stdcgi_xml, "nvr-02")
        assert result.firmware_version == "V3.4.97"

    def test_parses_mac_address(self, stdcgi_xml: str) -> None:
        result = HikvisionMapper.parse_device_info(stdcgi_xml, "nvr-02")
        assert result.mac_address == "58:03:fb:c8:56:2b"

    def test_parses_device_type(self, stdcgi_xml: str) -> None:
        result = HikvisionMapper.parse_device_info(stdcgi_xml, "nvr-02")
        assert result.device_type == "IPC"


class TestParseChannelsStatus:
    """Tests for HikvisionMapper.parse_channels_status."""

    @pytest.fixture()
    def checked_at(self) -> datetime:
        return datetime(2025, 1, 15, 12, 0, 0, tzinfo=timezone.utc)

    def test_returns_two_channels(self, channels_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_channels_status(channels_status_xml, "nvr-01", checked_at)
        assert len(result) == 2

    def test_first_channel_is_online(self, channels_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_channels_status(channels_status_xml, "nvr-01", checked_at)
        assert result[0].status == CameraStatus.ONLINE

    def test_second_channel_is_offline(self, channels_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_channels_status(channels_status_xml, "nvr-01", checked_at)
        assert result[1].status == CameraStatus.OFFLINE

    def test_first_channel_name(self, channels_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_channels_status(channels_status_xml, "nvr-01", checked_at)
        assert result[0].channel_name == "Front Door"

    def test_second_channel_name(self, channels_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_channels_status(channels_status_xml, "nvr-01", checked_at)
        assert result[1].channel_name == "Back Yard"

    def test_first_channel_ip(self, channels_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_channels_status(channels_status_xml, "nvr-01", checked_at)
        assert result[0].ip_address == "192.168.1.10"

    def test_second_channel_ip(self, channels_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_channels_status(channels_status_xml, "nvr-01", checked_at)
        assert result[1].ip_address == "192.168.1.11"

    def test_channel_id(self, channels_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_channels_status(channels_status_xml, "nvr-01", checked_at)
        assert result[0].channel_id == "1"
        assert result[1].channel_id == "2"

    def test_device_id_is_set(self, channels_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_channels_status(channels_status_xml, "nvr-01", checked_at)
        assert result[0].device_id == "nvr-01"

    def test_checked_at_is_set(self, channels_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_channels_status(channels_status_xml, "nvr-01", checked_at)
        assert result[0].checked_at == checked_at

    def test_returns_camera_channel_status_instances(self, channels_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_channels_status(channels_status_xml, "nvr-01", checked_at)
        assert all(isinstance(ch, CameraChannelStatus) for ch in result)


class TestParseDiskStatus:
    """Tests for HikvisionMapper.parse_disk_status."""

    @pytest.fixture()
    def checked_at(self) -> datetime:
        return datetime(2025, 1, 15, 12, 0, 0, tzinfo=timezone.utc)

    def test_returns_one_disk(self, hdd_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_disk_status(hdd_status_xml, "nvr-01", checked_at)
        assert len(result) == 1

    def test_disk_id(self, hdd_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_disk_status(hdd_status_xml, "nvr-01", checked_at)
        assert result[0].disk_id == "1"

    def test_capacity_bytes(self, hdd_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_disk_status(hdd_status_xml, "nvr-01", checked_at)
        assert result[0].capacity_bytes == 2000 * 1024 * 1024

    def test_free_bytes(self, hdd_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_disk_status(hdd_status_xml, "nvr-01", checked_at)
        assert result[0].free_bytes == 500 * 1024 * 1024

    def test_health_status(self, hdd_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_disk_status(hdd_status_xml, "nvr-01", checked_at)
        assert result[0].health_status == "ok"

    def test_status_enum(self, hdd_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_disk_status(hdd_status_xml, "nvr-01", checked_at)
        assert result[0].status == DiskStatus.OK

    def test_device_id_is_set(self, hdd_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_disk_status(hdd_status_xml, "nvr-01", checked_at)
        assert result[0].device_id == "nvr-01"

    def test_checked_at_is_set(self, hdd_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_disk_status(hdd_status_xml, "nvr-01", checked_at)
        assert result[0].checked_at == checked_at

    def test_returns_disk_health_status_instances(self, hdd_status_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_disk_status(hdd_status_xml, "nvr-01", checked_at)
        assert all(isinstance(d, DiskHealthStatus) for d in result)


class TestParseDiskStatusReal:
    """Tests for parsing real NVR HDD response (lowercase <hdd> tags)."""

    @pytest.fixture()
    def checked_at(self) -> datetime:
        return datetime(2025, 1, 15, 12, 0, 0, tzinfo=timezone.utc)

    @pytest.fixture()
    def real_hdd_xml(self) -> str:
        return (FIXTURES_DIR / "hdd_status_real.xml").read_text(encoding="utf-8")

    def test_returns_two_disks(self, real_hdd_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_disk_status(real_hdd_xml, "nvr-brosh-40", checked_at)
        assert len(result) == 2

    def test_first_disk_ok(self, real_hdd_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_disk_status(real_hdd_xml, "nvr-brosh-40", checked_at)
        assert result[0].status == DiskStatus.OK
        assert result[0].disk_id == "1"

    def test_second_disk_idle_maps_to_ok(self, real_hdd_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_disk_status(real_hdd_xml, "nvr-brosh-40", checked_at)
        assert result[1].status == DiskStatus.OK
        assert result[1].health_status == "idle"

    def test_capacity_in_bytes(self, real_hdd_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_disk_status(real_hdd_xml, "nvr-brosh-40", checked_at)
        assert result[0].capacity_bytes == 1907729 * 1024 * 1024

    def test_free_space_zero(self, real_hdd_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_disk_status(real_hdd_xml, "nvr-brosh-40", checked_at)
        assert result[0].free_bytes == 0


class TestParseVideoInputs:
    """Tests for parsing DVR VideoInputChannel (analog cameras)."""

    @pytest.fixture()
    def checked_at(self) -> datetime:
        return datetime(2025, 1, 15, 12, 0, 0, tzinfo=timezone.utc)

    @pytest.fixture()
    def dvr_xml(self) -> str:
        return (FIXTURES_DIR / "video_inputs_dvr.xml").read_text(encoding="utf-8")

    def test_returns_three_channels(self, dvr_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_video_inputs(dvr_xml, "nvr-brosh-40", checked_at)
        assert len(result) == 3

    def test_channel_with_signal_is_online(self, dvr_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_video_inputs(dvr_xml, "nvr-brosh-40", checked_at)
        assert result[0].status == CameraStatus.ONLINE
        assert result[0].channel_name == "Camera1"

    def test_channel_no_video_is_offline(self, dvr_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_video_inputs(dvr_xml, "nvr-brosh-40", checked_at)
        assert result[1].status == CameraStatus.OFFLINE
        assert result[1].channel_name == "Camera2"

    def test_channel_id(self, dvr_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_video_inputs(dvr_xml, "nvr-brosh-40", checked_at)
        assert result[0].channel_id == "1"
        assert result[1].channel_id == "2"
        assert result[2].channel_id == "3"

    def test_ip_address_is_none_for_analog(self, dvr_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_video_inputs(dvr_xml, "nvr-brosh-40", checked_at)
        assert all(ch.ip_address is None for ch in result)


class TestParseChannelsStatusReal:
    """Tests for parsing real NVR InputProxyChannelStatus (no <name> tag)."""

    @pytest.fixture()
    def checked_at(self) -> datetime:
        return datetime(2025, 1, 15, 12, 0, 0, tzinfo=timezone.utc)

    @pytest.fixture()
    def real_channels_xml(self) -> str:
        return (FIXTURES_DIR / "channels_status_real.xml").read_text(encoding="utf-8")

    def test_returns_four_channels(self, real_channels_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_channels_status(real_channels_xml, "nvr-mike-7", checked_at)
        assert len(result) == 4

    def test_three_online_one_offline(self, real_channels_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_channels_status(real_channels_xml, "nvr-mike-7", checked_at)
        online = [ch for ch in result if ch.status == CameraStatus.ONLINE]
        offline = [ch for ch in result if ch.status == CameraStatus.OFFLINE]
        assert len(online) == 3
        assert len(offline) == 1

    def test_ip_addresses_parsed(self, real_channels_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_channels_status(real_channels_xml, "nvr-mike-7", checked_at)
        assert result[0].ip_address == "192.168.254.2"
        assert result[3].ip_address == "192.168.254.5"

    def test_name_empty_when_missing(self, real_channels_xml: str, checked_at: datetime) -> None:
        result = HikvisionMapper.parse_channels_status(real_channels_xml, "nvr-mike-7", checked_at)
        assert result[0].channel_name == ""
