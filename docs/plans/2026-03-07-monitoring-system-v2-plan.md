# CCTV Monitoring System v2 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Transform basic polling tool into full monitoring system with rich device details, auto-polling, health history, tags, snapshots, and MUI frontend.

**Architecture:** Extend existing FastAPI backend with new DB tables (device_tags, device_health_log), new columns on devices (model, serial, firmware, last_health_json), background scheduler job, and new API endpoints. Rebuild frontend with MUI components (DataGrid, Charts, Tabs).

**Tech Stack:** Python 3.14, FastAPI, SQLAlchemy async, APScheduler, React 19, TypeScript, Vite, MUI 6, MUI X DataGrid, MUI X Charts

---

## Phase 1: Backend Database & Core

### Task 1: Add new columns to DeviceTable

**Files:**
- Modify: `src/cctv_monitor/storage/tables.py:23-38`
- Create: `migrations/versions/0002_v2_schema.py`
- Test: `tests/unit/storage/test_tables_v2.py`

**Step 1: Write the migration**

Create `migrations/versions/0002_v2_schema.py`:

```python
"""Add v2 columns to devices + new tables."""

from alembic import op
import sqlalchemy as sa

revision = "0002"
down_revision = "0001"

def upgrade() -> None:
    # New columns on devices
    op.add_column("devices", sa.Column("model", sa.String(255), nullable=True))
    op.add_column("devices", sa.Column("serial_number", sa.String(255), nullable=True))
    op.add_column("devices", sa.Column("firmware_version", sa.String(255), nullable=True))
    op.add_column("devices", sa.Column("last_poll_at", sa.DateTime(timezone=True), nullable=True))
    op.add_column("devices", sa.Column("last_health_json", sa.JSON(), nullable=True))

    # device_tags table
    op.create_table(
        "device_tags",
        sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
        sa.Column("device_id", sa.String(100), sa.ForeignKey("devices.device_id", ondelete="CASCADE"), nullable=False),
        sa.Column("tag", sa.String(100), nullable=False),
        sa.UniqueConstraint("device_id", "tag", name="uq_device_tag"),
    )
    op.create_index("ix_device_tags_tag", "device_tags", ["tag"])

    # device_health_log table
    op.create_table(
        "device_health_log",
        sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
        sa.Column("device_id", sa.String(100), sa.ForeignKey("devices.device_id", ondelete="CASCADE"), nullable=False),
        sa.Column("reachable", sa.Boolean(), nullable=False),
        sa.Column("camera_count", sa.Integer(), default=0),
        sa.Column("online_cameras", sa.Integer(), default=0),
        sa.Column("offline_cameras", sa.Integer(), default=0),
        sa.Column("disk_ok", sa.Boolean(), default=True),
        sa.Column("response_time_ms", sa.Float(), default=0),
        sa.Column("checked_at", sa.DateTime(timezone=True), nullable=False),
    )
    op.create_index("ix_health_log_device_time", "device_health_log", ["device_id", "checked_at"])

def downgrade() -> None:
    op.drop_table("device_health_log")
    op.drop_table("device_tags")
    op.drop_column("devices", "last_health_json")
    op.drop_column("devices", "last_poll_at")
    op.drop_column("devices", "firmware_version")
    op.drop_column("devices", "serial_number")
    op.drop_column("devices", "model")
```

**Step 2: Update tables.py**

Add to `DeviceTable` class (after line 36 `is_active`):

```python
    model = Column(String(255), nullable=True)
    serial_number = Column(String(255), nullable=True)
    firmware_version = Column(String(255), nullable=True)
    last_poll_at = Column(DateTime(timezone=True), nullable=True)
    last_health_json = Column(JSON, nullable=True)
```

Add new table classes after `AlertTable`:

```python
class DeviceTagTable(Base):
    __tablename__ = "device_tags"
    id = Column(Integer, primary_key=True, autoincrement=True)
    device_id = Column(String(100), ForeignKey("devices.device_id", ondelete="CASCADE"), nullable=False)
    tag = Column(String(100), nullable=False)
    __table_args__ = (UniqueConstraint("device_id", "tag", name="uq_device_tag"),)


class DeviceHealthLogTable(Base):
    __tablename__ = "device_health_log"
    id = Column(Integer, primary_key=True, autoincrement=True)
    device_id = Column(String(100), ForeignKey("devices.device_id", ondelete="CASCADE"), nullable=False)
    reachable = Column(Boolean, nullable=False)
    camera_count = Column(Integer, default=0)
    online_cameras = Column(Integer, default=0)
    offline_cameras = Column(Integer, default=0)
    disk_ok = Column(Boolean, default=True)
    response_time_ms = Column(Float, default=0)
    checked_at = Column(DateTime(timezone=True), nullable=False)
```

**Step 3: Write test for new tables**

