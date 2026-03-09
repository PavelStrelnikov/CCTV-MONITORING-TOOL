"""SDK transport for Hikvision devices using HCNetSDK bindings."""

from __future__ import annotations

import asyncio
from concurrent.futures import ThreadPoolExecutor

import structlog

from cctv_monitor.drivers.hikvision.transports.base import HikvisionTransport
from cctv_monitor.drivers.hikvision.transports.sdk_bindings import HCNetSDKBinding

logger = structlog.get_logger()

# Asyncio lock to serialize SDK calls (SDK is NOT thread-safe).
# Lives at module level so all SdkTransport instances share it.
# Using asyncio.Lock (not threading.Lock) so timeout cancellation
# properly releases the lock — a hung thread won't block the next call.
_sdk_async_lock: asyncio.Lock | None = None


def _get_sdk_lock() -> asyncio.Lock:
    """Get or create the module-level asyncio lock (must be called from event loop)."""
    global _sdk_async_lock
    if _sdk_async_lock is None:
        _sdk_async_lock = asyncio.Lock()
    return _sdk_async_lock


class SdkTransport(HikvisionTransport):
    """Hikvision transport backed by the native HCNetSDK (via ctypes).

    All SDK calls are blocking and not thread-safe, so they are serialized
    via an asyncio lock. Each call runs in a fresh daemon thread. If a call
    hangs and times out, the asyncio lock is released and the next caller
    can proceed — the hung thread is abandoned (cleaned up on process exit).
    """

    def __init__(self, binding: HCNetSDKBinding | None = None) -> None:
        self._binding = binding or HCNetSDKBinding()
        self._user_id: int = -1
        self._device_id: str = ""
        self._login_info: dict = {}

    _SDK_CALL_TIMEOUT = 30  # seconds — hard limit per SDK call

    async def _run(self, fn, *args):
        """Run a blocking SDK call in a fresh daemon thread with timeout.

        Acquires an asyncio lock before dispatching to thread, so only one
        SDK call runs at a time. On timeout, the lock is released at the
        asyncio level (the hung thread is abandoned but doesn't block others).
        """
        lock = _get_sdk_lock()

        try:
            await asyncio.wait_for(lock.acquire(), timeout=5.0)
        except asyncio.TimeoutError:
            logger.error(
                "sdk.lock_timeout",
                fn=fn.__name__, device=self._device_id,
            )
            raise TimeoutError("SDK lock acquisition timed out — previous call may be hung")

        try:
            loop = asyncio.get_running_loop()
            executor = ThreadPoolExecutor(max_workers=1, thread_name_prefix="hcnetsdk")
            try:
                return await asyncio.wait_for(
                    loop.run_in_executor(executor, fn, *args),
                    timeout=self._SDK_CALL_TIMEOUT,
                )
            except asyncio.TimeoutError:
                logger.error(
                    "sdk.call_timeout",
                    fn=fn.__name__, device=self._device_id,
                    timeout=self._SDK_CALL_TIMEOUT,
                )
                raise
            finally:
                executor.shutdown(wait=False)
        finally:
            lock.release()

    # -- lifecycle -----------------------------------------------------------

    @staticmethod
    def _tcp_probe(host: str, port: int, timeout: float = 3.0) -> bool:
        """Quick TCP connect check — returns True if port is reachable."""
        import socket
        try:
            with socket.create_connection((host, port), timeout=timeout):
                return True
        except (OSError, TimeoutError):
            return False

    async def connect(
        self, host: str, port: int, username: str, password: str
    ) -> None:
        self._device_id = f"{host}:{port}"

        # Pre-check: verify SDK port is reachable before calling into DLL.
        # This prevents SDK DLL crashes/hangs on unreachable hosts.
        loop = asyncio.get_running_loop()
        reachable = await loop.run_in_executor(
            None, self._tcp_probe, host, port,
        )
        if not reachable:
            raise ConnectionError(f"SDK port {host}:{port} is not reachable")

        user_id, login_info = await self._run(
            self._binding.login, host, port, username, password
        )
        self._user_id = user_id
        self._login_info = login_info

    async def disconnect(self) -> None:
        if self._user_id >= 0:
            await self._run(self._binding.logout, self._user_id)
            self._user_id = -1

    # -- queries -------------------------------------------------------------

    async def get_device_info(self) -> dict:
        config = await self._run(
            self._binding.get_device_config, self._user_id
        )
        return {"sdk_data": config}

    async def get_channels_status(self) -> list[dict]:
        ip_chan_num = self._login_info.get("ip_chan_num", 0)
        chan_num = self._login_info.get("chan_num", 0)
        start_dchan = self._login_info.get("start_dchan", 33)

        logger.debug(
            "sdk.channels_info",
            ip_chan_num=ip_chan_num, chan_num=chan_num,
            start_dchan=start_dchan,
            device=self._device_id,
        )

        # Primary: tunnel ISAPI through SDK to get full channel status
        channels = await self._get_channels_via_isapi_tunnel(
            ip_chan_num, start_dchan,
        )
        if channels:
            return channels

        # Fallback: cmd 6126 digital channel state
        channels = await self._get_channels_via_cmd6126(
            ip_chan_num, start_dchan,
        )

        # Analog channels
        start_chan = self._login_info.get("start_chan", 1)
        for i in range(chan_num):
            ch_id = start_chan + i
            channels.append(
                {
                    "sdk_data": {
                        "channel_id": str(ch_id),
                        "channel_name": f"Analog {ch_id}",
                        "online": None,
                        "source": "sdk_login_info",
                    }
                }
            )
        return channels

    async def _get_channels_via_isapi_tunnel(
        self, ip_chan_num: int, start_dchan: int,
    ) -> list[dict]:
        """Get IP channel status via ISAPI tunneled through SDK."""
        import re

        try:
            xml_data = await self._run(
                self._binding.std_xml_config,
                self._user_id,
                "GET /ISAPI/ContentMgmt/InputProxy/channels/status",
            )
        except Exception as exc:
            logger.warning(
                "sdk.isapi_tunnel_failed",
                error=str(exc), device=self._device_id,
            )
            return []

        if not xml_data:
            return []

        # Parse channel blocks from XML
        blocks = re.findall(
            r"<InputProxyChannelStatus[^>]*>(.*?)</InputProxyChannelStatus>",
            xml_data, re.DOTALL | re.IGNORECASE,
        )

        configured: dict[int, dict] = {}
        for block in blocks:
            id_m = re.search(r"<id>(\d+)</id>", block)
            if not id_m:
                continue
            ch_id = int(id_m.group(1))

            online_m = re.search(r"<online>([^<]+)</online>", block)
            is_online = online_m is not None and online_m.group(1).lower() == "true"

            ip_m = re.search(r"<ipAddress>([^<]+)</ipAddress>", block)
            ip_addr = ip_m.group(1) if ip_m else None

            name_m = re.search(r"<name>([^<]+)</name>", block)
            name = name_m.group(1) if name_m else None

            configured[ch_id] = {
                "online": is_online,
                "ip_address": ip_addr,
                "channel_name": name,
            }

        logger.info(
            "sdk.isapi_tunnel_channels",
            total_blocks=len(blocks),
            configured=len(configured),
            device=self._device_id,
        )

        # Build result list for all IP channel slots
        channels: list[dict] = []
        for slot in range(1, ip_chan_num + 1):
            ch_id = start_dchan + slot - 1
            ch_data = configured.get(slot, None)
            channels.append(
                {
                    "sdk_data": {
                        "channel_id": str(ch_id),
                        "channel_name": ch_data["channel_name"] or f"Channel {ch_id}" if ch_data else f"Channel {ch_id}",
                        "online": ch_data["online"] if ch_data else None,
                        "ip_address": ch_data["ip_address"] if ch_data else None,
                        "source": "isapi_via_sdk",
                    }
                }
            )
        return channels

    async def _get_channels_via_cmd6126(
        self, ip_chan_num: int, start_dchan: int,
    ) -> list[dict]:
        """Fallback: get channel status via NET_DVR_GET_DIGITAL_CHANNEL_STATE."""
        online_map: dict[str, bool | None] = {}
        if ip_chan_num > 0:
            try:
                digital_state = await self._run(
                    self._binding.get_digital_channel_state,
                    self._user_id, start_dchan, ip_chan_num,
                )
                for ds in digital_state:
                    online_map[ds["channel_id"]] = ds["online"]
                logger.info(
                    "sdk.cmd6126_ok",
                    count=len(digital_state),
                    device=self._device_id,
                )
            except Exception as exc:
                logger.warning(
                    "sdk.cmd6126_failed",
                    error=str(exc), device=self._device_id,
                )

        channels: list[dict] = []
        for i in range(ip_chan_num):
            ch_id = start_dchan + i
            channels.append(
                {
                    "sdk_data": {
                        "channel_id": str(ch_id),
                        "channel_name": f"Channel {ch_id}",
                        "online": online_map.get(str(ch_id)),
                        "ip_address": None,
                        "source": "sdk_cmd6126",
                    }
                }
            )
        return channels

    async def get_disk_status(self) -> list[dict]:
        disks = await self._run(
            self._binding.get_hdd_config, self._user_id
        )
        return [{"sdk_data": disk} for disk in disks]

    async def get_video_inputs(self) -> dict:
        raise NotImplementedError("get_video_inputs is not supported via SDK transport")

    async def get_recording_status(self) -> list[dict]:
        raise NotImplementedError("get_recording_status is not supported via SDK transport")

    async def get_snapshot(self, channel_id: str) -> bytes:
        # channel_id is already the SDK channel number (e.g. "33" for first IP channel)
        return await self._run(self._binding.capture_jpeg, self._user_id, int(channel_id))
