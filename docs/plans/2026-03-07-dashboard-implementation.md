# Dashboard MVP Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a web dashboard (React SPA + FastAPI REST API) to manage CCTV devices and view their statuses.

**Architecture:** React SPA (Vite + TypeScript) talks to FastAPI REST API via `/api/*` endpoints. FastAPI uses existing SQLAlchemy models, repositories, and Hikvision driver. Vite dev server proxies API requests to FastAPI during development.

**Tech Stack:** Python 3.12, FastAPI, SQLAlchemy 2.0 async, asyncpg, React 18, Vite 5, TypeScript, react-router-dom v6

---

### Task 1: API Pydantic Schemas

**Files:**
- Create: `src/cctv_monitor/api/schemas.py`
- Test: `tests/unit/api/test_schemas.py`

**Step 1: Write the test**

```python
# tests/unit/api/test_schemas.py
from datetime import datetime, timezone

from cctv_monitor.api.schemas import (
    DeviceCreate,
    DeviceOut,
    HealthSummaryOut,
    CameraChannelOut,
    DiskOut,
    DeviceDetailOut,
    PollResultOut,
    OverviewOut,
)


def test_device_create_valid():
    d = DeviceCreate(
        device_id="nvr-01",
        name="Test NVR",
        vendor="hikvision",
        host="192.168.1.100",
        port=80,
        username="admin",
        password="secret",
    )
    assert d.device_id == "nvr-01"
    assert d.port == 80


def test_device_create_default_port():
    d = DeviceCreate(
        device_id="nvr-01",
        name="Test",
        vendor="hikvision",
        host="10.0.0.1",
        username="admin",
        password="pass",
    )
    assert d.port == 80


def test_device_out_with_health():
    now = datetime.now(timezone.utc)
    h = HealthSummaryOut(
        reachable=True,
        camera_count=4,
        online_cameras=3,
        offline_cameras=1,
        disk_ok=True,
        response_time_ms=120.5,
        checked_at=now,
    )
    d = DeviceOut(
        device_id="nvr-01",
        name="Test",
        vendor="hikvision",
        host="10.0.0.1",
        port=80,
        is_active=True,
        last_health=h,
    )
    assert d.last_health.camera_count == 4


def test_device_out_without_health():
    d = DeviceOut(
        device_id="nvr-01",
        name="Test",
        vendor="hikvision",
        host="10.0.0.1",
        port=80,
        is_active=True,
        last_health=None,
    )
    assert d.last_health is None


def test_overview_out():
    o = OverviewOut(
        total_devices=7,
        reachable_devices=5,
        total_cameras=20,
        online_cameras=18,
        disks_ok=True,
    )
    assert o.total_devices == 7
```

**Step 2: Run test to verify it fails**

Run: `python -m pytest tests/unit/api/test_schemas.py -v`
Expected: FAIL — cannot import `schemas`

**Step 3: Write implementation**

```python
# src/cctv_monitor/api/schemas.py
from datetime import datetime

from pydantic import BaseModel


class DeviceCreate(BaseModel):
    device_id: str
    name: str
    vendor: str
    host: str
    port: int = 80
    username: str
    password: str


class HealthSummaryOut(BaseModel):
    reachable: bool
    camera_count: int
    online_cameras: int
    offline_cameras: int
    disk_ok: bool
    response_time_ms: float
    checked_at: datetime


class DeviceOut(BaseModel):
    device_id: str
    name: str
    vendor: str
    host: str
    port: int
    is_active: bool
    last_health: HealthSummaryOut | None = None


class CameraChannelOut(BaseModel):
    channel_id: str
    channel_name: str
    status: str
    ip_address: str | None = None
    checked_at: datetime


class DiskOut(BaseModel):
    disk_id: str
    status: str
    capacity_bytes: int
    free_bytes: int
    health_status: str
    checked_at: datetime


class AlertOut(BaseModel):
    id: int
    alert_type: str
    severity: str
    message: str
    status: str
    created_at: datetime
    resolved_at: datetime | None = None


class DeviceDetailOut(BaseModel):
    device: DeviceOut
    cameras: list[CameraChannelOut] = []
    disks: list[DiskOut] = []
    alerts: list[AlertOut] = []


class PollResultOut(BaseModel):
    device_id: str
    health: HealthSummaryOut


class OverviewOut(BaseModel):
    total_devices: int
    reachable_devices: int
    total_cameras: int
    online_cameras: int
    disks_ok: bool
```

**Step 4: Run test to verify it passes**

Run: `python -m pytest tests/unit/api/test_schemas.py -v`
Expected: PASS (5 tests)

**Step 5: Commit**

```bash
git add src/cctv_monitor/api/schemas.py tests/unit/api/test_schemas.py
git commit -m "feat(api): add Pydantic request/response schemas"
```

---

### Task 2: Extend DeviceRepository

**Files:**
- Modify: `src/cctv_monitor/storage/repositories.py`
- Test: `tests/unit/storage/test_repositories.py`

**Step 1: Write the test**

Create `tests/unit/storage/__init__.py` (empty) if it doesn't exist.

```python
# tests/unit/storage/test_repositories.py
import pytest
from unittest.mock import AsyncMock, MagicMock, patch
from datetime import datetime, timezone

from cctv_monitor.storage.repositories import DeviceRepository


@pytest.fixture
def mock_session():
    session = AsyncMock()
    return session


@pytest.mark.asyncio
async def test_list_all(mock_session):
    mock_result = MagicMock()
    mock_result.scalars.return_value.all.return_value = ["device1", "device2"]
    mock_session.execute.return_value = mock_result

    repo = DeviceRepository(mock_session)
    result = await repo.list_all()
    assert len(result) == 2
    mock_session.execute.assert_called_once()


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
```

