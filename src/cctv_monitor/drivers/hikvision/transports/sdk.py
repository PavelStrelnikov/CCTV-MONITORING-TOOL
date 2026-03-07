from cctv_monitor.drivers.hikvision.transports.base import HikvisionTransport
from cctv_monitor.drivers.hikvision.errors import SdkError


class SdkTransport(HikvisionTransport):
    """Stub SDK transport — not yet implemented.

    Will use ctypes to interface with HCNetSDK.
    See docs/vendors/hikvision/SDK_INTEGRATION_PLAN.md
    """

    async def connect(self, host: str, port: int, username: str, password: str) -> None:
        raise SdkError(
            device_id=f"{host}:{port}",
            error_code=-1,
            message="SDK transport not yet implemented",
        )

    async def disconnect(self) -> None:
        pass

    async def get_device_info(self) -> dict:
        raise NotImplementedError("SDK transport not yet implemented")

    async def get_channels_status(self) -> list[dict]:
        raise NotImplementedError("SDK transport not yet implemented")

    async def get_video_inputs(self) -> dict:
        raise NotImplementedError("SDK transport not yet implemented")

    async def get_disk_status(self) -> list[dict]:
        raise NotImplementedError("SDK transport not yet implemented")

    async def get_recording_status(self) -> list[dict]:
        raise NotImplementedError("SDK transport not yet implemented")

    async def get_snapshot(self, channel_id: str) -> bytes:
        raise NotImplementedError("SDK transport not yet implemented")