```python
# tests/unit/storage/test_tables_v2.py
from cctv_monitor.storage.tables import DeviceTable, DeviceTagTable, DeviceHealthLogTable

def test_device_table_has_v2_columns():
    cols = {c.name for c in DeviceTable.__table__.columns}
    assert "model" in cols
    assert "serial_number" in cols
    assert "firmware_version" in cols
    assert "last_poll_at" in cols
    assert "last_health_json" in cols

def test_device_tag_table_exists():
    cols = {c.name for c in DeviceTagTable.__table__.columns}
    assert "device_id" in cols
    assert "tag" in cols

def test_device_health_log_table_exists():
    cols = {c.name for c in DeviceHealthLogTable.__table__.columns}
    assert "device_id" in cols
    assert "reachable" in cols
    assert "response_time_ms" in cols
    assert "checked_at" in cols
```

**Step 4: Run tests**

Run: `python -m pytest tests/unit/storage/test_tables_v2.py -v`
Expected: PASS

**Step 5: Run migration against dev DB**

Run: `cd "c:\Users\Pavel\DEV\CCTV Monitoring Tool" && python -c "from cctv_monitor.storage.database import create_engine; from cctv_monitor.storage.tables import Base; import asyncio; e = create_engine('sqlite+aiosqlite:///cctv_monitor.db'); asyncio.run(Base.metadata.create_all(e))"`

Note: If using alembic, run `alembic upgrade head` instead. If not, the create_all approach works for SQLite.

**Step 6: Commit**

```bash
git add -A && git commit -m "feat: add v2 schema — device_tags, device_health_log, new device columns"
```

---

### Task 2: New repositories (tags, health_log)

**Files:**
- Modify: `src/cctv_monitor/storage/repositories.py:100-106`
- Test: `tests/unit/storage/test_repositories_v2.py`

**Step 1: Write tests**

```python
# tests/unit/storage/test_repositories_v2.py
from unittest.mock import AsyncMock, MagicMock
import pytest
from cctv_monitor.storage.repositories import DeviceTagRepository, DeviceHealthLogRepository

@pytest.mark.asyncio
async def test_tag_repo_add_tag():
    session = AsyncMock()
    repo = DeviceTagRepository(session)
    await repo.add_tag("dev1", "haifa")
    session.add.assert_called_once()

@pytest.mark.asyncio
async def test_tag_repo_get_tags():
    session = AsyncMock()
    mock_result = MagicMock()
    mock_result.scalars.return_value.all.return_value = ["haifa", "client-a"]
    session.execute.return_value = mock_result
    repo = DeviceTagRepository(session)
    tags = await repo.get_tags("dev1")
    assert tags == ["haifa", "client-a"]

@pytest.mark.asyncio
async def test_health_log_repo_insert():
    session = AsyncMock()
    repo = DeviceHealthLogRepository(session)
    await repo.insert(device_id="dev1", reachable=True, camera_count=8,
                      online_cameras=6, offline_cameras=2, disk_ok=True,
                      response_time_ms=45.0)
    session.add.assert_called_once()
```

**Step 2: Run tests to see them fail**

Run: `python -m pytest tests/unit/storage/test_repositories_v2.py -v`
Expected: FAIL (ImportError)

**Step 3: Implement repositories**

Add to `src/cctv_monitor/storage/repositories.py`:

```python
from cctv_monitor.storage.tables import DeviceTagTable, DeviceHealthLogTable

class DeviceTagRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def add_tag(self, device_id: str, tag: str) -> None:
        self._session.add(DeviceTagTable(device_id=device_id, tag=tag))

    async def remove_tag(self, device_id: str, tag: str) -> bool:
        from sqlalchemy import delete
        result = await self._session.execute(
            delete(DeviceTagTable).where(
                DeviceTagTable.device_id == device_id,
                DeviceTagTable.tag == tag,
            )
        )
        return result.rowcount > 0

    async def get_tags(self, device_id: str) -> list[str]:
        from sqlalchemy import select
        result = await self._session.execute(
            select(DeviceTagTable.tag).where(DeviceTagTable.device_id == device_id)
        )
        return list(result.scalars().all())

    async def get_all_unique_tags(self) -> list[str]:
        from sqlalchemy import select
        result = await self._session.execute(
            select(DeviceTagTable.tag).distinct()
        )
        return list(result.scalars().all())


class DeviceHealthLogRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def insert(self, *, device_id: str, reachable: bool,
                     camera_count: int, online_cameras: int,
                     offline_cameras: int, disk_ok: bool,
                     response_time_ms: float) -> None:
        from datetime import datetime, timezone
        self._session.add(DeviceHealthLogTable(
            device_id=device_id, reachable=reachable,
            camera_count=camera_count, online_cameras=online_cameras,
            offline_cameras=offline_cameras, disk_ok=disk_ok,
            response_time_ms=response_time_ms,
            checked_at=datetime.now(timezone.utc),
        ))

    async def get_history(self, device_id: str, hours: int = 24) -> list:
        from datetime import datetime, timezone, timedelta
        from sqlalchemy import select
        cutoff = datetime.now(timezone.utc) - timedelta(hours=hours)
        result = await self._session.execute(
            select(DeviceHealthLogTable)
            .where(DeviceHealthLogTable.device_id == device_id,
                   DeviceHealthLogTable.checked_at >= cutoff)
            .order_by(DeviceHealthLogTable.checked_at)
        )
        return list(result.scalars().all())

    async def cleanup_old(self, days: int = 30) -> int:
        from datetime import datetime, timezone, timedelta
        from sqlalchemy import delete
        cutoff = datetime.now(timezone.utc) - timedelta(days=days)
        result = await self._session.execute(
            delete(DeviceHealthLogTable).where(DeviceHealthLogTable.checked_at < cutoff)
        )
        return result.rowcount
```