**Step 2: Run test to verify it fails**

Run: `python -m pytest tests/unit/storage/test_repositories.py -v`
Expected: FAIL — `list_all`, `create`, `delete` not found

**Step 3: Add methods to DeviceRepository**

Add these methods to the existing `DeviceRepository` class in `src/cctv_monitor/storage/repositories.py`:

```python
# Add to DeviceRepository class (after get_by_id method, around line 24)

    async def list_all(self) -> list[DeviceTable]:
        result = await self._session.execute(select(DeviceTable))
        return list(result.scalars().all())

    async def create(self, device: DeviceTable) -> None:
        self._session.add(device)
        await self._session.flush()

    async def delete(self, device_id: str) -> bool:
        device = await self.get_by_id(device_id)
        if device is None:
            return False
        await self._session.delete(device)
        await self._session.flush()
        return True
```

**Step 4: Run test to verify it passes**

Run: `python -m pytest tests/unit/storage/test_repositories.py -v`
Expected: PASS (4 tests)

**Step 5: Commit**

```bash
git add src/cctv_monitor/storage/repositories.py tests/unit/storage/test_repositories.py
git commit -m "feat(storage): add list_all, create, delete to DeviceRepository"
```

---

### Task 3: API Dependencies

**Files:**
- Create: `src/cctv_monitor/api/deps.py`

**Step 1: Write implementation**

This is infrastructure glue code — no unit test needed, it will be tested via route tests.

```python
# src/cctv_monitor/api/deps.py
from collections.abc import AsyncGenerator

from fastapi import Depends, Request
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.core.config import Settings
from cctv_monitor.drivers.registry import DriverRegistry
from cctv_monitor.core.http_client import HttpClientManager
from cctv_monitor.storage.repositories import DeviceRepository


async def get_session(request: Request) -> AsyncGenerator[AsyncSession, None]:
    session_factory = request.app.state.session_factory
    async with session_factory() as session:
        yield session


def get_settings(request: Request) -> Settings:
    return request.app.state.settings


def get_driver_registry(request: Request) -> DriverRegistry:
    return request.app.state.driver_registry


def get_http_client(request: Request) -> HttpClientManager:
    return request.app.state.http_client


def get_device_repo(session: AsyncSession = Depends(get_session)) -> DeviceRepository:
    return DeviceRepository(session)
```

**Step 2: Commit**

```bash
git add src/cctv_monitor/api/deps.py
git commit -m "feat(api): add FastAPI dependency injection helpers"
```

---

### Task 4: Refactor app.py with Lifespan

**Files:**
- Modify: `src/cctv_monitor/api/app.py`
- Modify: `src/cctv_monitor/main.py`
- Test: `tests/unit/api/test_app.py`

**Step 1: Write the test**

```python
# tests/unit/api/test_app.py
import pytest
from unittest.mock import patch, AsyncMock, MagicMock
from fastapi.testclient import TestClient


def test_health_endpoint():
    """Health endpoint works without database."""
    from cctv_monitor.api.app import create_app
    app = create_app()
    client = TestClient(app)
    response = client.get("/health")
    assert response.status_code == 200
    assert response.json()["status"] == "ok"
```

**Step 2: Run test to verify it passes (existing behavior)**

Run: `python -m pytest tests/unit/api/test_app.py -v`
Expected: PASS

**Step 3: Rewrite app.py with CORS and router inclusion**

```python
# src/cctv_monitor/api/app.py
from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware


def create_app() -> FastAPI:
    app = FastAPI(title="CCTV Monitor", version="0.1.0")

    app.add_middleware(
        CORSMiddleware,
        allow_origins=["http://localhost:5173"],
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    from cctv_monitor.api.routes.devices import router as devices_router
    from cctv_monitor.api.routes.status import router as status_router

    app.include_router(devices_router, prefix="/api")
    app.include_router(status_router, prefix="/api")

    @app.get("/health")
    async def health():
        return {"status": "ok", "service": "cctv-monitor"}

    return app
```

**Step 4: Simplify main.py to store state on app**

```python
# src/cctv_monitor/main.py
"""Application entry point."""

import asyncio

import structlog
import uvicorn

from cctv_monitor.api.app import create_app
from cctv_monitor.core.config import Settings
from cctv_monitor.core.http_client import HttpClientManager
from cctv_monitor.core.types import DeviceVendor
from cctv_monitor.drivers.hikvision.driver import HikvisionDriver
from cctv_monitor.drivers.registry import DriverRegistry
from cctv_monitor.metrics.collector import MetricsCollector
from cctv_monitor.polling.scheduler import create_scheduler
from cctv_monitor.storage.database import create_engine, create_session_factory

logger = structlog.get_logger()


async def main() -> None:
    structlog.configure(
        wrapper_class=structlog.make_filtering_bound_logger(20),
    )

    logger.info("cctv_monitor.starting")

    settings = Settings()  # type: ignore[call-arg]
    http_client = HttpClientManager()
    metrics = MetricsCollector()

    # Database
    engine = create_engine(settings.database_url)
    session_factory = create_session_factory(engine)

    # Driver registry
    registry = DriverRegistry()
    registry.register(DeviceVendor.HIKVISION, HikvisionDriver)

    # Scheduler
    scheduler = create_scheduler()
    scheduler.start()

    # API
    app = create_app()

    # Store shared state on app for dependency injection
    app.state.settings = settings
    app.state.session_factory = session_factory
    app.state.driver_registry = registry
    app.state.http_client = http_client
    app.state.metrics = metrics
    app.state.scheduler = scheduler

    logger.info("cctv_monitor.started")

    config = uvicorn.Config(app, host="0.0.0.0", port=8000, log_level="info")
    server = uvicorn.Server(config)
    try:
        await server.serve()
    finally:
        scheduler.shutdown()
        await http_client.close()
        await engine.dispose()
        logger.info("cctv_monitor.stopped")


if __name__ == "__main__":
    asyncio.run(main())
```

