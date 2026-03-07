import httpx


class HttpClientManager:
    def __init__(
        self,
        connect_timeout: float = 10.0,
        read_timeout: float = 30.0,
        pool_timeout: float = 10.0,
    ) -> None:
        self._connect_timeout = connect_timeout
        self._read_timeout = read_timeout
        self._pool_timeout = pool_timeout
        self._client: httpx.AsyncClient | None = None

    async def get_client(self) -> httpx.AsyncClient:
        if self._client is None or self._client.is_closed:
            self._client = httpx.AsyncClient(
                timeout=httpx.Timeout(
                    connect=self._connect_timeout,
                    read=self._read_timeout,
                    write=self._read_timeout,
                    pool=self._pool_timeout,
                ),
                follow_redirects=True,
                verify=False,  # CCTV devices often use self-signed certs
            )
        return self._client

    async def close(self) -> None:
        if self._client is not None and not self._client.is_closed:
            await self._client.aclose()
            self._client = None