**Step 4: Run tests**

Run: `python -m pytest tests/unit/storage/test_repositories_v2.py -v`
Expected: PASS

**Step 5: Commit**

```bash
git add -A && git commit -m "feat: add DeviceTagRepository and DeviceHealthLogRepository"
```

---

### Task 3: Update API schemas

**Files:**
- Modify: `src/cctv_monitor/api/schemas.py`
- Test: `tests/unit/api/test_schemas_v2.py`

**Step 1: Write tests**

```python
# tests/unit/api/test_schemas_v2.py
from cctv_monitor.api.schemas import DeviceOut, DeviceDetailOut, HealthLogEntryOut, TagOut

def test_device_out_has_v2_fields():
    d = DeviceOut(
        device_id="d1", name="Test", vendor="hikvision",
        host="1.2.3.4", web_port=80, sdk_port=8000,
        transport_mode="sdk", is_active=True, last_health=None,
        model="DS-7608NI", serial_number="SN123",
        firmware_version="4.1.0", last_poll_at=None, tags=["haifa"],
    )
    assert d.model == "DS-7608NI"
    assert d.tags == ["haifa"]

def test_health_log_entry_out():
    from datetime import datetime, timezone
    e = HealthLogEntryOut(
        reachable=True, camera_count=8, online_cameras=6,
        offline_cameras=2, disk_ok=True, response_time_ms=45.0,
        checked_at=datetime.now(timezone.utc),
    )
    assert e.online_cameras == 6
```

**Step 2: Run tests (fail)**

Run: `python -m pytest tests/unit/api/test_schemas_v2.py -v`

**Step 3: Update schemas.py**

Add new fields to `DeviceOut`:
```python
    model: str | None = None
    serial_number: str | None = None
    firmware_version: str | None = None
    last_poll_at: datetime | None = None
    tags: list[str] = []
```

Add new schemas:
```python
class HealthLogEntryOut(BaseModel):
    reachable: bool
    camera_count: int
    online_cameras: int
    offline_cameras: int
    disk_ok: bool
    response_time_ms: float
    checked_at: datetime

class TagOut(BaseModel):
    tag: str

class TagCreate(BaseModel):
    tag: str
```

Update `DeviceDetailOut` to include `last_health_json` data:
```python
class DeviceDetailOut(BaseModel):
    device: DeviceOut
    cameras: list[CameraChannelOut]
    disks: list[DiskOut]
    alerts: list[AlertOut]
    health: HealthSummaryOut | None = None
```

**Step 4: Run tests**

Run: `python -m pytest tests/unit/api/test_schemas_v2.py -v && python -m pytest tests/unit/api/ -v`
Expected: ALL PASS

**Step 5: Commit**

```bash
git add -A && git commit -m "feat: update API schemas with v2 fields"
```

---

### Task 4: New API routes (tags, history, alerts, snapshots)

**Files:**
- Create: `src/cctv_monitor/api/routes/tags.py`
- Create: `src/cctv_monitor/api/routes/history.py`
- Create: `src/cctv_monitor/api/routes/alerts_routes.py`
- Modify: `src/cctv_monitor/api/routes/devices.py` — update _device_out, get_device_detail, poll_device
- Modify: `src/cctv_monitor/api/app.py` — mount new routers
- Test: `tests/unit/api/test_tags_routes.py`

**Step 1: Create tags routes**

```python
# src/cctv_monitor/api/routes/tags.py
from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.exc import IntegrityError
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.api.deps import get_session
from cctv_monitor.api.schemas import TagCreate
from cctv_monitor.storage.repositories import DeviceTagRepository

router = APIRouter(tags=["tags"])

@router.get("/tags")
async def list_all_tags(session: AsyncSession = Depends(get_session)):
    repo = DeviceTagRepository(session)
    return await repo.get_all_unique_tags()

@router.post("/devices/{device_id}/tags", status_code=201)
async def add_tag(device_id: str, body: TagCreate, session: AsyncSession = Depends(get_session)):
    repo = DeviceTagRepository(session)
    try:
        await repo.add_tag(device_id, body.tag)
        await session.commit()
    except IntegrityError:
        await session.rollback()
        raise HTTPException(status_code=409, detail="Tag already exists")
    return {"tag": body.tag}

@router.delete("/devices/{device_id}/tags/{tag}", status_code=204)
async def remove_tag(device_id: str, tag: str, session: AsyncSession = Depends(get_session)):
    repo = DeviceTagRepository(session)
    deleted = await repo.remove_tag(device_id, tag)
    if not deleted:
        raise HTTPException(status_code=404, detail="Tag not found")
    await session.commit()
```