**Step 5: Run test to verify it still passes**

Run: `python -m pytest tests/unit/api/test_app.py -v`
Expected: PASS

Note: This test will fail if the route modules don't exist yet. Create empty route stubs first:

```python
# src/cctv_monitor/api/routes/__init__.py
# (empty)
```

```python
# src/cctv_monitor/api/routes/devices.py
from fastapi import APIRouter

router = APIRouter()
```

```python
# src/cctv_monitor/api/routes/status.py
from fastapi import APIRouter

router = APIRouter()
```

**Step 6: Commit**

```bash
git add src/cctv_monitor/api/ src/cctv_monitor/main.py tests/unit/api/
git commit -m "feat(api): refactor app with CORS, routers, and state injection"
```

---

### Task 5: Device Routes — List and Create

**Files:**
- Modify: `src/cctv_monitor/api/routes/devices.py`
- Test: `tests/unit/api/test_device_routes.py`

**Step 1: Write the test**

```python
# tests/unit/api/test_device_routes.py
import pytest
from unittest.mock import AsyncMock, MagicMock, patch
from fastapi.testclient import TestClient

from cctv_monitor.api.app import create_app
from cctv_monitor.api.deps import get_session, get_settings, get_driver_registry, get_http_client


@pytest.fixture
def mock_session():
    session = AsyncMock()
    return session


@pytest.fixture
def mock_settings():
    settings = MagicMock()
    settings.ENCRYPTION_KEY = "dGVzdGtleXRlc3RrZXl0ZXN0a2V5dGVzdGtleTE2Mzg="  # valid Fernet key
    return settings


@pytest.fixture
def mock_registry():
    return MagicMock()


@pytest.fixture
def mock_http_client():
    return MagicMock()


@pytest.fixture
def client(mock_session, mock_settings, mock_registry, mock_http_client):
    app = create_app()

    async def override_session():
        yield mock_session

    app.dependency_overrides[get_session] = override_session
    app.dependency_overrides[get_settings] = lambda: mock_settings
    app.dependency_overrides[get_driver_registry] = lambda: mock_registry
    app.dependency_overrides[get_http_client] = lambda: mock_http_client

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
    device.port = 80
    device.is_active = True

    mock_result = MagicMock()
    mock_result.scalars.return_value.all.return_value = [device]
    mock_session.execute.return_value = mock_result

    response = client.get("/api/devices")
    assert response.status_code == 200
    data = response.json()
    assert len(data) == 1
    assert data[0]["device_id"] == "nvr-01"


def test_create_device(client, mock_session, mock_settings):
    mock_session.flush = AsyncMock()

    response = client.post("/api/devices", json={
        "device_id": "nvr-02",
        "name": "New NVR",
        "vendor": "hikvision",
        "host": "10.0.0.1",
        "port": 8443,
        "username": "admin",
        "password": "secret123",
    })
    assert response.status_code == 201
    data = response.json()
    assert data["device_id"] == "nvr-02"
    assert data["is_active"] is True


def test_create_device_duplicate(client, mock_session, mock_settings):
    from sqlalchemy.exc import IntegrityError
    mock_session.flush = AsyncMock(side_effect=IntegrityError("dup", {}, None))

    response = client.post("/api/devices", json={
        "device_id": "nvr-01",
        "name": "Dup",
        "vendor": "hikvision",
        "host": "10.0.0.1",
        "port": 80,
        "username": "admin",
        "password": "pass",
    })
    assert response.status_code == 409
```

**Step 2: Run test to verify it fails**

Run: `python -m pytest tests/unit/api/test_device_routes.py -v`
Expected: FAIL — routes don't have endpoints yet

**Step 3: Implement device routes (list + create)**

