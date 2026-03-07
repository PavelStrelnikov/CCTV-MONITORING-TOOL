"""Hikvision ISAPI XML response mappers."""

from __future__ import annotations

import xml.etree.ElementTree as ET
from datetime import datetime

from cctv_monitor.core.types import CameraStatus, DeviceId, DiskStatus
from cctv_monitor.models.device import DeviceInfo
from cctv_monitor.models.status import CameraChannelStatus, DiskHealthStatus

KNOWN_NAMESPACES = [
    {"hik": "http://www.hikvision.com/ver20/XMLSchema"},
    {"hik": "http://www.std-cgi.com/ver20/XMLSchema"},
]


def _detect_ns(root: ET.Element) -> dict[str, str]:
    """Detect which namespace the XML uses from known Hikvision variants."""
    tag = root.tag
    for ns in KNOWN_NAMESPACES:
        uri = ns["hik"]
        if f"{{{uri}}}" in tag:
            return ns
    return {}


def _find_text(element: ET.Element, tag: str, default: str = "", ns: dict | None = None) -> str:
    """Find text of a child element, trying namespaced first then plain."""
    if ns:
        node = element.find(f"hik:{tag}", ns)
        if node is not None:
            return node.text.strip() if node.text else default
    node = element.find(tag)
    return node.text.strip() if node is not None and node.text else default


class HikvisionMapper:
    """Maps Hikvision ISAPI XML responses to normalized domain models."""

    @staticmethod
    def parse_device_info(xml_text: str, device_id: DeviceId) -> DeviceInfo:
        """Parse ISAPI /System/deviceInfo response into DeviceInfo."""
        root = ET.fromstring(xml_text)
        ns = _detect_ns(root)
        return DeviceInfo(
            device_id=device_id,
            model=_find_text(root, "model", ns=ns),
            serial_number=_find_text(root, "serialNumber", ns=ns),
            firmware_version=_find_text(root, "firmwareVersion", ns=ns),
            device_type=_find_text(root, "deviceType", ns=ns),
            mac_address=_find_text(root, "macAddress", ns=ns) or None,
            channels_count=0,
        )

    @staticmethod
    def parse_channels_status(
        xml_text: str, device_id: DeviceId, checked_at: datetime
    ) -> list[CameraChannelStatus]:
        """Parse ISAPI /ContentMgmt/InputProxy/channels/status response."""
        root = ET.fromstring(xml_text)
        ns = _detect_ns(root)
        channels: list[CameraChannelStatus] = []

        items = root.findall("hik:InputProxyChannelStatus", ns) if ns else []
        if not items:
            items = root.findall("InputProxyChannelStatus")

        for ch_elem in items:
            online_text = _find_text(ch_elem, "online", "false", ns=ns)
            status = CameraStatus.ONLINE if online_text.lower() == "true" else CameraStatus.OFFLINE

            # ip is nested inside sourceInputPortDescriptor
            ip_address: str | None = None
            descriptor = ch_elem.find("hik:sourceInputPortDescriptor", ns) if ns else None
            if descriptor is None:
                descriptor = ch_elem.find("sourceInputPortDescriptor")
            if descriptor is not None:
                ip_address = _find_text(descriptor, "ipAddress", ns=ns) or None

            channels.append(
                CameraChannelStatus(
                    device_id=device_id,
                    channel_id=_find_text(ch_elem, "id", ns=ns),
                    channel_name=_find_text(ch_elem, "name", ns=ns),
                    status=status,
                    ip_address=ip_address,
                    protocol=None,
                    checked_at=checked_at,
                )
            )

        return channels

    @staticmethod
    def parse_video_inputs(
        xml_text: str, device_id: DeviceId, checked_at: datetime
    ) -> list[CameraChannelStatus]:
        """Parse ISAPI /System/Video/inputs/channels response (DVR analog channels)."""
        root = ET.fromstring(xml_text)
        ns = _detect_ns(root)
        channels: list[CameraChannelStatus] = []

        _NO_SIGNAL = {"no video", "novideo", "none"}

        items = root.findall("hik:VideoInputChannel", ns) if ns else []
        if not items:
            items = root.findall("VideoInputChannel")

        for ch_elem in items:
            res_desc = _find_text(ch_elem, "resDesc", "", ns=ns).lower()
            enabled = _find_text(ch_elem, "videoInputEnabled", "true", ns=ns).lower()

            if enabled == "true" and res_desc not in _NO_SIGNAL:
                status = CameraStatus.ONLINE
            else:
                status = CameraStatus.OFFLINE

            channels.append(
                CameraChannelStatus(
                    device_id=device_id,
                    channel_id=_find_text(ch_elem, "id", ns=ns),
                    channel_name=_find_text(ch_elem, "name", ns=ns),
                    status=status,
                    ip_address=None,
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
        ns = _detect_ns(root)
        disks: list[DiskHealthStatus] = []

        _STATUS_MAP = {
            "ok": DiskStatus.OK,
            "normal": DiskStatus.OK,
            "idle": DiskStatus.OK,
            "warning": DiskStatus.WARNING,
            "error": DiskStatus.ERROR,
            "unformatted": DiskStatus.WARNING,
            "abnormal": DiskStatus.ERROR,
        }

        # Real NVRs return <hdd> (lowercase), spec says <HDD>
        items = root.findall("hik:HDD", ns) if ns else []
        if not items:
            items = root.findall("HDD")
        if not items:
            items = root.findall("hik:hdd", ns) if ns else []
        if not items:
            items = root.findall("hdd")

        for hdd_elem in items:
            raw_status = _find_text(hdd_elem, "status", "unknown", ns=ns).lower()
            capacity_mb = int(_find_text(hdd_elem, "capacity", "0", ns=ns))
            free_mb = int(_find_text(hdd_elem, "freeSpace", "0", ns=ns))

            disks.append(
                DiskHealthStatus(
                    device_id=device_id,
                    disk_id=_find_text(hdd_elem, "id", ns=ns),
                    status=_STATUS_MAP.get(raw_status, DiskStatus.UNKNOWN),
                    capacity_bytes=capacity_mb * 1024 * 1024,
                    free_bytes=free_mb * 1024 * 1024,
                    health_status=raw_status,
                    checked_at=checked_at,
                )
            )

        return disks
