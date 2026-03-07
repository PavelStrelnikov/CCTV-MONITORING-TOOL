"""Hikvision ISAPI XML response mappers."""

from __future__ import annotations

import xml.etree.ElementTree as ET
from datetime import datetime

from cctv_monitor.core.types import CameraStatus, DeviceId, DiskStatus
from cctv_monitor.models.device import DeviceInfo
from cctv_monitor.models.status import CameraChannelStatus, DiskHealthStatus

NS = {"hik": "http://www.hikvision.com/ver20/XMLSchema"}


def _find_text(element: ET.Element, tag: str, default: str = "") -> str:
    """Find text of a child element, trying namespaced first then plain."""
    node = element.find(f"hik:{tag}", NS)
    if node is None:
        node = element.find(tag)
    return node.text.strip() if node is not None and node.text else default


class HikvisionMapper:
    """Maps Hikvision ISAPI XML responses to normalized domain models."""

    @staticmethod
    def parse_device_info(xml_text: str, device_id: DeviceId) -> DeviceInfo:
        """Parse ISAPI /System/deviceInfo response into DeviceInfo."""
        root = ET.fromstring(xml_text)
        return DeviceInfo(
            device_id=device_id,
            model=_find_text(root, "model"),
            serial_number=_find_text(root, "serialNumber"),
            firmware_version=_find_text(root, "firmwareVersion"),
            device_type=_find_text(root, "deviceType"),
            mac_address=_find_text(root, "macAddress") or None,
            channels_count=0,
        )

    @staticmethod
    def parse_channels_status(
        xml_text: str, device_id: DeviceId, checked_at: datetime
    ) -> list[CameraChannelStatus]:
        """Parse ISAPI /ContentMgmt/InputProxy/channels/status response."""
        root = ET.fromstring(xml_text)
        channels: list[CameraChannelStatus] = []

        for ch_elem in root.findall("hik:InputProxyChannelStatus", NS):
            online_text = _find_text(ch_elem, "online", "false")
            status = CameraStatus.ONLINE if online_text.lower() == "true" else CameraStatus.OFFLINE

            # ip is nested inside sourceInputPortDescriptor
            ip_address: str | None = None
            descriptor = ch_elem.find("hik:sourceInputPortDescriptor", NS)
            if descriptor is None:
                descriptor = ch_elem.find("sourceInputPortDescriptor")
            if descriptor is not None:
                ip_address = _find_text(descriptor, "ipAddress") or None

            channels.append(
                CameraChannelStatus(
                    device_id=device_id,
                    channel_id=_find_text(ch_elem, "id"),
                    channel_name=_find_text(ch_elem, "name"),
                    status=status,
                    ip_address=ip_address,
                    protocol=None,
                    checked_at=checked_at,
                )
            )

        return channels

    @staticmethod
    def parse_disk_status(
        xml_text: str, device_id: DeviceId, checked_at: datetime
    ) -> list[DiskHealthStatus]:
        """Parse ISAPI /ContentMgmt/Storage response."""
        root = ET.fromstring(xml_text)
        disks: list[DiskHealthStatus] = []

        _STATUS_MAP = {
            "ok": DiskStatus.OK,
            "warning": DiskStatus.WARNING,
            "error": DiskStatus.ERROR,
        }

        for hdd_elem in root.findall("hik:HDD", NS):
            raw_status = _find_text(hdd_elem, "status", "unknown").lower()
            capacity_mb = int(_find_text(hdd_elem, "capacity", "0"))
            free_mb = int(_find_text(hdd_elem, "freeSpace", "0"))

            disks.append(
                DiskHealthStatus(
                    device_id=device_id,
                    disk_id=_find_text(hdd_elem, "id"),
                    status=_STATUS_MAP.get(raw_status, DiskStatus.UNKNOWN),
                    capacity_bytes=capacity_mb * 1024 * 1024,
                    free_bytes=free_mb * 1024 * 1024,
                    health_status=raw_status,
                    checked_at=checked_at,
                )
            )

        return disks
