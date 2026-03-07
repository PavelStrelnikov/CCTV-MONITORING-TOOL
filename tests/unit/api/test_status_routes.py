import pytest
from unittest.mock import AsyncMock, MagicMock
from fastapi.testclient import TestClient
from cctv_monitor.api.app import create_app
from cctv_monitor.api.deps import get_session

@pytest.fixture
def client():
    app = create_app()
    mock_session = AsyncMock()
    async def override():
        yield mock_session
    app.dependency_overrides[get_session] = override
    return TestClient(app), mock_session

def test_overview_empty(client):
    test_client, mock_session = client
    mock_result = MagicMock()
    mock_result.scalars.return_value.all.return_value = []
    mock_session.execute.return_value = mock_result
    response = test_client.get("/api/overview")
    assert response.status_code == 200
    data = response.json()
    assert data["total_devices"] == 0
    assert data["disks_ok"] is True
