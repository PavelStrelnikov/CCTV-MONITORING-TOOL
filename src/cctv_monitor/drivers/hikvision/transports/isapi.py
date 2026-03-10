"""ISAPI transport for Hikvision devices using httpx with Digest Auth."""

import logging

import httpx

from cctv_monitor.core.http_client import HttpClientManager
from cctv_monitor.drivers.hikvision.errors import IsapiAuthError, IsapiError
from cctv_monitor.drivers.hikvision.transports.base import HikvisionTransport

logger = logging.getLogger(__name__)


class IsapiTransport(HikvisionTransport):
    """Hikvision ISAPI transport over HTTP(S) with Digest authentication."""

    DEVICE_INFO = "/ISAPI/System/deviceInfo"
    CHANNELS_STATUS = "/ISAPI/ContentMgmt/InputProxy/channels/status"
    VIDEO_INPUTS = "/ISAPI/System/Video/inputs/channels"
    HDD_STATUS = "/ISAPI/ContentMgmt/Storage/hdd"
    SNAPSHOT_PROXY = "/ISAPI/ContentMgmt/StreamingProxy/channels/{channel_id}/picture"
    SNAPSHOT_DIRECT = "/ISAPI/Streaming/channels/{channel_id}/picture"
    RECORDING_STATUS = "/ISAPI/ContentMgmt/record/tracks"

    def __init__(self, client_manager: HttpClientManager) -> None:
        self._client_manager = client_manager
        self._base_url: str = ""
        self._auth: httpx.DigestAuth | None = None
        self._device_id: str = ""

    async def connect(self, host: str, port: int, username: str, password: str) -> None:
        scheme = "https" if port % 1000 == 443 else "http"
        self._base_url = f"{scheme}://{host}:{port}"
        self._auth = httpx.DigestAuth(username, password)
        self._device_id = f"{host}:{port}"

    async def disconnect(self) -> None:
        self._auth = None

    async def _request(self, method: str, path: str, **kwargs: object) -> httpx.Response:
        client = await self._client_manager.get_client()
        url = f"{self._base_url}{path}"
        response = await client.request(method, url=url, auth=self._auth, **kwargs)

        if response.status_code == 401:
            raise IsapiAuthError(device_id=self._device_id)
        if response.status_code >= 400:
            raise IsapiError(
                device_id=self._device_id,
                status_code=response.status_code,
                message=response.text,
            )
        return response

    async def get_device_info(self) -> dict:
        response = await self._request("GET", self.DEVICE_INFO)
        return {"raw_xml": response.text}

    async def get_channels_status(self) -> list[dict]:
        response = await self._request("GET", self.CHANNELS_STATUS)
        return [{"raw_xml": response.text}]

    async def get_video_inputs(self) -> dict:
        response = await self._request("GET", self.VIDEO_INPUTS)
        return {"raw_xml": response.text}

    async def get_disk_status(self) -> list[dict]:
        response = await self._request("GET", self.HDD_STATUS)
        return [{"raw_xml": response.text}]

    async def get_disk_smart(self, disk_id: int) -> dict | None:
        """Get SMART test status for a specific disk. Returns raw XML or None."""
        try:
            response = await self._request(
                "GET", f"/ISAPI/ContentMgmt/Storage/hdd/{disk_id}/SMARTTest/status",
            )
            return {"raw_xml": response.text}
        except Exception:
            return None

    async def get_recording_status(self) -> list[dict]:
        response = await self._request("GET", self.RECORDING_STATUS)
        return [{"raw_xml": response.text}]

    async def search_recordings(self, track_id: int, start_time: str, end_time: str) -> dict | None:
        """Search for recordings on a channel in a time window.

        Args:
            track_id: Hikvision track ID (channel * 100 + stream), e.g. 301, 3301
            start_time: ISO format e.g. "2026-03-08T00:00:00Z"
            end_time: ISO format e.g. "2026-03-08T14:00:00Z"

        Returns dict with raw_xml or None on error.
        """
        import uuid
        search_id = str(uuid.uuid4()).upper()
        body = (
            '<?xml version="1.0" encoding="UTF-8"?>\n'
            "<CMSearchDescription>\n"
            f"<searchID>{search_id}</searchID>\n"
            f"<trackIDList><trackID>{track_id}</trackID></trackIDList>\n"
            "<timeSpanList>\n"
            "<timeSpan>\n"
            f"<startTime>{start_time}</startTime>\n"
            f"<endTime>{end_time}</endTime>\n"
            "</timeSpan>\n"
            "</timeSpanList>\n"
            "<maxResults>1</maxResults>\n"
            "<searchResultPosition>0</searchResultPosition>\n"
            "<metadataList>\n"
            "<metadataDescriptor>//recordType.meta.std-cgi.com</metadataDescriptor>\n"
            "</metadataList>\n"
            "</CMSearchDescription>"
        )
        try:
            response = await self._request(
                "POST", "/ISAPI/ContentMgmt/search",
                content=body,
                headers={"Content-Type": "text/xml"},
            )
            return {"raw_xml": response.text}
        except Exception:
            return None

    async def get_device_time(self) -> dict:
        """Get device time from /ISAPI/System/time."""
        response = await self._request("GET", "/ISAPI/System/time")
        return {"raw_xml": response.text}

    async def get_snapshot(self, channel_id: str) -> bytes:
        # Different Hikvision models expose snapshot channels in different formats:
        # 1) Sequential track format: 101, 201, ...
        # 2) Raw channel index: 1, 2, ...
        # 3) Digital SDK-like index: 3301, 3401, ...
        # We try multiple candidates to make snapshots resilient across DVR/NVR variants.
        candidates: list[str] = []
        try:
            raw_num = int(channel_id)
            candidates.append(str(raw_num))  # raw ID from API
            candidates.append(str(raw_num * 100 + 1))  # main stream
            candidates.append(str(raw_num * 100 + 2))  # sub stream
            if raw_num >= 33:
                seq = raw_num - 32
                candidates.append(str(seq))
                candidates.append(str(seq * 100 + 1))
                candidates.append(str(seq * 100 + 2))
        except (ValueError, TypeError):
            candidates.append(channel_id)

        # Deduplicate while preserving order.
        seen: set[str] = set()
        channel_candidates = [c for c in candidates if c and not (c in seen or seen.add(c))]

        client = await self._client_manager.get_client()
        snap_timeout = httpx.Timeout(connect=5.0, read=15.0, write=5.0, pool=10.0)

        response: httpx.Response | None = None
        # Try StreamingProxy first (works on many NVRs), then direct endpoint.
        for candidate in channel_candidates:
            for template in (self.SNAPSHOT_PROXY, self.SNAPSHOT_DIRECT):
                path = template.format(channel_id=candidate)
                url = f"{self._base_url}{path}"
                response = await client.request(
                    "GET", url=url, auth=self._auth, timeout=snap_timeout,
                )
                if response.status_code == 401:
                    raise IsapiAuthError(device_id=self._device_id)
                if response.status_code < 400:
                    return response.content
                logger.debug(
                    "snapshot attempt %s channel_id=%s candidate=%s -> HTTP %s",
                    path, channel_id, candidate, response.status_code,
                )

        raise IsapiError(
            device_id=self._device_id,
            status_code=response.status_code if response is not None else 500,
            message=(
                f"Snapshot ch={channel_id} failed for candidates={channel_candidates}; "
                f"HTTP {response.status_code if response is not None else 'n/a'}"
            ),
        )