**Step 2: Create history route**

```python
# src/cctv_monitor/api/routes/history.py
from fastapi import APIRouter, Depends, Query
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.api.deps import get_session
from cctv_monitor.api.schemas import HealthLogEntryOut
from cctv_monitor.storage.repositories import DeviceHealthLogRepository

router = APIRouter(tags=["history"])

@router.get("/devices/{device_id}/history", response_model=list[HealthLogEntryOut])
async def get_device_history(
    device_id: str,
    hours: int = Query(default=24, ge=1, le=720),
    session: AsyncSession = Depends(get_session),
):
    repo = DeviceHealthLogRepository(session)
    entries = await repo.get_history(device_id, hours)
    return [
        HealthLogEntryOut(
            reachable=e.reachable, camera_count=e.camera_count,
            online_cameras=e.online_cameras, offline_cameras=e.offline_cameras,
            disk_ok=e.disk_ok, response_time_ms=e.response_time_ms,
            checked_at=e.checked_at,
        ) for e in entries
    ]
```

**Step 3: Create alerts route**

```python
# src/cctv_monitor/api/routes/alerts_routes.py
from fastapi import APIRouter, Depends, Query
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.api.deps import get_session
from cctv_monitor.api.schemas import AlertOut
from cctv_monitor.storage.repositories import AlertRepository

router = APIRouter(tags=["alerts"])

@router.get("/alerts", response_model=list[AlertOut])
async def list_alerts(
    status: str | None = Query(default=None),
    device_id: str | None = Query(default=None),
    session: AsyncSession = Depends(get_session),
):
    repo = AlertRepository(session)
    # Use existing get_active_alerts or extend with filters
    if device_id and status == "active":
        alerts = await repo.get_active_alerts(device_id)
    elif device_id:
        alerts = await repo.get_active_alerts(device_id)  # extend later
    else:
        alerts = await repo.get_all_alerts(status=status)
    return [
        AlertOut(
            id=a.id, alert_type=a.alert_type, severity=a.severity,
            message=a.message, status=a.status,
            created_at=a.created_at, resolved_at=a.resolved_at,
        ) for a in alerts
    ]
```

Note: You'll need to add `get_all_alerts(status)` to `AlertRepository`.

**Step 4: Update devices.py — _device_out helper**

Update `_device_out` in `src/cctv_monitor/api/routes/devices.py` to include new fields:

```python
def _device_out(d: DeviceTable, tags: list[str] | None = None) -> DeviceOut:
    return DeviceOut(
        device_id=d.device_id, name=d.name, vendor=d.vendor,
        host=d.host, web_port=d.web_port, sdk_port=d.sdk_port,
        transport_mode=d.transport_mode, is_active=d.is_active,
        last_health=None,
        model=d.model, serial_number=d.serial_number,
        firmware_version=d.firmware_version, last_poll_at=d.last_poll_at,
        tags=tags or [],
    )
```

**Step 5: Update get_device_detail to return cameras/disks from last_health_json**

```python
@router.get("/devices/{device_id}", response_model=DeviceDetailOut)
async def get_device_detail(device_id: str, session: AsyncSession = Depends(get_session)):
    repo = DeviceRepository(session)
    device = await repo.get_by_id(device_id)
    if device is None:
        raise HTTPException(status_code=404, detail="Device not found")

    tag_repo = DeviceTagRepository(session)
    tags = await tag_repo.get_tags(device_id)

    alert_repo = AlertRepository(session)
    alerts = await alert_repo.get_active_alerts(device_id)

    # Extract cameras/disks/health from cached last_health_json
    cached = device.last_health_json or {}
    cameras = [CameraChannelOut(**c) for c in cached.get("cameras", [])]
    disks = [DiskOut(**d) for d in cached.get("disks", [])]
    health_data = cached.get("health")
    health = HealthSummaryOut(**health_data) if health_data else None

    return DeviceDetailOut(
        device=_device_out(device, tags),
        cameras=cameras,
        disks=disks,
        alerts=[
            AlertOut(
                id=a.id, alert_type=a.alert_type, severity=a.severity,
                message=a.message, status=a.status, created_at=a.created_at,
                resolved_at=a.resolved_at,
            ) for a in alerts
        ],
        health=health,
    )
```

**Step 6: Update poll_device to save last_health_json**

After successful poll in `poll_device()`, add:

