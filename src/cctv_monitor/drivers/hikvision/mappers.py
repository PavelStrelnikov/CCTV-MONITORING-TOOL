"""Hikvision ISAPI XML and SDK response mappers."""

from __future__ import annotations

import re
import xml.etree.ElementTree as ET
from datetime import datetime

from cctv_monitor.core.types import CameraStatus, DeviceId, DiskStatus, RecordingStatus
from cctv_monitor.models.device import DeviceInfo
from cctv_monitor.models.status import CameraChannelStatus, ChannelRecordingStatus, DiskHealthStatus

KNOWN_NAMESPACES = [
    {"hik": "http://www.hikvision.com/ver20/XMLSchema"},
    {"hik": "http://www.isapi.org/ver20/XMLSchema"},
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

    @staticmethod
    def parse_smart_status(xml_text: str) -> dict:
        """Parse ISAPI /ContentMgmt/Storage/hdd/{id}/SMARTTest/status response.

        Returns dict with smart_status, temperature, power_on_hours.
        """
        info: dict = {
            "smart_status": "ok",
            "temperature": None,
            "power_on_hours": None,
        }

        # SMART overall status
        self_eval = re.search(
            r"<selfEvaluaingStatus>(\w+)</selfEvaluaingStatus>",
            xml_text, re.IGNORECASE,
        )
        if self_eval and self_eval.group(1).lower() in ("error", "fail", "fault"):
            info["smart_status"] = "error"

        all_eval = re.search(
            r"<allEvaluaingStatus>(\w+)</allEvaluaingStatus>",
            xml_text, re.IGNORECASE,
        )
        if all_eval and all_eval.group(1).lower() in ("fault", "error", "fail"):
            info["smart_status"] = "error"

        # Temperature (Hikvision typo: temprature)
        temp_m = re.search(r"<temprature>(\d+)</temprature>", xml_text, re.IGNORECASE)
        if not temp_m:
            temp_m = re.search(r"<temperature>(\d+)</temperature>", xml_text, re.IGNORECASE)
        if temp_m:
            info["temperature"] = int(temp_m.group(1))

        # Power-on days
        pod_m = re.search(r"<powerOnDay>(\d+)</powerOnDay>", xml_text, re.IGNORECASE)
        if pod_m:
            info["power_on_hours"] = int(pod_m.group(1)) * 24

        # SMART attributes (ID 9=Power-On Hours, 5=Reallocated, 194=Temp)
        attrs = re.findall(
            r"<TestResult>.*?<attributeID>(\d+)</attributeID>.*?<rawValue>(\d+)</rawValue>.*?</TestResult>",
            xml_text, re.DOTALL | re.IGNORECASE,
        )
        if not attrs:
            attrs = re.findall(
                r"<SMARTAttribute>.*?<id>(\d+)</id>.*?<rawValue>(\d+)</rawValue>.*?</SMARTAttribute>",
                xml_text, re.DOTALL | re.IGNORECASE,
            )

        for attr_id_str, raw_str in attrs:
            attr_id = int(attr_id_str)
            raw_val = int(raw_str)
            if attr_id == 9:
                info["power_on_hours"] = raw_val
            elif attr_id == 194:
                if info["temperature"] is None:
                    info["temperature"] = raw_val & 0xFF
            elif attr_id == 5:
                if raw_val > 100:
                    info["smart_status"] = "error"
                elif raw_val > 0 and info["smart_status"] == "ok":
                    info["smart_status"] = "warning"

        return info

    @staticmethod
    def parse_recording_tracks(
        xml_text: str, device_id: DeviceId, checked_at: datetime
    ) -> list[ChannelRecordingStatus]:
        """Parse ISAPI /ContentMgmt/record/tracks response.

        Each <Track> has <id> (like "101"), <trackType> (video), and
        a nested <CustomExtension> with <recordStatus> (RECORDING/STOPPED).
        """
        root = ET.fromstring(xml_text)
        ns = _detect_ns(root)
        results: list[ChannelRecordingStatus] = []

        items = root.findall("hik:Track", ns) if ns else []
        if not items:
            items = root.findall("Track")

        for track in items:
            track_type = _find_text(track, "trackType", "", ns=ns).lower()
            if track_type != "video":
                continue

            track_id = _find_text(track, "id", "", ns=ns)
            # Channel ID: track_id is like "101" (channel 1 stream 1)
            # Get the channel part
            channel_id = track_id[:len(track_id) - 2] if len(track_id) > 2 else track_id

            # Look for recording status in CustomExtension or directly
            rec_status_text = ""
            custom = track.find("hik:CustomExtension", ns) if ns else None
            if custom is None:
                custom = track.find("CustomExtension")
            if custom is not None:
                rec_status_text = _find_text(custom, "recordStatus", "", ns=ns).lower()

            if not rec_status_text:
                rec_status_text = _find_text(track, "recordStatus", "", ns=ns).lower()

            if rec_status_text in ("recording", "started"):
                status = RecordingStatus.RECORDING
            elif rec_status_text in ("stopped", "notrecording", "not_recording"):
                status = RecordingStatus.NOT_RECORDING
            else:
                status = RecordingStatus.UNKNOWN

            # Only keep main stream tracks (ending in "01")
            if not track_id.endswith("01") and not track_id.endswith("1"):
                continue

            record_type_text = _find_text(track, "recordType", None, ns=ns)

            results.append(
                ChannelRecordingStatus(
                    device_id=device_id,
                    channel_id=channel_id,
                    status=status,
                    record_type=record_type_text,
                    checked_at=checked_at,
                )
            )

        return results

    @staticmethod
    def parse_recording_search(xml_text: str) -> bool:
        """Parse ISAPI /ContentMgmt/search response.

        Returns True if at least one recording was found.
        """
        # Quick check: if <numOfMatches>0</numOfMatches> — no recordings
        num_m = re.search(r"<numOfMatches>(\d+)</numOfMatches>", xml_text, re.IGNORECASE)
        if num_m:
            return int(num_m.group(1)) > 0

        # Also check responseStatus
        status_m = re.search(r"<responseStatus>(\w+)</responseStatus>", xml_text, re.IGNORECASE)
        if status_m and status_m.group(1).lower() == "true":
            return True

        return False

    @staticmethod
    def parse_device_time(xml_text: str) -> dict:
        """Parse /ISAPI/System/time response.

        Returns dict with device_time (str), timezone, time_mode.
        """
        local_time = None
        tz_str = None
        time_mode = None

        # <localTime>2026-03-08T14:30:45+03:00</localTime>
        lt_m = re.search(r"<localTime>([^<]+)</localTime>", xml_text, re.IGNORECASE)
        if lt_m:
            local_time = lt_m.group(1)
        tz_m = re.search(r"<timeZone>([^<]+)</timeZone>", xml_text, re.IGNORECASE)
        if tz_m:
            tz_str = tz_m.group(1)
        mode_m = re.search(r"<timeMode>([^<]+)</timeMode>", xml_text, re.IGNORECASE)
        if mode_m:
            time_mode = mode_m.group(1)

        return {
            "local_time": local_time,
            "timezone": tz_str,
            "time_mode": time_mode,
        }

    # ── SDK data parsing methods ──────────────────────────────────────

    @staticmethod
    def parse_device_info_sdk(sdk_data: dict, device_id: DeviceId) -> DeviceInfo:
        """Parse SDK login response data into DeviceInfo."""
        return DeviceInfo(
            device_id=device_id,
            model=sdk_data["device_type_name"],
            serial_number=sdk_data["serial_number"],
            firmware_version=sdk_data["firmware_version"],
            device_type=str(sdk_data.get("device_type", sdk_data.get("dvr_type", ""))),
            mac_address=None,
            channels_count=sdk_data["ip_chan_num"] + sdk_data["chan_num"],
        )

    @staticmethod
    def parse_channels_sdk(
        sdk_channels: list[dict], device_id: DeviceId, checked_at: datetime
    ) -> list[CameraChannelStatus]:
        """Parse SDK channel list into CameraChannelStatus list."""
        channels: list[CameraChannelStatus] = []
        for ch in sdk_channels:
            online = ch.get("online")
            if online is True:
                status = CameraStatus.ONLINE
            elif online is False:
                status = CameraStatus.OFFLINE
            else:
                status = CameraStatus.UNKNOWN

            channels.append(
                CameraChannelStatus(
                    device_id=device_id,
                    channel_id=ch["channel_id"],
                    channel_name=ch["channel_name"],
                    status=status,
                    ip_address=ch.get("ip_address"),
                    protocol=None,
                    checked_at=checked_at,
                )
            )
        return channels

    @staticmethod
    def parse_disk_status_sdk(
        sdk_disks: list[dict], device_id: DeviceId, checked_at: datetime
    ) -> list[DiskHealthStatus]:
        """Parse SDK disk info list into DiskHealthStatus list."""
        _SDK_STATUS_MAP: dict[str, DiskStatus] = {
            "normal": DiskStatus.OK,
            "sleep": DiskStatus.OK,
            "error": DiskStatus.ERROR,
            "smart_failed": DiskStatus.ERROR,
            "bad_disk": DiskStatus.ERROR,
            "warning": DiskStatus.WARNING,
            "raw": DiskStatus.WARNING,
            "unmatched": DiskStatus.WARNING,
        }

        disks: list[DiskHealthStatus] = []
        for d in sdk_disks:
            raw_status = d.get("status_name", "").lower()
            disks.append(
                DiskHealthStatus(
                    device_id=device_id,
                    disk_id=d["disk_id"],
                    status=_SDK_STATUS_MAP.get(raw_status, DiskStatus.UNKNOWN),
                    capacity_bytes=d["capacity_mb"] * 1024 * 1024,
                    free_bytes=d["free_space_mb"] * 1024 * 1024,
                    health_status=raw_status,
                    checked_at=checked_at,
                )
            )
        return disks
