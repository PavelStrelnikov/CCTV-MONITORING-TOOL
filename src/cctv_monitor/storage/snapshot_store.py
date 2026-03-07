import asyncio
from datetime import datetime, timezone
from pathlib import Path


class SnapshotStore:
    def __init__(self, base_dir: str) -> None:
        self._base_dir = Path(base_dir)

    async def save(
        self, device_id: str, channel_id: str, image_data: bytes
    ) -> str:
        now = datetime.now(timezone.utc)
        directory = self._base_dir / device_id / channel_id
        filename = now.strftime("%Y%m%d_%H%M%S") + ".jpg"
        file_path = directory / filename

        await asyncio.to_thread(self._write_file, file_path, image_data)
        return str(file_path)

    @staticmethod
    def _write_file(path: Path, data: bytes) -> None:
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(data)
