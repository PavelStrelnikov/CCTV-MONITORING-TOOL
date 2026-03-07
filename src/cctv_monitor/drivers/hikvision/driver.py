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

    async def connect(self, config: DeviceConfig) -> None:
        self._config = config
        await self._transport.connect(config.host, config.port, config.username, config.password)

    async def disconnect(self) -> None:
        await self._transport.disconnect()

    async def get_device_info(self) -> DeviceInfo:
        raw = await self._transport.get_device_info()
        return HikvisionMapper.parse_device_info(raw["raw_xml"], self.device_id)

    async def get_camera_statuses(self) -> list[CameraChannelStatus]:
        now = datetime.now(timezone.utc)

        # Try NVR-style InputProxy first (IP cameras)
        raw_list = await self._transport.get_channels_status()
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
        all_disks: list[DiskHealthStatus] = []
        for raw in raw_list:
            all_disks.extend(
                HikvisionMapper.parse_disk_status(raw["raw_xml"], self.device_id, now)
            )
        return all_disks

    async def get_recording_statuses(self) -> list[ChannelRecordingStatus]:
        return []  # TODO: implement when recording endpoint is verified

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
