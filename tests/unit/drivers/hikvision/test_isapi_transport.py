"""Tests for IsapiTransport."""

from pathlib import Path
from unittest.mock import AsyncMock, MagicMock

import httpx
import pytest

from cctv_monitor.drivers.hikvision.errors import IsapiAuthError, IsapiError
from cctv_monitor.drivers.hikvision.transports.isapi import IsapiTransport

FIXTURES = Path(__file__).resolve().parents[3] / "fixtures" / "hikvision"


@pytest.fixture
def mock_client_manager():
    """Return (manager_mock, client_mock) with async request mock."""
    client = MagicMock(spec=httpx.AsyncClient)
    client.request = AsyncMock()
    manager = AsyncMock()
    manager.get_client = AsyncMock(return_value=client)
    return manager, client


@pytest.fixture
async def transport(mock_client_manager):
    """Return a connected IsapiTransport."""
    manager, _ = mock_client_manager
    t = IsapiTransport(manager)
    await t.connect("192.168.1.100", 80, "admin", "password123")
    return t


def _make_response(status_code: int = 200, text: str = "", content: bytes = b"") -> MagicMock:
    resp = MagicMock(spec=httpx.Response)
    resp.status_code = status_code
    resp.text = text
    resp.content = content
    return resp


@pytest.mark.asyncio
async def test_connect_sets_base_url_and_auth(mock_client_manager):
    manager, _ = mock_client_manager
    t = IsapiTransport(manager)
    await t.connect("192.168.1.100", 80, "admin", "pass")

    assert t._base_url == "http://192.168.1.100:80"
    assert isinstance(t._auth, httpx.DigestAuth)


@pytest.mark.asyncio
async def test_connect_https_on_port_443(mock_client_manager):
    manager, _ = mock_client_manager
    t = IsapiTransport(manager)
    await t.connect("192.168.1.100", 443, "admin", "pass")

    assert t._base_url == "https://192.168.1.100:443"


@pytest.mark.asyncio
async def test_get_device_info_returns_raw_xml(mock_client_manager):
    manager, client = mock_client_manager
    t = IsapiTransport(manager)
    await t.connect("192.168.1.100", 80, "admin", "pass")

    xml_text = (FIXTURES / "device_info.xml").read_text()
    client.request.return_value = _make_response(text=xml_text)

    result = await t.get_device_info()
    assert result == {"raw_xml": xml_text}


@pytest.mark.asyncio
async def test_get_channels_status_returns_raw_xml(mock_client_manager):
    manager, client = mock_client_manager
    t = IsapiTransport(manager)
    await t.connect("192.168.1.100", 80, "admin", "pass")

    xml_text = (FIXTURES / "channels_status.xml").read_text()
    client.request.return_value = _make_response(text=xml_text)

    result = await t.get_channels_status()
    assert result == [{"raw_xml": xml_text}]


@pytest.mark.asyncio
async def test_get_disk_status_returns_raw_xml(mock_client_manager):
    manager, client = mock_client_manager
    t = IsapiTransport(manager)
    await t.connect("192.168.1.100", 80, "admin", "pass")

    xml_text = (FIXTURES / "hdd_status.xml").read_text()
    client.request.return_value = _make_response(text=xml_text)

    result = await t.get_disk_status()
    assert result == [{"raw_xml": xml_text}]


@pytest.mark.asyncio
async def test_get_snapshot_returns_bytes(mock_client_manager):
    manager, client = mock_client_manager
    t = IsapiTransport(manager)
    await t.connect("192.168.1.100", 80, "admin", "pass")

    fake_jpeg = b"\xff\xd8\xff\xe0FAKE_JPEG_DATA"
    client.request.return_value = _make_response(content=fake_jpeg)

    result = await t.get_snapshot("101")
    assert result == fake_jpeg


@pytest.mark.asyncio
async def test_auth_error_raises_isapi_auth_error(mock_client_manager):
    manager, client = mock_client_manager
    t = IsapiTransport(manager)
    await t.connect("192.168.1.100", 80, "admin", "wrong")

    client.request.return_value = _make_response(status_code=401)

    with pytest.raises(IsapiAuthError):
        await t.get_device_info()


@pytest.mark.asyncio
async def test_server_error_raises_isapi_error(mock_client_manager):
    manager, client = mock_client_manager
    t = IsapiTransport(manager)
    await t.connect("192.168.1.100", 80, "admin", "pass")

    client.request.return_value = _make_response(status_code=500, text="Internal Server Error")

    with pytest.raises(IsapiError) as exc_info:
        await t.get_device_info()
    assert exc_info.value.status_code == 500


@pytest.mark.asyncio
async def test_disconnect_clears_auth(mock_client_manager):
    manager, _ = mock_client_manager
    t = IsapiTransport(manager)
    await t.connect("192.168.1.100", 80, "admin", "pass")
    assert t._auth is not None

    await t.disconnect()
    assert t._auth is None


@pytest.mark.asyncio
async def test_snapshot_url_includes_channel_id(mock_client_manager):
    manager, client = mock_client_manager
    t = IsapiTransport(manager)
    await t.connect("192.168.1.100", 80, "admin", "pass")

    client.request.return_value = _make_response(content=b"img")

    await t.get_snapshot("102")

    call_args = client.request.call_args
    url = call_args[1].get("url", call_args[0][1] if len(call_args[0]) > 1 else None)
    assert "102" in url
