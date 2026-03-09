import pytest
from unittest.mock import AsyncMock, MagicMock
from fastapi.testclient import TestClient
from cctv_monitor.api.app import create_app
from cctv_monitor.api.deps import get_session, get_settings, get_driver_registry, get_http_client

@pytest.fixture
def mock_session():
    return AsyncMock()

@pytest.fixture
def mock_settings():
    settings = MagicMock()
    settings.ENCRYPTION_KEY = "dGVzdGtleXRlc3RrZXl0ZXN0a2V5dGVzdGtleTE2Mzg="
    return settings

@pytest.fixture
def client(mock_session, mock_settings):
    app = create_app()
    async def override_session():
        yield mock_session
    app.dependency_overrides[get_session] = override_session
    app.dependency_overrides[get_settings] = lambda: mock_settings
    app.dependency_overrides[get_driver_registry] = lambda: MagicMock()
    app.dependency_overrides[get_http_client] = lambda: MagicMock()
    return TestClient(app)

def test_list_devices_empty(client, mock_session):
    mock_result = MagicMock()
    mock_result.scalars.return_value.all.return_value = []
    mock_session.execute.return_value = mock_result
    response = client.get("/api/devices")
    assert response.status_code == 200
    assert response.json() == []

def test_list_devices_with_data(client, mock_session):
    device = MagicMock()
    device.device_id = "nvr-01"
    device.name = "Test NVR"
    device.vendor = "hikvision"
    device.host = "192.168.1.100"
    device.web_port = 80
    device.sdk_port = None
    device.transport_mode = "isapi"
    device.is_active = True
    device.model = None
    device.serial_number = None
    device.firmware_version = None
    device.last_poll_at = None

    # First execute call returns device list, second returns tags (empty)
    mock_result_devices = MagicMock()
    mock_result_devices.scalars.return_value.all.return_value = [device]
    mock_result_tags = MagicMock()
    mock_result_tags.scalars.return_value.all.return_value = []
    mock_session.execute.side_effect = [mock_result_devices, mock_result_tags]

    response = client.get("/api/devices")
    assert response.status_code == 200
    data = response.json()
    assert len(data) == 1
    assert data[0]["device_id"] == "nvr-01"

def test_create_device(client, mock_session, mock_settings):
    mock_session.flush = AsyncMock()
    mock_session.commit = AsyncMock()
    response = client.post("/api/devices", json={
        "device_id": "nvr-02", "name": "New NVR", "vendor": "hikvision",
        "host": "10.0.0.1", "web_port": 8443, "sdk_port": None, "username": "admin", "password": "secret123",
    })
    assert response.status_code == 201
    data = response.json()
    assert data["device_id"] == "nvr-02"
    assert data["is_active"] is True

def test_create_device_duplicate(client, mock_session, mock_settings):
    from sqlalchemy.exc import IntegrityError
    mock_session.flush = AsyncMock(side_effect=IntegrityError("dup", {}, None))
    mock_session.rollback = AsyncMock()
    response = client.post("/api/devices", json={
        "device_id": "nvr-01", "name": "Dup", "vendor": "hikvision",
        "host": "10.0.0.1", "web_port": 80, "sdk_port": None, "username": "admin", "password": "pass",
    })
    assert response.status_code == 409
