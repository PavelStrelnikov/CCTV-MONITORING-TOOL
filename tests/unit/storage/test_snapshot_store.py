import pytest
from pathlib import Path
from cctv_monitor.storage.snapshot_store import SnapshotStore


@pytest.fixture
def store(tmp_path):
    return SnapshotStore(base_dir=str(tmp_path))


async def test_save_snapshot(store, tmp_path):
    image_data = b"\xff\xd8\xff\xe0" + b"\x00" * 100  # fake JPEG
    path = await store.save(
        device_id="nvr-01",
        channel_id="101",
        image_data=image_data,
    )
    assert Path(path).exists()
    assert Path(path).read_bytes() == image_data
    assert "nvr-01" in path
    assert "101" in path


async def test_save_creates_directory(store, tmp_path):
    image_data = b"\xff\xd8\xff\xe0"
    path = await store.save("nvr-99", "201", image_data)
    assert Path(path).parent.exists()


async def test_save_uses_jpg_extension(store):
    image_data = b"\xff\xd8\xff\xe0"
    path = await store.save("nvr-01", "101", image_data)
    assert path.endswith(".jpg")
