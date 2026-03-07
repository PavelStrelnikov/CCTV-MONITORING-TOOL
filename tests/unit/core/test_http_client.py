from cctv_monitor.core.http_client import HttpClientManager


async def test_get_client_returns_same_instance():
    manager = HttpClientManager()
    try:
        client1 = await manager.get_client()
        client2 = await manager.get_client()
        assert client1 is client2
    finally:
        await manager.close()


async def test_close_sets_client_to_none():
    manager = HttpClientManager()
    await manager.get_client()
    await manager.close()
    # After close, next get_client creates a new instance
    client = await manager.get_client()
    assert client is not None
    await manager.close()


async def test_client_has_timeout():
    manager = HttpClientManager(connect_timeout=5.0, read_timeout=15.0)
    try:
        client = await manager.get_client()
        assert client.timeout.connect == 5.0
        assert client.timeout.read == 15.0
    finally:
        await manager.close()