```python
    # Save full snapshot to device
    from datetime import datetime, timezone
    now = datetime.now(timezone.utc)

    cameras_data = [
        {"channel_id": c.channel_id, "channel_name": c.channel_name,
         "status": c.status.value, "ip_address": c.ip_address,
         "checked_at": now.isoformat()}
        for c in cameras
    ]
    disks_data = [
        {"disk_id": d.disk_id, "status": d.status.value,
         "capacity_bytes": d.capacity_bytes, "free_bytes": d.free_bytes,
         "health_status": d.health_status, "checked_at": now.isoformat()}
        for d in disks
    ]
    health_data = {
        "reachable": health.reachable, "camera_count": health.camera_count,
        "online_cameras": health.online_cameras, "offline_cameras": health.offline_cameras,
        "disk_ok": health.disk_ok, "response_time_ms": health.response_time_ms,
        "checked_at": now.isoformat(),
    }

    await repo.update(device_id,
        last_health_json={"health": health_data, "cameras": cameras_data, "disks": disks_data},
        last_poll_at=now,
        model=info.model if info else device.model,
        serial_number=info.serial_number if info else device.serial_number,
        firmware_version=info.firmware_version if info else device.firmware_version,
    )

    # Log to health_log
    health_log_repo = DeviceHealthLogRepository(session)
    await health_log_repo.insert(
        device_id=device_id, reachable=health.reachable,
        camera_count=health.camera_count, online_cameras=health.online_cameras,
        offline_cameras=health.offline_cameras, disk_ok=health.disk_ok,
        response_time_ms=health.response_time_ms,
    )
    await session.commit()
```

Note: This requires refactoring `poll_device` to call `get_device_info()`, `get_camera_statuses()`, and `get_disk_statuses()` individually instead of just `check_health()`. The `check_health()` method already calls them internally — extract the results.

**Step 7: Mount new routers in app.py**

Add to `src/cctv_monitor/api/app.py`:

```python
from cctv_monitor.api.routes.tags import router as tags_router
from cctv_monitor.api.routes.history import router as history_router
from cctv_monitor.api.routes.alerts_routes import router as alerts_router

app.include_router(tags_router, prefix="/api")
app.include_router(history_router, prefix="/api")
app.include_router(alerts_router, prefix="/api")
```

**Step 8: Run all backend tests**

Run: `python -m pytest tests/ -v`
Expected: ALL PASS

**Step 9: Commit**

```bash
git add -A && git commit -m "feat: add tags, history, alerts API routes + update poll to save full data"
```

---

### Task 5: Background polling scheduler job

**Files:**
- Create: `src/cctv_monitor/polling/background.py`
- Modify: `src/cctv_monitor/main.py` — register job with scheduler
- Test: `tests/unit/polling/test_background.py`

**Step 1: Write the background poll job**

