from abc import ABC, abstractmethod


class HikvisionTransport(ABC):
    @abstractmethod
    async def connect(self, host: str, port: int, username: str, password: str, *, protocol: str = "http") -> None: ...

    @abstractmethod
    async def disconnect(self) -> None: ...

    @abstractmethod
    async def get_device_info(self) -> dict: ...

    @abstractmethod
    async def get_channels_status(self) -> list[dict]: ...

    @abstractmethod
    async def get_video_inputs(self) -> dict: ...

    @abstractmethod
    async def get_disk_status(self) -> list[dict]: ...

    @abstractmethod
    async def get_recording_status(self) -> list[dict]: ...

    @abstractmethod
    async def get_snapshot(self, channel_id: str) -> bytes: ...