```python
# src/cctv_monitor/api/routes/devices.py
from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.exc import IntegrityError
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.api.deps import get_session, get_settings, get_driver_registry, get_http_client
from cctv_monitor.api.schemas import DeviceCreate, DeviceOut, DeviceDetailOut, PollResultOut, HealthSummaryOut, CameraChannelOut, DiskOut, AlertOut
from cctv_monitor.core.config import Settings
from cctv_monitor.core.crypto import encrypt_value
from cctv_monitor.core.types import DeviceTransport, DeviceVendor
from cctv_monitor.drivers.hikvision.transports.isapi import IsapiTransport
from cctv_monitor.drivers.registry import DriverRegistry
from cctv_monitor.core.http_client import HttpClientManager
from cctv_monitor.models.device import DeviceConfig
from cctv_monitor.storage.repositories import DeviceRepository, AlertRepository
from cctv_monitor.storage.tables import DeviceTable

router = APIRouter(tags=["devices"])


@router.get("/devices", response_model=list[DeviceOut])
async def list_devices(session: AsyncSession = Depends(get_session)):
    repo = DeviceRepository(session)
    devices = await repo.list_all()
    return [
        DeviceOut(
            device_id=d.device_id,
            name=d.name,
            vendor=d.vendor,
            host=d.host,
            port=d.port,
            is_active=d.is_active,
            last_health=None,
        )
        for d in devices
    ]


@router.post("/devices", response_model=DeviceOut, status_code=201)
async def create_device(
    body: DeviceCreate,
    session: AsyncSession = Depends(get_session),
    settings: Settings = Depends(get_settings),
):
    encrypted_password = encrypt_value(body.password, settings.ENCRYPTION_KEY)
    device = DeviceTable(
        device_id=body.device_id,
        name=body.name,
        vendor=body.vendor,
        host=body.host,
        port=body.port,
        username=body.username,
        password_encrypted=encrypted_password,
        transport_mode="isapi",
        is_active=True,
    )
    repo = DeviceRepository(session)
    try:
        await repo.create(device)
        await session.commit()
    except IntegrityError:
        await session.rollback()
        raise HTTPException(status_code=409, detail=f"Device '{body.device_id}' already exists")
    return DeviceOut(
        device_id=device.device_id,
        name=device.name,
        vendor=device.vendor,
        host=device.host,
        port=device.port,
        is_active=device.is_active,
        last_health=None,
    )


@router.delete("/devices/{device_id}", status_code=204)
async def delete_device(
    device_id: str,
    session: AsyncSession = Depends(get_session),
):
    repo = DeviceRepository(session)
    deleted = await repo.delete(device_id)
    if not deleted:
        raise HTTPException(status_code=404, detail="Device not found")
    await session.commit()


@router.get("/devices/{device_id}", response_model=DeviceDetailOut)
async def get_device_detail(
    device_id: str,
    session: AsyncSession = Depends(get_session),
):
    repo = DeviceRepository(session)
    device = await repo.get_by_id(device_id)
    if device is None:
        raise HTTPException(status_code=404, detail="Device not found")

    alert_repo = AlertRepository(session)
    alerts = await alert_repo.get_active_alerts(device_id)

    return DeviceDetailOut(
        device=DeviceOut(
            device_id=device.device_id,
            name=device.name,
            vendor=device.vendor,
            host=device.host,
            port=device.port,
            is_active=device.is_active,
            last_health=None,
        ),
        cameras=[],
        disks=[],
        alerts=[
            AlertOut(
                id=a.id,
                alert_type=a.alert_type,
                severity=a.severity,
                message=a.message,
                status=a.status,
                created_at=a.created_at,
                resolved_at=a.resolved_at,
            )
            for a in alerts
        ],
    )


@router.post("/devices/{device_id}/poll", response_model=PollResultOut)
async def poll_device(
    device_id: str,
    session: AsyncSession = Depends(get_session),
    settings: Settings = Depends(get_settings),
    registry: DriverRegistry = Depends(get_driver_registry),
    http_client: HttpClientManager = Depends(get_http_client),
):
    repo = DeviceRepository(session)
    device = await repo.get_by_id(device_id)
    if device is None:
        raise HTTPException(status_code=404, detail="Device not found")

    from cctv_monitor.core.crypto import decrypt_value

    vendor = DeviceVendor(device.vendor)
    driver_cls = registry.get(vendor)
    transport = IsapiTransport(http_client)
    driver = driver_cls(transport)

    password = decrypt_value(device.password_encrypted, settings.ENCRYPTION_KEY)
    config = DeviceConfig(
        device_id=device.device_id,
        name=device.name,
        vendor=vendor,
        host=device.host,
        port=device.port,
        username=device.username,
        password=password,
        transport_mode=DeviceTransport(device.transport_mode),
        polling_policy_id=device.polling_policy_id,
        is_active=device.is_active,
    )

    try:
        await driver.connect(config)
        health = await driver.check_health()
    finally:
        try:
            await driver.disconnect()
        except Exception:
            pass

    return PollResultOut(
        device_id=device_id,
        health=HealthSummaryOut(
            reachable=health.reachable,
            camera_count=health.camera_count,
            online_cameras=health.online_cameras,
            offline_cameras=health.offline_cameras,
            disk_ok=health.disk_ok,
            response_time_ms=health.response_time_ms,
            checked_at=health.checked_at,
        ),
    )
```

**Step 4: Run test to verify it passes**

Run: `python -m pytest tests/unit/api/test_device_routes.py -v`
Expected: PASS (4 tests)

**Step 5: Commit**

```bash
git add src/cctv_monitor/api/routes/devices.py tests/unit/api/test_device_routes.py
git commit -m "feat(api): add device list, create, delete, detail, poll endpoints"
```

---

### Task 6: Status Overview Route

**Files:**
- Modify: `src/cctv_monitor/api/routes/status.py`
- Test: `tests/unit/api/test_status_routes.py`

**Step 1: Write the test**

```python
# tests/unit/api/test_status_routes.py
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
```

**Step 2: Run test to verify it fails**

Run: `python -m pytest tests/unit/api/test_status_routes.py -v`
Expected: FAIL — no `/api/overview` endpoint

**Step 3: Implement overview route**

```python
# src/cctv_monitor/api/routes/status.py
from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.api.deps import get_session
from cctv_monitor.api.schemas import OverviewOut
from cctv_monitor.storage.repositories import DeviceRepository

router = APIRouter(tags=["status"])


@router.get("/overview", response_model=OverviewOut)
async def get_overview(session: AsyncSession = Depends(get_session)):
    repo = DeviceRepository(session)
    devices = await repo.list_all()
    return OverviewOut(
        total_devices=len(devices),
        reachable_devices=0,
        total_cameras=0,
        online_cameras=0,
        disks_ok=True,
    )
```

**Step 4: Run test to verify it passes**

Run: `python -m pytest tests/unit/api/test_status_routes.py -v`
Expected: PASS