```python
# src/cctv_monitor/polling/background.py
"""Background job that polls all active devices."""
from __future__ import annotations

import structlog

from cctv_monitor.core.config import Settings
from cctv_monitor.core.crypto import decrypt_value
from cctv_monitor.core.types import DeviceTransport, DeviceVendor
from cctv_monitor.drivers.registry import DriverRegistry
from cctv_monitor.models.device import DeviceConfig

logger = structlog.get_logger()


async def poll_all_devices(
    session_factory,
    settings: Settings,
    registry: DriverRegistry,
    http_client,
    sdk_binding_getter,
) -> None:
    """Poll every active device, save results to DB."""
    from datetime import datetime, timezone
    from cctv_monitor.drivers.hikvision.transports.isapi import IsapiTransport
    from cctv_monitor.drivers.hikvision.transports.sdk import SdkTransport
    from cctv_monitor.storage.repositories import (
        DeviceRepository, DeviceHealthLogRepository,
    )

    async with session_factory() as session:
        repo = DeviceRepository(session)
        devices = await repo.get_active_devices()

        for device in devices:
            device_id = device.device_id
            try:
                vendor = DeviceVendor(device.vendor)
                driver_cls = registry.get(vendor)
                transport_mode = device.transport_mode or "isapi"

                if transport_mode == "sdk":
                    sdk_binding = sdk_binding_getter()
                    if not sdk_binding or not device.sdk_port:
                        logger.warning("bg_poll.skip_no_sdk", device=device_id)
                        continue
                    transport = SdkTransport(binding=sdk_binding)
                    connect_port = device.sdk_port
                else:
                    if not device.web_port:
                        logger.warning("bg_poll.skip_no_port", device=device_id)
                        continue
                    transport = IsapiTransport(http_client)
                    connect_port = device.web_port

                driver = driver_cls(transport)
                password = decrypt_value(device.password_encrypted, settings.ENCRYPTION_KEY)
                config = DeviceConfig(
                    device_id=device_id, name=device.name, vendor=vendor,
                    host=device.host, web_port=device.web_port,
                    sdk_port=device.sdk_port, username=device.username,
                    password=password, transport_mode=DeviceTransport(transport_mode),
                    polling_policy_id=device.polling_policy_id,
                    is_active=device.is_active,
                )

                now = datetime.now(timezone.utc)
                import time
                start = time.monotonic()

                try:
                    await driver.connect(config, port=connect_port)
                    health = await driver.check_health()
                    cameras = await driver.get_camera_statuses()
                    disks = await driver.get_disk_statuses()
                    info = await driver.get_device_info()
                except Exception as exc:
                    logger.warning("bg_poll.device_error", device=device_id, error=str(exc))
                    # Save unreachable state
                    await repo.update(device_id, last_poll_at=now, last_health_json={
                        "health": {"reachable": False, "camera_count": 0, "online_cameras": 0,
                                   "offline_cameras": 0, "disk_ok": False,
                                   "response_time_ms": (time.monotonic() - start) * 1000,
                                   "checked_at": now.isoformat()},
                        "cameras": [], "disks": [],
                    })
                    health_log = DeviceHealthLogRepository(session)
                    await health_log.insert(
                        device_id=device_id, reachable=False, camera_count=0,
                        online_cameras=0, offline_cameras=0, disk_ok=False,
                        response_time_ms=(time.monotonic() - start) * 1000,
                    )
                    await session.commit()
                    continue
                finally:
                    try:
                        await driver.disconnect()
                    except Exception:
                        pass

                response_time = (time.monotonic() - start) * 1000

                cameras_data = [
                    {"channel_id": c.channel_id, "channel_name": c.channel_name,
                     "status": c.status.value, "ip_address": getattr(c, "ip_address", None),
                     "checked_at": now.isoformat()}
                    for c in cameras
                ]
                disks_data = [
                    {"disk_id": d.disk_id, "status": d.status.value,
                     "capacity_bytes": d.capacity_bytes, "free_bytes": d.free_bytes,
                     "health_status": d.health_status, "checked_at": now.isoformat()}
                    for d in disks
                ]
                health_json = {
                    "reachable": health.reachable,
                    "camera_count": health.camera_count,
                    "online_cameras": health.online_cameras,
                    "offline_cameras": health.offline_cameras,
                    "disk_ok": health.disk_ok,
                    "response_time_ms": health.response_time_ms,
                    "checked_at": now.isoformat(),
                }

                await repo.update(device_id,
                    last_poll_at=now,
                    last_health_json={"health": health_json, "cameras": cameras_data, "disks": disks_data},
                    model=getattr(info, "model", None) or device.model,
                    serial_number=getattr(info, "serial_number", None) or device.serial_number,
                    firmware_version=getattr(info, "firmware_version", None) or device.firmware_version,
                )
                health_log = DeviceHealthLogRepository(session)
                await health_log.insert(
                    device_id=device_id, reachable=health.reachable,
                    camera_count=health.camera_count, online_cameras=health.online_cameras,
                    offline_cameras=health.offline_cameras, disk_ok=health.disk_ok,
                    response_time_ms=health.response_time_ms,
                )
                await session.commit()

                logger.info("bg_poll.ok", device=device_id,
                            cameras=f"{health.online_cameras}/{health.camera_count}",
                            response_ms=round(health.response_time_ms))

            except Exception as exc:
                logger.error("bg_poll.unexpected", device=device_id, error=str(exc))
                await session.rollback()
```

**Step 2: Register job in main.py**

Add after `scheduler.start()` in `src/cctv_monitor/main.py`:

```python
    from cctv_monitor.polling.background import poll_all_devices
    from cctv_monitor.api.deps import get_sdk_binding

    def _get_sdk_binding():
        return getattr(app.state, "sdk_binding", None)

    scheduler.add_job(
        poll_all_devices,
        "interval",
        minutes=2,
        args=[session_factory, settings, registry, http_client, _get_sdk_binding],
        id="poll_all_devices",
        replace_existing=True,
    )
    logger.info("scheduler.job_registered", job="poll_all_devices", interval_min=2)
```

**Step 3: Write basic test**

```python
# tests/unit/polling/test_background.py
import pytest
from unittest.mock import AsyncMock, MagicMock, patch
from cctv_monitor.polling.background import poll_all_devices

@pytest.mark.asyncio
async def test_poll_all_devices_empty():
    """When no active devices, should complete without error."""
    session = AsyncMock()
    session.execute = AsyncMock(return_value=MagicMock(
        scalars=MagicMock(return_value=MagicMock(all=MagicMock(return_value=[])))
    ))
    session_factory = AsyncMock()
    session_factory.return_value.__aenter__ = AsyncMock(return_value=session)
    session_factory.return_value.__aexit__ = AsyncMock(return_value=False)

    settings = MagicMock()
    registry = MagicMock()
    http_client = MagicMock()

    await poll_all_devices(session_factory, settings, registry, http_client, lambda: None)
```

**Step 4: Run tests**

Run: `python -m pytest tests/unit/polling/test_background.py -v`
Expected: PASS

**Step 5: Commit**

