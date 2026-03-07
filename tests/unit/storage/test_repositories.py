import pytest
from unittest.mock import AsyncMock, MagicMock
from cctv_monitor.storage.repositories import DeviceRepository

@pytest.fixture
def mock_session():
    return AsyncMock()

@pytest.mark.asyncio
async def test_list_all(mock_session):
    mock_result = MagicMock()
    mock_result.scalars.return_value.all.return_value = ["device1", "device2"]
    mock_session.execute.return_value = mock_result
    repo = DeviceRepository(mock_session)
    result = await repo.list_all()
    assert len(result) == 2

@pytest.mark.asyncio
async def test_create(mock_session):
    repo = DeviceRepository(mock_session)
    device = MagicMock()
    await repo.create(device)
    mock_session.add.assert_called_once_with(device)
    mock_session.flush.assert_called_once()

@pytest.mark.asyncio
async def test_delete(mock_session):
    mock_device = MagicMock()
    mock_result = MagicMock()
    mock_result.scalar_one_or_none.return_value = mock_device
    mock_session.execute.return_value = mock_result
    repo = DeviceRepository(mock_session)
    deleted = await repo.delete("nvr-01")
    assert deleted is True
    mock_session.delete.assert_called_once_with(mock_device)

@pytest.mark.asyncio
async def test_delete_not_found(mock_session):
    mock_result = MagicMock()
    mock_result.scalar_one_or_none.return_value = None
    mock_session.execute.return_value = mock_result
    repo = DeviceRepository(mock_session)
    deleted = await repo.delete("missing")
    assert deleted is False
