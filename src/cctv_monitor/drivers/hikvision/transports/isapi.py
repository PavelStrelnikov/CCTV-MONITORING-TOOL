"""ISAPI transport for Hikvision devices using httpx with Digest Auth."""

import httpx

from cctv_monitor.core.http_client import HttpClientManager
from cctv_monitor.drivers.hikvision.errors import IsapiAuthError, IsapiError
from cctv_monitor.drivers.hikvision.transports.base import HikvisionTransport


class IsapiTransport(HikvisionTransport):
    """Hikvision ISAPI transport over HTTP(S) with Digest authentication."""

    DEVICE_INFO = "/ISAPI/System/deviceInfo"
    CHANNELS_STATUS = "/ISAPI/ContentMgmt/InputProxy/channels/status"
    HDD_STATUS = "/ISAPI/ContentMgmt/Storage/hdd"
    SNAPSHOT = "/ISAPI/Streaming/channels/{channel_id}/picture"
    RECORDING_STATUS = "/ISAPI/ContentMgmt/record/tracks"

    def __init__(self, client_manager: HttpClientManager) -> None:
        self._client_manager = client_manager
        self._base_url: str = ""
        self._auth: httpx.DigestAuth | None = None
        self._device_id: str = ""

    async def connect(self, host: str, port: int, username: str, password: str) -> None:
        scheme = "https" if port == 443 else "http"
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

    async def get_disk_status(self) -> list[dict]:
        response = await self._request("GET", self.HDD_STATUS)
        return [{"raw_xml": response.text}]

    async def get_recording_status(self) -> list[dict]:
        response = await self._request("GET", self.RECORDING_STATUS)
        return [{"raw_xml": response.text}]

    async def get_snapshot(self, channel_id: str) -> bytes:
        path = self.SNAPSHOT.format(channel_id=channel_id)
        response = await self._request("GET", path)
        return response.content