**Step 5: Commit**

```bash
git add src/cctv_monitor/api/routes/status.py tests/unit/api/test_status_routes.py
git commit -m "feat(api): add overview endpoint"
```

---

### Task 7: Frontend Scaffold (Vite + React + TypeScript)

**Files:**
- Create: `frontend/` directory with Vite project

**Step 1: Initialize Vite project**

```bash
cd "c:/Users/Pavel/DEV/CCTV Monitoring Tool"
npm create vite@latest frontend -- --template react-ts
cd frontend
npm install
npm install react-router-dom
```

**Step 2: Configure Vite proxy**

```typescript
// frontend/vite.config.ts
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:8000',
        changeOrigin: true,
      },
    },
  },
})
```

**Step 3: Clean up default files**

Remove default Vite content from `src/App.tsx`, `src/App.css`, `src/index.css`.

**Step 4: Verify it builds**

```bash
cd frontend
npm run build
```

Expected: Build succeeds.

**Step 5: Commit**

```bash
cd ..
git add frontend/
git commit -m "feat(frontend): scaffold Vite + React + TypeScript project"
```

---

### Task 8: Frontend TypeScript Types and API Client

**Files:**
- Create: `frontend/src/types.ts`
- Create: `frontend/src/api/client.ts`

**Step 1: Create TypeScript interfaces**

```typescript
// frontend/src/types.ts
export interface HealthSummary {
  reachable: boolean;
  camera_count: number;
  online_cameras: number;
  offline_cameras: number;
  disk_ok: boolean;
  response_time_ms: number;
  checked_at: string;
}

export interface Device {
  device_id: string;
  name: string;
  vendor: string;
  host: string;
  port: number;
  is_active: boolean;
  last_health: HealthSummary | null;
}

export interface CameraChannel {
  channel_id: string;
  channel_name: string;
  status: string;
  ip_address: string | null;
  checked_at: string;
}

export interface Disk {
  disk_id: string;
  status: string;
  capacity_bytes: number;
  free_bytes: number;
  health_status: string;
  checked_at: string;
}

export interface Alert {
  id: number;
  alert_type: string;
  severity: string;
  message: string;
  status: string;
  created_at: string;
  resolved_at: string | null;
}

export interface DeviceDetail {
  device: Device;
  cameras: CameraChannel[];
  disks: Disk[];
  alerts: Alert[];
}

export interface DeviceCreate {
  device_id: string;
  name: string;
  vendor: string;
  host: string;
  port: number;
  username: string;
  password: string;
}

export interface PollResult {
  device_id: string;
  health: HealthSummary;
}

export interface Overview {
  total_devices: number;
  reachable_devices: number;
  total_cameras: number;
  online_cameras: number;
  disks_ok: boolean;
}
```

**Step 2: Create API client**

```typescript
// frontend/src/api/client.ts
import type { Device, DeviceCreate, DeviceDetail, PollResult, Overview } from '../types';

const BASE = '/api';

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const response = await fetch(`${BASE}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });
  if (!response.ok) {
    const body = await response.json().catch(() => ({}));
    throw new Error(body.detail || `HTTP ${response.status}`);
  }
  if (response.status === 204) return undefined as T;
  return response.json();
}