```bash
git add -A && git commit -m "feat: add background polling job — polls all devices every 2 min"
```

---

## Phase 2: Frontend (MUI)

### Task 6: Install MUI dependencies

**Step 1: Install packages**

```bash
cd "c:\Users\Pavel\DEV\CCTV Monitoring Tool\frontend"
npm install @mui/material @mui/icons-material @emotion/react @emotion/styled @mui/x-data-grid @mui/x-charts
```

**Step 2: Commit**

```bash
git add -A && git commit -m "chore: install MUI, DataGrid, Charts dependencies"
```

---

### Task 7: Update TypeScript types and API client

**Files:**
- Modify: `frontend/src/types.ts`
- Modify: `frontend/src/api/client.ts`

**Step 1: Update types.ts**

Add to existing interfaces:

```typescript
// Add to Device interface:
  model?: string;
  serial_number?: string;
  firmware_version?: string;
  last_poll_at?: string;
  tags: string[];

// Add to DeviceDetail interface:
  health?: HealthSummary;

// New interfaces:
export interface HealthLogEntry {
  reachable: boolean;
  camera_count: number;
  online_cameras: number;
  offline_cameras: number;
  disk_ok: boolean;
  response_time_ms: number;
  checked_at: string;
}
```

**Step 2: Update api/client.ts**

Add new methods to the `api` object:

```typescript
  // Tags
  getTags: () => request<string[]>('/tags'),
  addTag: (deviceId: string, tag: string) =>
    request<{tag: string}>(`/devices/${deviceId}/tags`, {
      method: 'POST', headers: {'Content-Type': 'application/json'},
      body: JSON.stringify({tag}),
    }),
  removeTag: (deviceId: string, tag: string) =>
    request<void>(`/devices/${deviceId}/tags/${tag}`, {method: 'DELETE'}),

  // History
  getDeviceHistory: (deviceId: string, hours = 24) =>
    request<HealthLogEntry[]>(`/devices/${deviceId}/history?hours=${hours}`),

  // Alerts
  getAlerts: (params?: {status?: string; device_id?: string}) => {
    const qs = new URLSearchParams(params as Record<string, string>).toString();
    return request<Alert[]>(`/alerts${qs ? '?' + qs : ''}`);
  },

  // Snapshot
  getSnapshotUrl: (deviceId: string, channelId: string) =>
    `${BASE}/devices/${deviceId}/snapshot/${channelId}`,
```

**Step 3: Commit**

```bash
git add -A && git commit -m "feat: update frontend types and API client for v2"
```

---

### Task 8: MUI theme + Layout component

**Files:**
- Create: `frontend/src/theme.ts`
- Modify: `frontend/src/App.tsx`
- Modify: `frontend/src/components/Layout.tsx`

**Step 1: Create MUI theme**

```typescript
// frontend/src/theme.ts
import { createTheme } from '@mui/material/styles';

export const theme = createTheme({
  palette: {
    primary: { main: '#1a1a2e' },
    secondary: { main: '#16213e' },
    success: { main: '#4caf50' },
    error: { main: '#f44336' },
    warning: { main: '#ff9800' },
  },
});
```

**Step 2: Update App.tsx with ThemeProvider and new routes**

```typescript
import { ThemeProvider, CssBaseline } from '@mui/material';
import { theme } from './theme';
// Add new page imports: Dashboard, Alerts

// Wrap with ThemeProvider + CssBaseline
// Add routes: "/" -> Dashboard, "/devices" -> DeviceList, "/alerts" -> Alerts
```

**Step 3: Update Layout with MUI AppBar + Drawer navigation**

Use MUI `AppBar`, `Toolbar`, `Drawer`, `List`, `ListItem` for sidebar nav with links: Dashboard, Devices, Alerts.

**Step 4: Commit**

```bash
git add -A && git commit -m "feat: add MUI theme, AppBar layout, routing"
```

---

### Task 9: Dashboard page

**Files:**
- Create: `frontend/src/pages/Dashboard.tsx`

**Step 1: Build Dashboard with MUI Cards + mini alerts table**

- 4 summary cards (MUI `Card`): Total Devices, Online, Offline, Active Alerts
- Recent alerts table (MUI `Table`)
- Uses `api.getOverview()` and `api.getAlerts({status: 'active'})`
- Auto-refresh every 30 seconds

**Step 2: Commit**

```bash
git add -A && git commit -m "feat: add Dashboard page with MUI cards"
```

---

### Task 10: Device List page (MUI DataGrid)

**Files:**
- Rewrite: `frontend/src/pages/DeviceList.tsx`

**Step 1: Rebuild with MUI DataGrid**

- Columns: name, host, status (chip), cameras (online/total), disks, response_time, last_poll, actions
- Tag filter: MUI `Chip` + `Autocomplete` using `api.getTags()`
- Search: MUI `TextField` filtering by name/IP
- Row click navigates to device detail
- Actions column: Poll Now, Edit, Delete buttons

