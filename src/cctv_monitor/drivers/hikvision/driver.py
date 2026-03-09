"""Hikvision device driver — wires transport + mappers into DeviceDriver interface."""

from __future__ import annotations

import time
from datetime import datetime, timezone

from cctv_monitor.core.types import CameraStatus, ChannelId, DiskStatus
from cctv_monitor.drivers.hikvision.mappers import HikvisionMapper
from cctv_monitor.drivers.hikvision.transports.base import HikvisionTransport
from cctv_monitor.models.capabilities import DeviceCapabilities
from cctv_monitor.models.device import DeviceConfig, DeviceInfo
from cctv_monitor.models.device_health import DeviceHealthSummary
from cctv_monitor.models.snapshot import SnapshotResult
from cctv_monitor.models.status import (
    CameraChannelStatus,
    ChannelRecordingStatus,
    DiskHealthStatus,
)


class HikvisionDriver:
    """Implements the DeviceDriver protocol for Hikvision devices."""

    def __init__(self, transport: HikvisionTransport) -> None:
        self._transport = transport
        self._config: DeviceConfig | None = None

    @property
    def device_id(self) -> str:
        return self._config.device_id if self._config else "unknown"

    async def connect(self, config: DeviceConfig, port: int | None = None) -> None:
        self._config = config
        connect_port = port or config.web_port or config.sdk_port or 80
        await self._transport.connect(config.host, connect_port, config.username, config.password)

    async def disconnect(self) -> None:
        await self._transport.disconnect()

    async def get_device_info(self) -> DeviceInfo:
        raw = await self._transport.get_device_info()
        if "sdk_data" in raw:
            return HikvisionMapper.parse_device_info_sdk(raw["sdk_data"], self.device_id)
        return HikvisionMapper.parse_device_info(raw["raw_xml"], self.device_id)

    async def get_camera_statuses(self) -> list[CameraChannelStatus]:
        now = datetime.now(timezone.utc)

        # Try NVR-style InputProxy first (IP cameras)
        raw_list = await self._transport.get_channels_status()

        if raw_list and "sdk_data" in raw_list[0]:
            return HikvisionMapper.parse_channels_sdk(
                [r["sdk_data"] for r in raw_list], self.device_id, now
            )

        all_statuses: list[CameraChannelStatus] = []
        for raw in raw_list:
            all_statuses.extend(
                HikvisionMapper.parse_channels_status(raw["raw_xml"], self.device_id, now)
            )

        # If empty, try DVR-style VideoInputChannels (analog cameras)
        if not all_statuses:
            try:
                raw = await self._transport.get_video_inputs()
                all_statuses = HikvisionMapper.parse_video_inputs(
                    raw["raw_xml"], self.device_id, now
                )
            except Exception:
                pass

        return all_statuses

    async def get_disk_statuses(self) -> list[DiskHealthStatus]:
        raw_list = await self._transport.get_disk_status()
        now = datetime.now(timezone.utc)

        if raw_list and "sdk_data" in raw_list[0]:
            return HikvisionMapper.parse_disk_status_sdk(
                [r["sdk_data"] for r in raw_list], self.device_id, now
            )

        all_disks: list[DiskHealthStatus] = []
        for raw in raw_list:
            all_disks.extend(
                HikvisionMapper.parse_disk_status(raw["raw_xml"], self.device_id, now)
            )

        # Enrich with SMART data if transport supports it
        if hasattr(self._transport, "get_disk_smart"):
            for disk in all_disks:
                try:
                    smart_raw = await self._transport.get_disk_smart(int(disk.disk_id))
                    if smart_raw and "raw_xml" in smart_raw:
                        smart = HikvisionMapper.parse_smart_status(smart_raw["raw_xml"])
                        disk.temperature = smart.get("temperature")
                        disk.power_on_hours = smart.get("power_on_hours")
                        disk.smart_status = smart.get("smart_status")
                except Exception:
                    pass

        return all_disks

    async def get_device_time(self) -> dict | None:
        """Get device time and calculate drift from server time.

        Returns dict with device_time, server_time, drift_seconds, timezone, time_mode
        or None if not supported.
        """
        if not hasattr(self._transport, "get_device_time"):
            return None
        try:
            raw = await self._transport.get_device_time()
            if not raw or "raw_xml" not in raw:
                return None
            parsed = HikvisionMapper.parse_device_time(raw["raw_xml"])
            if not parsed.get("local_time"):
                return None

            # Parse device local time — strip timezone offset for drift calc
            device_time_str = parsed["local_time"]
            # Remove timezone offset if present (e.g. +03:00)
            clean = device_time_str.rstrip("Z")
            if "+" in clean[10:]:
                clean = clean[:clean.rindex("+")]
            elif clean.count("-") > 2:
                # e.g. 2026-03-08T14:30:45-03:00
                clean = clean[:clean.rindex("-")]

            from datetime import datetime as dt
            device_local = dt.strptime(clean, "%Y-%m-%dT%H:%M:%S")
            server_local = dt.now()
            drift_seconds = int((device_local - server_local).total_seconds())

            return {
                "device_time": parsed["local_time"],
                "server_time": server_local.strftime("%Y-%m-%dT%H:%M:%S"),
                "drift_seconds": drift_seconds,
                "timezone": parsed.get("timezone"),
                "time_mode": parsed.get("time_mode"),
            }
        except Exception:
            return None

    async def get_recording_statuses(self) -> list[ChannelRecordingStatus]:
        now = datetime.now(timezone.utc)

        # Try /record/tracks first (fast, single request)
        try:
            raw_list = await self._transport.get_recording_status()
            if raw_list and "raw_xml" in raw_list[0]:
                results = HikvisionMapper.parse_recording_tracks(
                    raw_list[0]["raw_xml"], self.device_id, now
                )
                if results:
                    return results
        except Exception:
            pass

        # Fallback: search for recording files in the last 24 hours per channel
        if hasattr(self._transport, "search_recordings"):
            try:
                channels = await self.get_camera_statuses()
                from datetime import timedelta
                from cctv_monitor.core.types import RecordingStatus
                # NVRs expect local time with Z suffix (Hikvision ISAPI convention)
                local_now = datetime.now()
                local_start = local_now - timedelta(hours=24)
                start_time = local_start.strftime("%Y-%m-%dT%H:%M:%SZ")
                end_time = local_now.strftime("%Y-%m-%dT%H:%M:%SZ")
                results: list[ChannelRecordingStatus] = []
                for ch in channels:
                    # Track ID = channel_number * 100 + stream (1=main)
                    ch_num = int(ch.channel_id)
                    track_id = ch_num * 100 + 1
                    try:
                        raw = await self._transport.search_recordings(
                            track_id, start_time, end_time,
                        )
                        has_rec = (
                            HikvisionMapper.parse_recording_search(raw["raw_xml"])
                            if raw and "raw_xml" in raw else False
                        )
                        results.append(ChannelRecordingStatus(
                            device_id=self.device_id,
                            channel_id=ch.channel_id,
                            status=RecordingStatus.RECORDING if has_rec else RecordingStatus.NOT_RECORDING,
                            record_type=None,
                            checked_at=now,
                        ))
                    except Exception:
                        pass
                return results
            except Exception:
                pass

        return []

    async def get_snapshot(self, channel_id: ChannelId) -> SnapshotResult:
        now = datetime.now(timezone.utc)
        try:
            image_data = await self._transport.get_snapshot(channel_id)
            return SnapshotResult(
                device_id=self.device_id,
                channel_id=channel_id,
                success=True,
                checked_at=now,
                file_path=None,
                file_size_bytes=len(image_data),
            )
        except Exception as exc:
            return SnapshotResult(
                device_id=self.device_id,
                channel_id=channel_id,
                success=False,
                checked_at=now,
                error=str(exc),
            )

    async def check_health(self) -> DeviceHealthSummary:
        now = datetime.now(timezone.utc)
        start = time.monotonic()
        try:
            await self.get_device_info()
            reachable = True
        except Exception:
            reachable = False
        response_time = (time.monotonic() - start) * 1000

        cameras: list[CameraChannelStatus] = []
        disks: list[DiskHealthStatus] = []
        if reachable:
            try:
                cameras = await self.get_camera_statuses()
            except Exception:
                pass
            try:
                disks = await self.get_disk_statuses()
            except Exception:
                pass

        online = sum(1 for c in cameras if c.status == CameraStatus.ONLINE)
        disk_ok = all(d.status == DiskStatus.OK for d in disks) if disks else True

        return DeviceHealthSummary(
            device_id=self.device_id,
            reachable=reachable,
            camera_count=len(cameras),
            online_cameras=online,
            offline_cameras=len(cameras) - online,
            disk_ok=disk_ok,
            recording_ok=True,
            response_time_ms=response_time,
            checked_at=now,
        )

    async def detect_capabilities(self) -> DeviceCapabilities:
        now = datetime.now(timezone.utc)
        model = ""
        firmware = ""
        supports_snapshot = False
        supports_disk = False

        try:
            info = await self.get_device_info()
            model = info.model
            firmware = info.firmware_version
        except Exception:
            pass
        try:
            await self.get_disk_statuses()
            supports_disk = True
        except Exception:
            pass
        try:
            await self.get_snapshot("101")
            supports_snapshot = True
        except Exception:
            pass

        return DeviceCapabilities(
            device_id=self.device_id,
            model=model,
            firmware_version=firmware,
            supports_isapi=True,
            supports_sdk=False,
            supports_snapshot=supports_snapshot,
            supports_recording_status=False,
            supports_disk_status=supports_disk,
            detected_at=now,
        )