export const api = {
  getDevices: () => request<Device[]>('/devices'),

  createDevice: (data: DeviceCreate) =>
    request<Device>('/devices', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  deleteDevice: (deviceId: string) =>
    request<void>(`/devices/${deviceId}`, { method: 'DELETE' }),

  getDeviceDetail: (deviceId: string) =>
    request<DeviceDetail>(`/devices/${deviceId}`),

  pollDevice: (deviceId: string) =>
    request<PollResult>(`/devices/${deviceId}/poll`, { method: 'POST' }),

  getOverview: () => request<Overview>('/overview'),
};
```

**Step 3: Commit**

```bash
git add frontend/src/types.ts frontend/src/api/client.ts
git commit -m "feat(frontend): add TypeScript types and API client"
```

---

### Task 9: Layout and Routing

**Files:**
- Create: `frontend/src/components/Layout.tsx`
- Create: `frontend/src/components/StatusBadge.tsx`
- Modify: `frontend/src/App.tsx`
- Modify: `frontend/src/main.tsx`
- Create: `frontend/src/App.css`

**Step 1: Create Layout component**

```tsx
// frontend/src/components/Layout.tsx
import { Link, Outlet } from 'react-router-dom';

export default function Layout() {
  return (
    <div className="app">
      <header className="header">
        <h1><Link to="/">CCTV Monitor</Link></h1>
        <nav>
          <Link to="/">Devices</Link>
          <Link to="/devices/add">Add Device</Link>
        </nav>
      </header>
      <main className="main">
        <Outlet />
      </main>
    </div>
  );
}
```

**Step 2: Create StatusBadge component**

```tsx
// frontend/src/components/StatusBadge.tsx
interface Props {
  status: string;
}

export default function StatusBadge({ status }: Props) {
  const className = `badge badge-${status.toLowerCase()}`;
  return <span className={className}>{status}</span>;
}
```

**Step 3: Create App with routing**

```tsx
// frontend/src/App.tsx
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Layout from './components/Layout';
import DeviceList from './pages/DeviceList';
import AddDevice from './pages/AddDevice';
import DeviceDetail from './pages/DeviceDetail';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route path="/" element={<DeviceList />} />
          <Route path="/devices/add" element={<AddDevice />} />
          <Route path="/devices/:deviceId" element={<DeviceDetail />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
```

**Step 4: Update main.tsx**

```tsx
// frontend/src/main.tsx
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import App from './App';
import './App.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
```

**Step 5: Create base CSS**

```css
/* frontend/src/App.css */
* {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

body {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  background: #f5f5f5;
  color: #333;
}

.app {
  min-height: 100vh;
}

.header {
  background: #1a1a2e;
  color: white;
  padding: 1rem 2rem;
  display: flex;
  align-items: center;
  gap: 2rem;
}

.header h1 {
  font-size: 1.25rem;
}

.header a {
  color: #ccc;
  text-decoration: none;
}

.header a:hover {
  color: white;
}

.header nav {
  display: flex;
  gap: 1.5rem;
}

.main {
  max-width: 1200px;
  margin: 2rem auto;
  padding: 0 1rem;
}

table {
  width: 100%;
  border-collapse: collapse;
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
}

th, td {
  padding: 0.75rem 1rem;
  text-align: left;
  border-bottom: 1px solid #eee;
}

th {
  background: #f8f9fa;
  font-weight: 600;
  font-size: 0.875rem;
  text-transform: uppercase;
  color: #666;
}

.badge {
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
}

.badge-online { background: #d4edda; color: #155724; }
.badge-offline { background: #f8d7da; color: #721c24; }
.badge-unknown { background: #e2e3e5; color: #383d41; }
.badge-ok { background: #d4edda; color: #155724; }
.badge-warning { background: #fff3cd; color: #856404; }
.badge-error { background: #f8d7da; color: #721c24; }
.badge-true { background: #d4edda; color: #155724; }
.badge-false { background: #f8d7da; color: #721c24; }

button {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.875rem;
}

.btn-primary {
  background: #0d6efd;
  color: white;
}

.btn-primary:hover {
  background: #0b5ed7;
}

.btn-danger {
  background: #dc3545;
  color: white;
}

.btn-danger:hover {
  background: #bb2d3b;
}

.btn-secondary {
  background: #6c757d;
  color: white;
}

.btn-secondary:hover {
  background: #5c636a;
}

button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.25rem;
  font-weight: 500;
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
}

.card {
  background: white;
  border-radius: 8px;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
  margin-bottom: 1.5rem;
}

.card h2 {
  margin-bottom: 1rem;
  font-size: 1.1rem;
}

.actions {
  display: flex;
  gap: 0.5rem;
}

.error {
  color: #dc3545;
  padding: 1rem;
  background: #f8d7da;
  border-radius: 4px;
  margin-bottom: 1rem;
}

.loading {
  text-align: center;
  padding: 2rem;
  color: #666;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}
```

**Step 6: Create placeholder pages** (needed for build to pass)

```tsx
// frontend/src/pages/DeviceList.tsx
export default function DeviceList() {
  return <div>Device list placeholder</div>;
}
```

```tsx
// frontend/src/pages/AddDevice.tsx
export default function AddDevice() {
  return <div>Add device placeholder</div>;
}
```

```tsx
// frontend/src/pages/DeviceDetail.tsx
export default function DeviceDetail() {
  return <div>Device detail placeholder</div>;
}
```

**Step 7: Verify it builds**

```bash
cd frontend && npm run build
```

Expected: Build succeeds.

**Step 8: Commit**

```bash
cd ..
git add frontend/src/
git commit -m "feat(frontend): add layout, routing, status badge, base CSS"
```

---

### Task 10: DeviceList Page

**Files:**
- Modify: `frontend/src/pages/DeviceList.tsx`

**Step 1: Implement DeviceList**

```tsx
// frontend/src/pages/DeviceList.tsx
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../api/client';
import StatusBadge from '../components/StatusBadge';
import type { Device } from '../types';

export default function DeviceList() {
  const [devices, setDevices] = useState<Device[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [polling, setPolling] = useState<string | null>(null);

  const fetchDevices = async () => {
    try {
      const data = await api.getDevices();
      setDevices(data);
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load devices');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDevices();
    const interval = setInterval(fetchDevices, 30000);
    return () => clearInterval(interval);
  }, []);

  const handlePoll = async (deviceId: string) => {
    setPolling(deviceId);
    try {
      const result = await api.pollDevice(deviceId);
      setDevices(prev =>
        prev.map(d =>
          d.device_id === deviceId
            ? { ...d, last_health: result.health }
            : d
        )
      );
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Poll failed');
    } finally {
      setPolling(null);
    }
  };

  const handleDelete = async (deviceId: string) => {
    if (!confirm(`Delete device ${deviceId}?`)) return;
    try {
      await api.deleteDevice(deviceId);
      setDevices(prev => prev.filter(d => d.device_id !== deviceId));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Delete failed');
    }
  };

  if (loading) return <div className="loading">Loading devices...</div>;

  return (
    <div>
      <div className="page-header">
        <h2>Devices ({devices.length})</h2>
        <Link to="/devices/add">
          <button className="btn-primary">+ Add Device</button>
        </Link>
      </div>

      {error && <div className="error">{error}</div>}

      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Host</th>
            <th>Vendor</th>
            <th>Reachable</th>
            <th>Cameras</th>
            <th>Disks</th>
            <th>Response</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {devices.map(d => (
            <tr key={d.device_id}>
              <td>
                <Link to={`/devices/${d.device_id}`}>{d.name}</Link>
              </td>
              <td>{d.host}:{d.port}</td>
              <td>{d.vendor}</td>
              <td>
                {d.last_health ? (
                  <StatusBadge status={d.last_health.reachable ? 'online' : 'offline'} />
                ) : (
                  <StatusBadge status="unknown" />
                )}
              </td>
              <td>
                {d.last_health
                  ? `${d.last_health.online_cameras}/${d.last_health.camera_count}`
                  : '—'}
              </td>
              <td>
                {d.last_health ? (
                  <StatusBadge status={d.last_health.disk_ok ? 'ok' : 'error'} />
                ) : '—'}
              </td>
              <td>
                {d.last_health
                  ? `${Math.round(d.last_health.response_time_ms)}ms`
                  : '—'}
              </td>
              <td>
                <div className="actions">
                  <button
                    className="btn-primary"
                    onClick={() => handlePoll(d.device_id)}
                    disabled={polling === d.device_id}
                  >
                    {polling === d.device_id ? 'Polling...' : 'Poll'}
                  </button>
                  <button
                    className="btn-danger"
                    onClick={() => handleDelete(d.device_id)}
                  >
                    Delete
                  </button>
                </div>
              </td>
            </tr>
          ))}
          {devices.length === 0 && (
            <tr>
              <td colSpan={8} style={{ textAlign: 'center', padding: '2rem' }}>
                No devices yet. <Link to="/devices/add">Add one</Link>
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
```

**Step 2: Verify it builds**

```bash
cd frontend && npm run build
```

**Step 3: Commit**

```bash
cd ..
git add frontend/src/pages/DeviceList.tsx
git commit -m "feat(frontend): implement DeviceList page with poll and delete"
```

---

### Task 11: AddDevice Page

**Files:**
- Modify: `frontend/src/pages/AddDevice.tsx`

**Step 1: Implement AddDevice form**

```tsx
// frontend/src/pages/AddDevice.tsx
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../api/client';
import type { DeviceCreate } from '../types';

export default function AddDevice() {
  const navigate = useNavigate();
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState<DeviceCreate>({
    device_id: '',
    name: '',
    vendor: 'hikvision',
    host: '',
    port: 80,
    username: 'admin',
    password: '',
  });

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setForm(prev => ({
      ...prev,
      [name]: name === 'port' ? parseInt(value, 10) || 0 : value,
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError('');
    try {
      await api.createDevice(form);
      navigate('/');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to add device');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div>
      <h2>Add Device</h2>
      {error && <div className="error">{error}</div>}
      <div className="card" style={{ maxWidth: 500 }}>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Device ID</label>
            <input name="device_id" value={form.device_id} onChange={handleChange}
              placeholder="nvr-building-1" required />
          </div>
          <div className="form-group">
            <label>Name</label>
            <input name="name" value={form.name} onChange={handleChange}
              placeholder="Building 1 NVR" required />
          </div>
          <div className="form-group">
            <label>Vendor</label>
            <select name="vendor" value={form.vendor} onChange={handleChange}>
              <option value="hikvision">Hikvision</option>
              <option value="dahua">Dahua</option>
              <option value="provision">Provision</option>
            </select>
          </div>
          <div className="form-group">
            <label>Host</label>
            <input name="host" value={form.host} onChange={handleChange}
              placeholder="192.168.1.100" required />
          </div>
          <div className="form-group">
            <label>Port</label>
            <input name="port" type="number" value={form.port} onChange={handleChange} required />
          </div>
          <div className="form-group">
            <label>Username</label>
            <input name="username" value={form.username} onChange={handleChange} required />
          </div>
          <div className="form-group">
            <label>Password</label>
            <input name="password" type="password" value={form.password} onChange={handleChange} required />
          </div>
          <button type="submit" className="btn-primary" disabled={submitting}>
            {submitting ? 'Adding...' : 'Add Device'}
          </button>
        </form>
      </div>
    </div>
  );
}
```

**Step 2: Verify it builds**

```bash
cd frontend && npm run build
```

**Step 3: Commit**

```bash
cd ..
git add frontend/src/pages/AddDevice.tsx
git commit -m "feat(frontend): implement AddDevice form page"
```

---

### Task 12: DeviceDetail Page

**Files:**
- Modify: `frontend/src/pages/DeviceDetail.tsx`

**Step 1: Implement DeviceDetail page**

```tsx
// frontend/src/pages/DeviceDetail.tsx
import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { api } from '../api/client';
import StatusBadge from '../components/StatusBadge';
import type { DeviceDetail as DeviceDetailType, PollResult } from '../types';

export default function DeviceDetail() {
  const { deviceId } = useParams<{ deviceId: string }>();
  const navigate = useNavigate();
  const [detail, setDetail] = useState<DeviceDetailType | null>(null);
  const [pollResult, setPollResult] = useState<PollResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [polling, setPolling] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!deviceId) return;
    api.getDeviceDetail(deviceId)
      .then(setDetail)
      .catch(err => setError(err instanceof Error ? err.message : 'Failed to load'))
      .finally(() => setLoading(false));
  }, [deviceId]);

  const handlePoll = async () => {
    if (!deviceId) return;
    setPolling(true);
    setError('');
    try {
      const result = await api.pollDevice(deviceId);
      setPollResult(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Poll failed');
    } finally {
      setPolling(false);
    }
  };

  const handleDelete = async () => {
    if (!deviceId || !confirm('Delete this device?')) return;
    try {
      await api.deleteDevice(deviceId);
      navigate('/');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Delete failed');
    }
  };

  const formatBytes = (bytes: number) => {
    if (bytes === 0) return '0 B';
    const gb = bytes / (1024 * 1024 * 1024);
    if (gb >= 1) return `${gb.toFixed(1)} GB`;
    const mb = bytes / (1024 * 1024);
    return `${mb.toFixed(0)} MB`;
  };

  if (loading) return <div className="loading">Loading...</div>;
  if (!detail) return <div className="error">Device not found</div>;

  const health = pollResult?.health || detail.device.last_health;

  return (
    <div>
      <div className="page-header">
        <h2>{detail.device.name} ({detail.device.device_id})</h2>
        <div className="actions">
          <button className="btn-primary" onClick={handlePoll} disabled={polling}>
            {polling ? 'Polling...' : 'Poll Now'}
          </button>
          <button className="btn-danger" onClick={handleDelete}>Delete</button>
        </div>
      </div>

      {error && <div className="error">{error}</div>}

      {/* Health Summary */}
      {health && (
        <div className="card">
          <h2>Health Summary</h2>
          <table>
            <tbody>
              <tr><td>Reachable</td><td><StatusBadge status={health.reachable ? 'online' : 'offline'} /></td></tr>
              <tr><td>Cameras</td><td>{health.online_cameras}/{health.camera_count} online</td></tr>
              <tr><td>Disks</td><td><StatusBadge status={health.disk_ok ? 'ok' : 'error'} /></td></tr>
              <tr><td>Response Time</td><td>{Math.round(health.response_time_ms)}ms</td></tr>
              <tr><td>Last Check</td><td>{new Date(health.checked_at).toLocaleString()}</td></tr>
            </tbody>
          </table>
        </div>
      )}

      {/* Cameras */}
      {detail.cameras.length > 0 && (
        <div className="card">
          <h2>Cameras ({detail.cameras.length})</h2>
          <table>
            <thead>
              <tr>
                <th>Channel</th>
                <th>Name</th>
                <th>IP</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {detail.cameras.map(c => (
                <tr key={c.channel_id}>
                  <td>{c.channel_id}</td>
                  <td>{c.channel_name || '—'}</td>
                  <td>{c.ip_address || '—'}</td>
                  <td><StatusBadge status={c.status} /></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Disks */}
      {detail.disks.length > 0 && (
        <div className="card">
          <h2>Disks ({detail.disks.length})</h2>
          <table>
            <thead>
              <tr>
                <th>Disk</th>
                <th>Capacity</th>
                <th>Free</th>
                <th>Health</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {detail.disks.map(d => (
                <tr key={d.disk_id}>
                  <td>{d.disk_id}</td>
                  <td>{formatBytes(d.capacity_bytes)}</td>
                  <td>{formatBytes(d.free_bytes)}</td>
                  <td>{d.health_status}</td>
                  <td><StatusBadge status={d.status} /></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Alerts */}
      {detail.alerts.length > 0 && (
        <div className="card">
          <h2>Active Alerts ({detail.alerts.length})</h2>
          <table>
            <thead>
              <tr>
                <th>Type</th>
                <th>Severity</th>
                <th>Message</th>
                <th>Since</th>
              </tr>
            </thead>
            <tbody>
              {detail.alerts.map(a => (
                <tr key={a.id}>
                  <td>{a.alert_type}</td>
                  <td><StatusBadge status={a.severity} /></td>
                  <td>{a.message}</td>
                  <td>{new Date(a.created_at).toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Device Info */}
      <div className="card">
        <h2>Connection Info</h2>
        <table>
          <tbody>
            <tr><td>Host</td><td>{detail.device.host}:{detail.device.port}</td></tr>
            <tr><td>Vendor</td><td>{detail.device.vendor}</td></tr>
            <tr><td>Active</td><td>{detail.device.is_active ? 'Yes' : 'No'}</td></tr>
          </tbody>
        </table>
      </div>
    </div>
  );
}
```

**Step 2: Verify it builds**

```bash
cd frontend && npm run build
```

**Step 3: Commit**

```bash
cd ..
git add frontend/src/pages/DeviceDetail.tsx
git commit -m "feat(frontend): implement DeviceDetail page with health, cameras, disks"
```

---

### Task 13: Alembic Migration for Initial Tables

**Files:**
- Create migration via alembic

**Step 1: Initialize alembic (if not done)**

```bash
cd "c:/Users/Pavel/DEV/CCTV Monitoring Tool"
python -m alembic init alembic
```

If already initialized, skip this step.

**Step 2: Configure alembic**

Edit `alembic/env.py` to use async engine and import Base:

```python
# In alembic/env.py, add after imports:
from cctv_monitor.storage.tables import Base
target_metadata = Base.metadata

# Also update sqlalchemy.url to read from .env or settings
```

**Step 3: Create migration**

```bash
python -m alembic revision --autogenerate -m "initial tables"
```

**Step 4: Apply migration**

```bash
python -m alembic upgrade head
```

**Step 5: Commit**

```bash
git add alembic/ alembic.ini
git commit -m "feat(db): add alembic migrations for initial tables"
```

---

### Task 14: End-to-End Smoke Test

**Files:**
- No new files — manual verification

**Step 1: Run all existing tests**

```bash
python -m pytest tests/ -v
```

Expected: All tests pass (142+ existing + new API tests).

**Step 2: Start the backend**

```bash
python -m cctv_monitor.main
```

Expected: Server starts on port 8000, `/health` returns OK.

**Step 3: Start the frontend**

```bash
cd frontend && npm run dev
```

Expected: Vite dev server on port 5173, shows device list page.

**Step 4: Test the flow**

1. Open http://localhost:5173 — empty device list
2. Click "Add Device" — fill in form with a real NVR
3. Submit — redirected to device list, new device appears
4. Click "Poll" — health data appears (reachable, cameras, disks)
5. Click device name — detail page shows
6. Click "Delete" — device removed

**Step 5: Commit any fixes**

```bash
git add -A
git commit -m "fix: resolve smoke test issues"
```
