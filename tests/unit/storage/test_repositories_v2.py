import pytest
from unittest.mock import AsyncMock, MagicMock, ANY
from cctv_monitor.storage.repositories import (
    DeviceTagRepository,
    DeviceHealthLogRepository,
    AlertRepository,
)


@pytest.fixture
def mock_session():
    return AsyncMock()


# ── DeviceTagRepository ──────────────────────────────────────────────


@pytest.mark.asyncio
async def test_add_tag(mock_session):
    repo = DeviceTagRepository(mock_session)
    await repo.add_tag("nvr-01", "office")
    mock_session.add.assert_called_once()
    added = mock_session.add.call_args[0][0]
    assert added.device_id == "nvr-01"
    assert added.tag == "office"


@pytest.mark.asyncio
async def test_get_tags(mock_session):
    mock_result = MagicMock()
    mock_result.scalars.return_value.all.return_value = ["office", "floor2"]
    mock_session.execute.return_value = mock_result
    repo = DeviceTagRepository(mock_session)
    tags = await repo.get_tags("nvr-01")
    assert tags == ["office", "floor2"]


@pytest.mark.asyncio
async def test_remove_tag_found(mock_session):
    mock_result = MagicMock()
    mock_result.rowcount = 1
    mock_session.execute.return_value = mock_result
    repo = DeviceTagRepository(mock_session)
    removed = await repo.remove_tag("nvr-01", "office")
    assert removed is True


@pytest.mark.asyncio
async def test_remove_tag_not_found(mock_session):
    mock_result = MagicMock()
    mock_result.rowcount = 0
    mock_session.execute.return_value = mock_result
    repo = DeviceTagRepository(mock_session)
    removed = await repo.remove_tag("nvr-01", "nonexistent")
    assert removed is False


@pytest.mark.asyncio
async def test_get_all_unique_tags(mock_session):
    mock_result = MagicMock()
    mock_result.scalars.return_value.all.return_value = ["office", "warehouse"]
    mock_session.execute.return_value = mock_result
    repo = DeviceTagRepository(mock_session)
    tags = await repo.get_all_unique_tags()
    assert tags == ["office", "warehouse"]


# ── DeviceHealthLogRepository ────────────────────────────────────────


@pytest.mark.asyncio
async def test_insert_health_log(mock_session):
    repo = DeviceHealthLogRepository(mock_session)
    await repo.insert(
        device_id="nvr-01",
        reachable=True,
        camera_count=4,
        online_cameras=3,
        offline_cameras=1,
        disk_ok=True,
        response_time_ms=120.5,
    )
    mock_session.add.assert_called_once()
    added = mock_session.add.call_args[0][0]
    assert added.device_id == "nvr-01"
    assert added.reachable is True
    assert added.camera_count == 4
    assert added.online_cameras == 3
    assert added.offline_cameras == 1
    assert added.disk_ok is True
    assert added.response_time_ms == 120.5
    assert added.checked_at is not None


# ── AlertRepository ──────────────────────────────────────────────────


@pytest.mark.asyncio
async def test_get_all_alerts_no_filter(mock_session):
    mock_result = MagicMock()
    mock_result.scalars.return_value.all.return_value = ["alert1", "alert2"]
    mock_session.execute.return_value = mock_result
    repo = AlertRepository(mock_session)
    alerts = await repo.get_all_alerts()
    assert len(alerts) == 2


@pytest.mark.asyncio
async def test_get_all_alerts_with_status(mock_session):
    mock_result = MagicMock()
    mock_result.scalars.return_value.all.return_value = ["alert1"]
    mock_session.execute.return_value = mock_result
    repo = AlertRepository(mock_session)
    alerts = await repo.get_all_alerts(status="active")
    assert len(alerts) == 1