**Step 2: Commit**

```bash
git add -A && git commit -m "feat: rebuild DeviceList with MUI DataGrid + tag filters"
```

---

### Task 11: Device Detail page (tabs, cameras, disks, history)

**Files:**
- Rewrite: `frontend/src/pages/DeviceDetail.tsx`

This is the largest frontend task. Build with MUI `Tabs`:

**Step 1: Header + Connection Info section**

- Device name, model, serial, firmware as `Typography`
- Status `Chip` (ONLINE green / OFFLINE red / UNKNOWN gray)
- Connection info: host, ports, transport mode
- Tags: editable `Chip` array with add/remove

**Step 2: Cameras Tab**

- Grid of MUI `Card` components, one per camera channel
- Each card: status icon (green/red/gray circle), channel name, IP address
- Snapshot button on each card (opens image in MUI `Dialog`)

**Step 3: Disks Tab**

- MUI `DataGrid` with columns: disk_id, capacity_gb, free_gb, used_percent (with MUI `LinearProgress`), status, health
- Color-code rows: green for ok, red for error

**Step 4: History Tab**

- MUI X Charts `LineChart` — two lines: response_time_ms, online_cameras
- Time range selector: 1h, 6h, 24h, 7d
- Uses `api.getDeviceHistory(deviceId, hours)`

**Step 5: Alerts Tab**

- MUI `Table` of device alerts
- Status, severity, message, created_at

**Step 6: Actions**

- Poll Now button (with loading spinner)
- Edit / Delete buttons

**Step 7: Commit**

```bash
git add -A && git commit -m "feat: rebuild DeviceDetail with MUI tabs, cameras, disks, history charts"
```

---

### Task 12: Alerts page

**Files:**
- Create: `frontend/src/pages/Alerts.tsx`

**Step 1: Build alerts page**

- MUI DataGrid with all alerts
- Filter by: status (active/resolved), severity, device
- Click to navigate to device detail

**Step 2: Commit**

```bash
git add -A && git commit -m "feat: add Alerts page with MUI DataGrid"
```

---

### Task 13: Update Add/Edit Device forms

**Files:**
- Modify: `frontend/src/pages/AddDevice.tsx`
- Modify: `frontend/src/pages/EditDevice.tsx`

**Step 1: Convert forms to MUI**

- Use MUI `TextField`, `Select`, `Switch`, `Button`
- Add tag management (MUI `Autocomplete` with freeSolo for new tags)
- Consistent styling with rest of app

**Step 2: Commit**

```bash
git add -A && git commit -m "feat: convert Add/Edit forms to MUI with tag management"
```

---

## Phase 3: Integration & Polish

### Task 14: Snapshot proxy endpoint

**Files:**
- Modify: `src/cctv_monitor/api/routes/devices.py`

**Step 1: Add snapshot endpoint**

```python
@router.get("/devices/{device_id}/snapshot/{channel_id}")
async def get_snapshot(device_id: str, channel_id: str, ...):
    # Connect to device, get snapshot bytes, return as StreamingResponse
    from fastapi.responses import StreamingResponse
    import io
    # ... connect, get snapshot, disconnect ...
    return StreamingResponse(io.BytesIO(image_data), media_type="image/jpeg")
```

**Step 2: Commit**

```bash
git add -A && git commit -m "feat: add snapshot proxy endpoint"
```

---

### Task 15: Final integration test & cleanup

**Step 1: Run full backend test suite**

Run: `python -m pytest tests/ -v`
Expected: ALL PASS

**Step 2: Run frontend dev server and verify**

Run: `cd frontend && npm run dev`
Verify: Dashboard, Device List, Device Detail, Alerts all work

**Step 3: Clean up old CSS (App.css) — remove styles replaced by MUI**

**Step 4: Commit**

```bash
git add -A && git commit -m "chore: final cleanup, remove legacy CSS"
```

---

## Execution Order Summary

| Phase | Task | Description | Est. Complexity |
|-------|------|-------------|-----------------|
| 1 | 1 | DB schema: new tables + columns | Medium |
| 1 | 2 | Repositories: tags, health_log | Small |
| 1 | 3 | API schemas: v2 fields | Small |
| 1 | 4 | API routes: tags, history, alerts, updated poll | Large |
| 1 | 5 | Background scheduler job | Medium |
| 2 | 6 | Install MUI | Trivial |
| 2 | 7 | Frontend types + API client | Small |
| 2 | 8 | MUI theme + Layout | Medium |
| 2 | 9 | Dashboard page | Medium |
| 2 | 10 | Device List (DataGrid) | Medium |
| 2 | 11 | Device Detail (tabs, charts) | Large |
| 2 | 12 | Alerts page | Small |
| 2 | 13 | Add/Edit forms (MUI) | Medium |
| 3 | 14 | Snapshot endpoint | Small |
| 3 | 15 | Integration test + cleanup | Small |
