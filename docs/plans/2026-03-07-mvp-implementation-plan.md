# CCTV Monitoring Platform — MVP Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Реализовать ядро системы мониторинга CCTV-инфраструктуры с Hikvision ISAPI драйвером, polling scheduler, alert engine и минимальным API.

**Architecture:** Модульный монолит Python. Flat module structure `cctv_monitor/` с driver abstraction через Protocol. Hikvision driver с transport layer (ISAPI + SDK stub). PostgreSQL 16 для хранения, файловая система для snapshots. APScheduler для polling, FastAPI для API.

**Tech Stack:** Python 3.12, PostgreSQL 16, SQLAlchemy 2.0, Alembic, httpx, APScheduler, FastAPI, cryptography (Fernet), pytest, Docker Compose.

---

## Phase 1: Project Foundation

### Task 1: Инициализация Python-проекта

**Files:**
- Create: `pyproject.toml`
- Create: `src/cctv_monitor/__init__.py`
- Create: `src/cctv_monitor/main.py`
- Create: `tests/__init__.py`
- Create: `.gitignore`
- Create: `.env.example`

**Step 1: Создать pyproject.toml**

```toml
[project]
name = "cctv-monitor"
version = "0.1.0"
description = "CCTV infrastructure monitoring platform"
requires-python = ">=3.12"
dependencies = [
    "sqlalchemy[asyncio]>=2.0,<3.0",
    "asyncpg>=0.29",
    "alembic>=1.13",
    "httpx>=0.27",
    "apscheduler>=3.10,<4.0",
    "fastapi>=0.110",
    "uvicorn>=0.29",
    "cryptography>=42.0",
    "pydantic>=2.6",
    "pydantic-settings>=2.2",
    "pyyaml>=6.0",
    "structlog>=24.1",
]

[project.optional-dependencies]
dev = [
    "pytest>=8.0",
    "pytest-asyncio>=0.23",
    "pytest-cov>=5.0",
    "ruff>=0.3",
]

[build-system]
requires = ["setuptools>=68.0"]
build-backend = "setuptools.backends._legacy:_Backend"

[tool.setuptools.packages.find]
where = ["src"]

[tool.pytest.ini_options]
testpaths = ["tests"]
asyncio_mode = "auto"

[tool.ruff]
target-version = "py312"
line-length = 100
```

**Step 2: Создать .env.example**

```env
POSTGRES_USER=cctv_admin
POSTGRES_PASSWORD=changeme
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DB=cctv_monitoring
ENCRYPTION_KEY=
SNAPSHOT_BASE_DIR=./data/snapshots
LOG_LEVEL=INFO
```

**Step 3: Создать .gitignore**

```gitignore
__pycache__/
*.py[cod]
*.egg-info/
dist/
build/
.eggs/
*.egg
.env
.venv/
venv/
data/snapshots/
third_party/hikvision/device-network-sdk/original/
*.dll
*.so
*.dylib
.pytest_cache/
.coverage
htmlcov/
.ruff_cache/
```

**Step 4: Создать базовые файлы пакета**

```python
# src/cctv_monitor/__init__.py
"""CCTV Monitoring Platform."""
```

```python
# src/cctv_monitor/main.py
"""Application entry point."""

import asyncio
import structlog

logger = structlog.get_logger()


async def main() -> None:
    logger.info("cctv_monitor.starting")
    # TODO: initialize components
    logger.info("cctv_monitor.started")


if __name__ == "__main__":
    asyncio.run(main())
```

```python
# tests/__init__.py
```

**Step 5: Создать виртуальное окружение и установить зависимости**

Run: `python -m venv venv && venv/Scripts/pip install -e ".[dev]"`

**Step 6: Проверить что проект запускается**

Run: `venv/Scripts/python -m cctv_monitor.main`
Expected: логи "cctv_monitor.starting" и "cctv_monitor.started"

**Step 7: Commit**

```bash
git init
git add pyproject.toml src/ tests/ .gitignore .env.example
git commit -m "feat: initialize Python project with dependencies"
```

---

### Task 2: Docker Compose для PostgreSQL

**Files:**
- Create: `docker-compose.yml`

**Step 1: Создать docker-compose.yml**

```yaml
services:
  cctv_monitoring_postgres:
    image: postgres:16
    container_name: cctv_monitoring_postgres
    environment:
      POSTGRES_DB: ${POSTGRES_DB:-cctv_monitoring}
      POSTGRES_USER: ${POSTGRES_USER:-cctv_admin}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-changeme}
    ports:
      - "${POSTGRES_PORT:-5432}:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER:-cctv_admin} -d ${POSTGRES_DB:-cctv_monitoring}"]
      interval: 5s
      timeout: 5s
      retries: 5

volumes:
  pgdata:
```

**Step 2: Запустить и проверить**

Run: `docker compose up -d`
Run: `docker compose ps`
Expected: контейнер `cctv_monitoring_postgres` в состоянии `healthy`

**Step 3: Commit**

```bash
git add docker-compose.yml
git commit -m "infra: add Docker Compose for PostgreSQL 16"
```

---

### Task 3: Конфигурация приложения (Settings)

**Files:**
- Create: `src/cctv_monitor/core/__init__.py`
- Create: `src/cctv_monitor/core/config.py`
- Test: `tests/unit/__init__.py`
- Test: `tests/unit/core/__init__.py`
- Test: `tests/unit/core/test_config.py`

**Step 1: Написать тест**

```python
# tests/unit/core/test_config.py
import os
from cctv_monitor.core.config import Settings


def test_settings_loads_defaults():
    settings = Settings(
        POSTGRES_PASSWORD="test",
        ENCRYPTION_KEY="dGVzdGtleQ==",
    )
    assert settings.POSTGRES_USER == "cctv_admin"
    assert settings.POSTGRES_HOST == "localhost"
    assert settings.POSTGRES_PORT == 5432
    assert settings.POSTGRES_DB == "cctv_monitoring"
    assert settings.LOG_LEVEL == "INFO"


def test_settings_database_url():
    settings = Settings(
        POSTGRES_PASSWORD="testpass",
        POSTGRES_USER="user",
        POSTGRES_HOST="db",
        POSTGRES_PORT=5433,
        POSTGRES_DB="mydb",
        ENCRYPTION_KEY="dGVzdGtleQ==",
    )
    assert "user:testpass@db:5433/mydb" in settings.database_url


def test_settings_snapshot_base_dir_default():
    settings = Settings(
        POSTGRES_PASSWORD="test",
        ENCRYPTION_KEY="dGVzdGtleQ==",
    )
    assert settings.SNAPSHOT_BASE_DIR == "./data/snapshots"
```

**Step 2: Запустить тест — убедиться что падает**

Run: `venv/Scripts/pytest tests/unit/core/test_config.py -v`
Expected: FAIL — модуль не найден

**Step 3: Реализовать Settings**

```python
# src/cctv_monitor/core/__init__.py
```

```python
# src/cctv_monitor/core/config.py
from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    POSTGRES_USER: str = "cctv_admin"
    POSTGRES_PASSWORD: str
    POSTGRES_HOST: str = "localhost"
    POSTGRES_PORT: int = 5432
    POSTGRES_DB: str = "cctv_monitoring"
    ENCRYPTION_KEY: str
    SNAPSHOT_BASE_DIR: str = "./data/snapshots"
    LOG_LEVEL: str = "INFO"

    model_config = {"env_file": ".env", "env_file_encoding": "utf-8"}

    @property
    def database_url(self) -> str:
        return (
            f"postgresql+asyncpg://{self.POSTGRES_USER}:{self.POSTGRES_PASSWORD}"
            f"@{self.POSTGRES_HOST}:{self.POSTGRES_PORT}/{self.POSTGRES_DB}"
        )

    @property
    def database_url_sync(self) -> str:
        return (
            f"postgresql://{self.POSTGRES_USER}:{self.POSTGRES_PASSWORD}"
            f"@{self.POSTGRES_HOST}:{self.POSTGRES_PORT}/{self.POSTGRES_DB}"
        )
```

**Step 4: Запустить тест — убедиться что проходит**

Run: `venv/Scripts/pytest tests/unit/core/test_config.py -v`
Expected: 3 passed

**Step 5: Commit**

```bash
git add src/cctv_monitor/core/ tests/unit/
git commit -m "feat: add application settings with pydantic-settings"
```

---

## Phase 2: Core Layer

### Task 4: Unified Error Model

**Files:**
- Create: `src/cctv_monitor/core/errors.py`
- Test: `tests/unit/core/test_errors.py`

**Step 1: Написать тест**

```python
# tests/unit/core/test_errors.py
from cctv_monitor.core.errors import (
    CctvMonitorError,
    DeviceConnectionError,
    DeviceAuthError,
    DeviceTimeoutError,
    EndpointNotSupportedError,
    DriverError,
)


def test_device_connection_error():
    err = DeviceConnectionError(device_id="nvr-01", message="Connection refused")
    assert err.device_id == "nvr-01"
    assert "Connection refused" in str(err)
    assert isinstance(err, CctvMonitorError)


def test_device_auth_error():
    err = DeviceAuthError(device_id="nvr-01", message="Invalid credentials")
    assert err.device_id == "nvr-01"
    assert isinstance(err, DeviceConnectionError)


def test_device_timeout_error():
    err = DeviceTimeoutError(device_id="nvr-01", timeout_ms=5000)
    assert err.timeout_ms == 5000
    assert isinstance(err, DeviceConnectionError)


def test_endpoint_not_supported_error():
    err = EndpointNotSupportedError(
        device_id="nvr-01", endpoint="/ISAPI/some/path"
    )
    assert err.endpoint == "/ISAPI/some/path"
    assert isinstance(err, DriverError)
```

**Step 2: Запустить тест — убедиться что падает**

Run: `venv/Scripts/pytest tests/unit/core/test_errors.py -v`
Expected: FAIL

**Step 3: Реализовать error model**

```python
# src/cctv_monitor/core/errors.py
class CctvMonitorError(Exception):
    """Base error for all CCTV Monitor errors."""


class DriverError(CctvMonitorError):
    """Base error for driver-level errors."""

    def __init__(self, device_id: str, message: str = "") -> None:
        self.device_id = device_id
        super().__init__(f"[{device_id}] {message}")


class DeviceConnectionError(DriverError):
    """Device is unreachable."""


class DeviceAuthError(DeviceConnectionError):
    """Authentication failed."""


class DeviceTimeoutError(DeviceConnectionError):
    """Device did not respond in time."""

    def __init__(self, device_id: str, timeout_ms: int) -> None:
        self.timeout_ms = timeout_ms
        super().__init__(device_id, f"Timeout after {timeout_ms}ms")


class EndpointNotSupportedError(DriverError):
    """Device does not support requested endpoint."""

    def __init__(self, device_id: str, endpoint: str) -> None:
        self.endpoint = endpoint
        super().__init__(device_id, f"Endpoint not supported: {endpoint}")
```

**Step 4: Запустить тест**

Run: `venv/Scripts/pytest tests/unit/core/test_errors.py -v`
Expected: 4 passed

**Step 5: Commit**

```bash
git add src/cctv_monitor/core/errors.py tests/unit/core/test_errors.py
git commit -m "feat: add unified error model"
```

---

### Task 5: Core Types и Enums

**Files:**
- Create: `src/cctv_monitor/core/types.py`
- Test: `tests/unit/core/test_types.py`

**Step 1: Написать тест**

```python
# tests/unit/core/test_types.py
from cctv_monitor.core.types import (
    TransportMode,
    CheckType,
    DiskStatus,
    AlertType,
    Severity,
    AlertStatus,
    Vendor,
)


def test_transport_mode_values():
    assert TransportMode.ISAPI == "isapi"
    assert TransportMode.SDK == "sdk"
    assert TransportMode.AUTO == "auto"


def test_check_type_values():
    assert CheckType.DEVICE_INFO == "device_info"
    assert CheckType.CAMERA_STATUS == "camera_status"
    assert CheckType.DISK_STATUS == "disk_status"
    assert CheckType.RECORDING_STATUS == "recording_status"
    assert CheckType.SNAPSHOT == "snapshot"


def test_vendor_values():
    assert Vendor.HIKVISION == "hikvision"
    assert Vendor.DAHUA == "dahua"
    assert Vendor.PROVISION == "provision"


def test_alert_status_values():
    assert AlertStatus.ACTIVE == "active"
    assert AlertStatus.RESOLVED == "resolved"
```

**Step 2: Запустить тест — FAIL**

Run: `venv/Scripts/pytest tests/unit/core/test_types.py -v`

**Step 3: Реализовать**

```python
# src/cctv_monitor/core/types.py
from enum import StrEnum


class Vendor(StrEnum):
    HIKVISION = "hikvision"
    DAHUA = "dahua"
    PROVISION = "provision"


class TransportMode(StrEnum):
    ISAPI = "isapi"
    SDK = "sdk"
    AUTO = "auto"


class CheckType(StrEnum):
    DEVICE_INFO = "device_info"
    CAMERA_STATUS = "camera_status"
    DISK_STATUS = "disk_status"
    RECORDING_STATUS = "recording_status"
    SNAPSHOT = "snapshot"


class DiskStatus(StrEnum):
    OK = "ok"
    ERROR = "error"
    UNFORMATTED = "unformatted"
    IDLE = "idle"
    UNKNOWN = "unknown"


class AlertType(StrEnum):
    DEVICE_UNREACHABLE = "device_unreachable"
    CAMERA_OFFLINE = "camera_offline"
    DISK_ERROR = "disk_error"
    RECORDING_STOPPED = "recording_stopped"
    SNAPSHOT_FAILED = "snapshot_failed"


class Severity(StrEnum):
    WARNING = "warning"
    CRITICAL = "critical"


class AlertStatus(StrEnum):
    ACTIVE = "active"
    RESOLVED = "resolved"
```

**Step 4: Запустить тест — PASS**

Run: `venv/Scripts/pytest tests/unit/core/test_types.py -v`
Expected: 4 passed

**Step 5: Commit**

```bash
git add src/cctv_monitor/core/types.py tests/unit/core/test_types.py
git commit -m "feat: add core types and enums"
```

---

### Task 6: Normalized Models

**Files:**
- Create: `src/cctv_monitor/models/__init__.py`
- Create: `src/cctv_monitor/models/device.py`
- Create: `src/cctv_monitor/models/status.py`
- Create: `src/cctv_monitor/models/device_health.py`
- Create: `src/cctv_monitor/models/check_result.py`
- Create: `src/cctv_monitor/models/capabilities.py`
- Create: `src/cctv_monitor/models/polling_policy.py`
- Create: `src/cctv_monitor/models/snapshot.py`
- Create: `src/cctv_monitor/models/alert.py`
- Test: `tests/unit/models/__init__.py`
- Test: `tests/unit/models/test_models.py`

**Step 1: Написать тест**

```python
# tests/unit/models/test_models.py
from datetime import datetime, timezone
from cctv_monitor.models.device import DeviceConfig
from cctv_monitor.models.status import CameraStatus, DiskHealth, RecordingStatus
from cctv_monitor.models.device_health import DeviceHealthSummary
from cctv_monitor.models.check_result import DeviceCheckResult
from cctv_monitor.models.capabilities import DeviceCapabilities
from cctv_monitor.models.polling_policy import PollingPolicy
from cctv_monitor.models.snapshot import SnapshotResult
from cctv_monitor.models.alert import AlertEvent
from cctv_monitor.core.types import (
    TransportMode, Vendor, CheckType, DiskStatus,
    AlertType, Severity, AlertStatus,
)


def test_device_config_creation():
    config = DeviceConfig(
        device_id="nvr-01",
        name="Office NVR",
        vendor=Vendor.HIKVISION,
        host="192.168.1.100",
        port=80,
        username="admin",
        password="encrypted_password",
        transport_mode=TransportMode.ISAPI,
        polling_policy_id="standard",
        is_active=True,
    )
    assert config.device_id == "nvr-01"
    assert config.vendor == Vendor.HIKVISION


def test_camera_status_creation():
    status = CameraStatus(
        device_id="nvr-01",
        channel_id="101",
        channel_name="Front Door",
        is_online=True,
        ip_address="192.168.1.10",
        protocol="TCP",
        checked_at=datetime.now(timezone.utc),
    )
    assert status.is_online is True
    assert status.channel_id == "101"


def test_device_health_summary():
    summary = DeviceHealthSummary(
        device_id="nvr-01",
        reachable=True,
        camera_count=4,
        online_cameras=3,
        offline_cameras=1,
        disk_ok=True,
        recording_ok=True,
        response_time_ms=150.5,
        checked_at=datetime.now(timezone.utc),
    )
    assert summary.offline_cameras == 1
    assert summary.reachable is True


def test_polling_policy():
    policy = PollingPolicy(
        name="standard",
        device_info_interval=300,
        camera_status_interval=120,
        disk_status_interval=600,
        snapshot_interval=900,
    )
    assert policy.name == "standard"
    assert policy.camera_status_interval == 120


def test_alert_event():
    alert = AlertEvent(
        id=1,
        device_id="nvr-01",
        channel_id="101",
        alert_type=AlertType.CAMERA_OFFLINE,
        severity=Severity.CRITICAL,
        message="Camera Front Door is offline",
        source="polling",
        status=AlertStatus.ACTIVE,
        created_at=datetime.now(timezone.utc),
        resolved_at=None,
    )
    assert alert.status == AlertStatus.ACTIVE
    assert alert.resolved_at is None
```

**Step 2: Запустить тест — FAIL**

Run: `venv/Scripts/pytest tests/unit/models/test_models.py -v`

**Step 3: Реализовать все модели**

```python
# src/cctv_monitor/models/__init__.py
```

```python
# src/cctv_monitor/models/device.py
from dataclasses import dataclass
from cctv_monitor.core.types import TransportMode, Vendor


@dataclass
class DeviceConfig:
    device_id: str
    name: str
    vendor: Vendor
    host: str
    port: int
    username: str
    password: str  # encrypted
    transport_mode: TransportMode
    polling_policy_id: str
    is_active: bool


@dataclass
class DeviceInfo:
    device_id: str
    model: str
    serial_number: str
    firmware_version: str
    device_type: str
    mac_address: str | None
    channels_count: int
```

```python
# src/cctv_monitor/models/status.py
from dataclasses import dataclass
from datetime import datetime
from cctv_monitor.core.types import DiskStatus


@dataclass
class CameraStatus:
    device_id: str
    channel_id: str
    channel_name: str
    is_online: bool
    ip_address: str | None
    protocol: str | None
    checked_at: datetime


@dataclass
class DiskHealth:
    device_id: str
    disk_id: str
    status: DiskStatus
    capacity_bytes: int
    free_bytes: int
    health_status: str
    checked_at: datetime


@dataclass
class RecordingStatus:
    device_id: str
    channel_id: str
    is_recording: bool
    record_type: str | None
    checked_at: datetime
```

```python
# src/cctv_monitor/models/device_health.py
from dataclasses import dataclass
from datetime import datetime


@dataclass
class DeviceHealthSummary:
    device_id: str
    reachable: bool
    camera_count: int
    online_cameras: int
    offline_cameras: int
    disk_ok: bool
    recording_ok: bool
    response_time_ms: float
    checked_at: datetime
```

```python
# src/cctv_monitor/models/check_result.py
from dataclasses import dataclass, field
from datetime import datetime
from cctv_monitor.core.types import CheckType


@dataclass
class DeviceCheckResult:
    device_id: str
    check_type: CheckType
    success: bool
    duration_ms: float
    checked_at: datetime
    id: int | None = None
    error_type: str | None = None
    payload_json: dict | None = field(default=None)
```

```python
# src/cctv_monitor/models/capabilities.py
from dataclasses import dataclass
from datetime import datetime


@dataclass
class DeviceCapabilities:
    device_id: str
    model: str
    firmware_version: str
    supports_isapi: bool
    supports_sdk: bool
    supports_snapshot: bool
    supports_recording_status: bool
    supports_disk_status: bool
    detected_at: datetime
```

```python
# src/cctv_monitor/models/polling_policy.py
from dataclasses import dataclass


@dataclass
class PollingPolicy:
    name: str
    device_info_interval: int
    camera_status_interval: int
    disk_status_interval: int
    snapshot_interval: int
```

```python
# src/cctv_monitor/models/snapshot.py
from dataclasses import dataclass
from datetime import datetime


@dataclass
class SnapshotResult:
    device_id: str
    channel_id: str
    success: bool
    checked_at: datetime
    file_path: str | None = None
    file_size_bytes: int | None = None
    error: str | None = None
```

```python
# src/cctv_monitor/models/alert.py
from dataclasses import dataclass
from datetime import datetime
from cctv_monitor.core.types import AlertType, Severity, AlertStatus


@dataclass
class AlertEvent:
    device_id: str
    alert_type: AlertType
    severity: Severity
    message: str
    source: str
    status: AlertStatus
    created_at: datetime
    id: int | None = None
    channel_id: str | None = None
    resolved_at: datetime | None = None
```

**Step 4: Запустить тест — PASS**

Run: `venv/Scripts/pytest tests/unit/models/test_models.py -v`
Expected: 5 passed

**Step 5: Commit**

```bash
git add src/cctv_monitor/models/ tests/unit/models/
git commit -m "feat: add all normalized data models"
```

---

### Task 7: DeviceDriver Protocol и Driver Registry

**Files:**
- Create: `src/cctv_monitor/core/interfaces.py`
- Create: `src/cctv_monitor/drivers/__init__.py`
- Create: `src/cctv_monitor/drivers/registry.py`
- Test: `tests/unit/drivers/__init__.py`
- Test: `tests/unit/drivers/test_registry.py`

**Step 1: Написать тест**

```python
# tests/unit/drivers/test_registry.py
import pytest
from unittest.mock import AsyncMock
from cctv_monitor.drivers.registry import DriverRegistry
from cctv_monitor.core.interfaces import DeviceDriver
from cctv_monitor.core.types import Vendor


class FakeDriver:
    """Fake driver implementing DeviceDriver protocol."""

    async def connect(self, config):
        pass

    async def disconnect(self):
        pass

    async def get_device_info(self):
        pass

    async def get_camera_statuses(self):
        return []

    async def get_disk_statuses(self):
        return []

    async def get_recording_statuses(self):
        return []

    async def get_snapshot(self, channel_id):
        pass

    async def check_health(self):
        pass

    async def detect_capabilities(self):
        pass


def test_register_and_get_driver():
    registry = DriverRegistry()
    registry.register(Vendor.HIKVISION, FakeDriver)
    driver_cls = registry.get(Vendor.HIKVISION)
    assert driver_cls is FakeDriver


def test_get_unknown_vendor_raises():
    registry = DriverRegistry()
    with pytest.raises(KeyError, match="dahua"):
        registry.get(Vendor.DAHUA)


def test_list_registered_vendors():
    registry = DriverRegistry()
    registry.register(Vendor.HIKVISION, FakeDriver)
    assert Vendor.HIKVISION in registry.vendors
```

**Step 2: Запустить тест — FAIL**

Run: `venv/Scripts/pytest tests/unit/drivers/test_registry.py -v`

**Step 3: Реализовать Protocol и Registry**

```python
# src/cctv_monitor/core/interfaces.py
from typing import Protocol
from cctv_monitor.models.device import DeviceConfig, DeviceInfo
from cctv_monitor.models.status import CameraStatus, DiskHealth, RecordingStatus
from cctv_monitor.models.snapshot import SnapshotResult
from cctv_monitor.models.device_health import DeviceHealthSummary
from cctv_monitor.models.capabilities import DeviceCapabilities


class DeviceDriver(Protocol):
    async def connect(self, config: DeviceConfig) -> None: ...
    async def disconnect(self) -> None: ...
    async def get_device_info(self) -> DeviceInfo: ...
    async def get_camera_statuses(self) -> list[CameraStatus]: ...
    async def get_disk_statuses(self) -> list[DiskHealth]: ...
    async def get_recording_statuses(self) -> list[RecordingStatus]: ...
    async def get_snapshot(self, channel_id: str) -> SnapshotResult: ...
    async def check_health(self) -> DeviceHealthSummary: ...
    async def detect_capabilities(self) -> DeviceCapabilities: ...
```

```python
# src/cctv_monitor/drivers/__init__.py
```

```python
# src/cctv_monitor/drivers/registry.py
from cctv_monitor.core.types import Vendor


class DriverRegistry:
    def __init__(self) -> None:
        self._drivers: dict[Vendor, type] = {}

    def register(self, vendor: Vendor, driver_cls: type) -> None:
        self._drivers[vendor] = driver_cls

    def get(self, vendor: Vendor) -> type:
        if vendor not in self._drivers:
            raise KeyError(f"No driver registered for vendor: {vendor}")
        return self._drivers[vendor]

    @property
    def vendors(self) -> list[Vendor]:
        return list(self._drivers.keys())
```

**Step 4: Запустить тест — PASS**

Run: `venv/Scripts/pytest tests/unit/drivers/test_registry.py -v`
Expected: 3 passed

**Step 5: Commit**

```bash
git add src/cctv_monitor/core/interfaces.py src/cctv_monitor/drivers/ tests/unit/drivers/
git commit -m "feat: add DeviceDriver protocol and driver registry"
```

---

### Task 8: Crypto Module (Fernet)

**Files:**
- Create: `src/cctv_monitor/core/crypto.py`
- Test: `tests/unit/core/test_crypto.py`

**Step 1: Написать тест**

```python
# tests/unit/core/test_crypto.py
from cryptography.fernet import Fernet
from cctv_monitor.core.crypto import encrypt_value, decrypt_value


def test_encrypt_decrypt_roundtrip():
    key = Fernet.generate_key().decode()
    original = "my_secret_password"
    encrypted = encrypt_value(original, key)
    assert encrypted != original
    decrypted = decrypt_value(encrypted, key)
    assert decrypted == original


def test_encrypted_value_is_different_each_time():
    key = Fernet.generate_key().decode()
    enc1 = encrypt_value("password", key)
    enc2 = encrypt_value("password", key)
    assert enc1 != enc2  # Fernet uses random IV
```

**Step 2: Запустить — FAIL**

Run: `venv/Scripts/pytest tests/unit/core/test_crypto.py -v`

**Step 3: Реализовать**

```python
# src/cctv_monitor/core/crypto.py
from cryptography.fernet import Fernet


def encrypt_value(plain_text: str, key: str) -> str:
    f = Fernet(key.encode())
    return f.encrypt(plain_text.encode()).decode()


def decrypt_value(encrypted_text: str, key: str) -> str:
    f = Fernet(key.encode())
    return f.decrypt(encrypted_text.encode()).decode()
```

**Step 4: Запустить — PASS**

Run: `venv/Scripts/pytest tests/unit/core/test_crypto.py -v`
Expected: 2 passed

**Step 5: Commit**

```bash
git add src/cctv_monitor/core/crypto.py tests/unit/core/test_crypto.py
git commit -m "feat: add Fernet encryption for device credentials"
```

---

### Task 9: Retry Policy

**Files:**
- Create: `src/cctv_monitor/core/retry.py`
- Test: `tests/unit/core/test_retry.py`

**Step 1: Написать тест**

```python
# tests/unit/core/test_retry.py
import pytest
from unittest.mock import AsyncMock
from cctv_monitor.core.retry import RetryPolicy, with_retry


@pytest.fixture
def policy():
    return RetryPolicy(max_retries=3, base_delay=0.01, max_delay=0.1)


async def test_no_retry_on_success(policy):
    func = AsyncMock(return_value="ok")
    result = await with_retry(func, policy)
    assert result == "ok"
    assert func.call_count == 1


async def test_retry_on_connection_error(policy):
    func = AsyncMock(side_effect=[ConnectionError, ConnectionError, "ok"])
    result = await with_retry(func, policy)
    assert result == "ok"
    assert func.call_count == 3


async def test_raises_after_max_retries(policy):
    func = AsyncMock(side_effect=ConnectionError("fail"))
    with pytest.raises(ConnectionError):
        await with_retry(func, policy)
    assert func.call_count == 4  # initial + 3 retries


async def test_no_retry_on_value_error(policy):
    func = AsyncMock(side_effect=ValueError("bad input"))
    with pytest.raises(ValueError):
        await with_retry(func, policy)
    assert func.call_count == 1
```

**Step 2: Запустить — FAIL**

Run: `venv/Scripts/pytest tests/unit/core/test_retry.py -v`

**Step 3: Реализовать**

```python
# src/cctv_monitor/core/retry.py
import asyncio
import random
from dataclasses import dataclass, field

import httpx


@dataclass
class RetryPolicy:
    max_retries: int = 3
    base_delay: float = 1.0
    max_delay: float = 30.0
    exponential_base: float = 2.0
    retry_on: tuple[type[Exception], ...] = field(default=(
        ConnectionError,
        TimeoutError,
        OSError,
        httpx.ConnectError,
        httpx.ReadTimeout,
        httpx.ConnectTimeout,
    ))

    def delay_for_attempt(self, attempt: int) -> float:
        delay = self.base_delay * (self.exponential_base ** attempt)
        delay = min(delay, self.max_delay)
        jitter = delay * 0.2 * (2 * random.random() - 1)  # noqa: S311
        return max(0, delay + jitter)


async def with_retry[T](
    func: ...,
    policy: RetryPolicy,
    *args,
    **kwargs,
) -> T:
    last_exception: Exception | None = None
    for attempt in range(policy.max_retries + 1):
        try:
            return await func(*args, **kwargs)
        except policy.retry_on as exc:
            last_exception = exc
            if attempt < policy.max_retries:
                delay = policy.delay_for_attempt(attempt)
                await asyncio.sleep(delay)
        except Exception:
            raise
    raise last_exception  # type: ignore[misc]
```

**Step 4: Запустить — PASS**

Run: `venv/Scripts/pytest tests/unit/core/test_retry.py -v`
Expected: 4 passed

**Step 5: Commit**

```bash
git add src/cctv_monitor/core/retry.py tests/unit/core/test_retry.py
git commit -m "feat: add retry policy with exponential backoff"
```

---

### Task 10: HTTP Client Manager

**Files:**
- Create: `src/cctv_monitor/core/http_client.py`
- Test: `tests/unit/core/test_http_client.py`

**Step 1: Написать тест**

```python
# tests/unit/core/test_http_client.py
import pytest
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
```

**Step 2: Запустить — FAIL**

Run: `venv/Scripts/pytest tests/unit/core/test_http_client.py -v`

**Step 3: Реализовать**

```python
# src/cctv_monitor/core/http_client.py
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
```

**Step 4: Запустить — PASS**

Run: `venv/Scripts/pytest tests/unit/core/test_http_client.py -v`
Expected: 3 passed

**Step 5: Commit**

```bash
git add src/cctv_monitor/core/http_client.py tests/unit/core/test_http_client.py
git commit -m "feat: add HTTP client manager with connection pooling"
```

---

## Phase 3: Database Layer

### Task 11: SQLAlchemy Tables

**Files:**
- Create: `src/cctv_monitor/storage/__init__.py`
- Create: `src/cctv_monitor/storage/database.py`
- Create: `src/cctv_monitor/storage/tables.py`

**Step 1: Реализовать database.py**

```python
# src/cctv_monitor/storage/__init__.py
```

```python
# src/cctv_monitor/storage/database.py
from sqlalchemy.ext.asyncio import (
    AsyncSession,
    async_sessionmaker,
    create_async_engine,
)


def create_engine(database_url: str):
    return create_async_engine(database_url, echo=False, pool_size=10)


def create_session_factory(engine) -> async_sessionmaker[AsyncSession]:
    return async_sessionmaker(engine, expire_on_commit=False)
```

**Step 2: Реализовать tables.py**

```python
# src/cctv_monitor/storage/tables.py
from datetime import datetime, timezone
from sqlalchemy import (
    Boolean, DateTime, Float, Integer, String, Text, ForeignKey, Index,
)
from sqlalchemy.dialects.postgresql import JSONB
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column


class Base(DeclarativeBase):
    pass


class PollingPolicyTable(Base):
    __tablename__ = "polling_policies"

    name: Mapped[str] = mapped_column(String(50), primary_key=True)
    device_info_interval: Mapped[int] = mapped_column(Integer, default=300)
    camera_status_interval: Mapped[int] = mapped_column(Integer, default=120)
    disk_status_interval: Mapped[int] = mapped_column(Integer, default=600)
    snapshot_interval: Mapped[int] = mapped_column(Integer, default=900)


class DeviceTable(Base):
    __tablename__ = "devices"

    device_id: Mapped[str] = mapped_column(String(100), primary_key=True)
    name: Mapped[str] = mapped_column(String(255))
    vendor: Mapped[str] = mapped_column(String(50))
    host: Mapped[str] = mapped_column(String(255))
    port: Mapped[int] = mapped_column(Integer, default=80)
    username: Mapped[str] = mapped_column(String(255))
    password_encrypted: Mapped[str] = mapped_column(Text)
    transport_mode: Mapped[str] = mapped_column(String(20), default="isapi")
    polling_policy_id: Mapped[str] = mapped_column(
        String(50), ForeignKey("polling_policies.name"), default="standard"
    )
    is_active: Mapped[bool] = mapped_column(Boolean, default=True)


class DeviceCapabilityTable(Base):
    __tablename__ = "device_capabilities"

    device_id: Mapped[str] = mapped_column(
        String(100), ForeignKey("devices.device_id"), primary_key=True
    )
    model: Mapped[str] = mapped_column(String(255), default="")
    firmware_version: Mapped[str] = mapped_column(String(255), default="")
    supports_isapi: Mapped[bool] = mapped_column(Boolean, default=False)
    supports_sdk: Mapped[bool] = mapped_column(Boolean, default=False)
    supports_snapshot: Mapped[bool] = mapped_column(Boolean, default=False)
    supports_recording_status: Mapped[bool] = mapped_column(Boolean, default=False)
    supports_disk_status: Mapped[bool] = mapped_column(Boolean, default=False)
    detected_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True),
        default=lambda: datetime.now(timezone.utc),
    )


class CheckResultTable(Base):
    __tablename__ = "check_results"
    __table_args__ = (
        Index("ix_check_results_device_type_time", "device_id", "check_type", "checked_at"),
    )

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    device_id: Mapped[str] = mapped_column(String(100), ForeignKey("devices.device_id"))
    check_type: Mapped[str] = mapped_column(String(50))
    success: Mapped[bool] = mapped_column(Boolean)
    error_type: Mapped[str | None] = mapped_column(String(100), nullable=True)
    duration_ms: Mapped[float] = mapped_column(Float)
    payload_json: Mapped[dict | None] = mapped_column(JSONB, nullable=True)
    checked_at: Mapped[datetime] = mapped_column(DateTime(timezone=True))


class SnapshotTable(Base):
    __tablename__ = "snapshots"

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    device_id: Mapped[str] = mapped_column(String(100), ForeignKey("devices.device_id"))
    channel_id: Mapped[str] = mapped_column(String(50))
    file_path: Mapped[str] = mapped_column(Text)
    file_size_bytes: Mapped[int] = mapped_column(Integer)
    checked_at: Mapped[datetime] = mapped_column(DateTime(timezone=True))


class AlertTable(Base):
    __tablename__ = "alerts"
    __table_args__ = (
        Index("ix_alerts_device_status", "device_id", "status"),
    )

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    device_id: Mapped[str] = mapped_column(String(100), ForeignKey("devices.device_id"))
    channel_id: Mapped[str | None] = mapped_column(String(50), nullable=True)
    alert_type: Mapped[str] = mapped_column(String(50))
    severity: Mapped[str] = mapped_column(String(20))
    message: Mapped[str] = mapped_column(Text)
    source: Mapped[str] = mapped_column(String(50))
    status: Mapped[str] = mapped_column(String(20), default="active")
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True))
    resolved_at: Mapped[datetime | None] = mapped_column(DateTime(timezone=True), nullable=True)
```

**Step 3: Commit**

```bash
git add src/cctv_monitor/storage/
git commit -m "feat: add SQLAlchemy table definitions and database setup"
```

---

### Task 12: Alembic Migrations

**Files:**
- Create: `alembic.ini`
- Create: `migrations/` (via alembic init)

**Step 1: Инициализировать Alembic**

Run: `venv/Scripts/alembic init migrations`

**Step 2: Настроить alembic.ini**

В `alembic.ini` заменить строку `sqlalchemy.url`:
```ini
sqlalchemy.url = postgresql://%(POSTGRES_USER)s:%(POSTGRES_PASSWORD)s@%(POSTGRES_HOST)s:%(POSTGRES_PORT)s/%(POSTGRES_DB)s
```

**Step 3: Настроить migrations/env.py**

В `migrations/env.py` добавить импорт моделей и target_metadata:

```python
# В начало файла
import os
import sys
sys.path.insert(0, os.path.join(os.path.dirname(__file__), "..", "src"))

from cctv_monitor.storage.tables import Base
target_metadata = Base.metadata
```

И в секцию `run_migrations_online()` добавить чтение env vars для config:

```python
from dotenv import load_dotenv
load_dotenv()

section = config.get_section(config.config_ini_section, {})
section["POSTGRES_USER"] = os.getenv("POSTGRES_USER", "cctv_admin")
section["POSTGRES_PASSWORD"] = os.getenv("POSTGRES_PASSWORD", "changeme")
section["POSTGRES_HOST"] = os.getenv("POSTGRES_HOST", "localhost")
section["POSTGRES_PORT"] = os.getenv("POSTGRES_PORT", "5432")
section["POSTGRES_DB"] = os.getenv("POSTGRES_DB", "cctv_monitoring")
```

**Step 4: Создать первую миграцию**

Run: `venv/Scripts/alembic revision --autogenerate -m "initial tables"`

**Step 5: Применить миграцию**

Run: `venv/Scripts/alembic upgrade head`
Expected: таблицы созданы в PostgreSQL

**Step 6: Commit**

```bash
git add alembic.ini migrations/
git commit -m "feat: add Alembic migrations with initial tables"
```

---

### Task 13: Repositories

**Files:**
- Create: `src/cctv_monitor/storage/repositories.py`
- Test: `tests/integration/__init__.py`
- Test: `tests/integration/test_repositories.py`

**Step 1: Реализовать repositories**

```python
# src/cctv_monitor/storage/repositories.py
from datetime import datetime, timezone
from sqlalchemy import select, update
from sqlalchemy.ext.asyncio import AsyncSession
from cctv_monitor.storage.tables import (
    DeviceTable, CheckResultTable, AlertTable, SnapshotTable,
    DeviceCapabilityTable, PollingPolicyTable,
)
from cctv_monitor.core.types import AlertStatus


class DeviceRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def get_active_devices(self) -> list[DeviceTable]:
        result = await self._session.execute(
            select(DeviceTable).where(DeviceTable.is_active.is_(True))
        )
        return list(result.scalars().all())

    async def get_by_id(self, device_id: str) -> DeviceTable | None:
        result = await self._session.execute(
            select(DeviceTable).where(DeviceTable.device_id == device_id)
        )
        return result.scalar_one_or_none()


class CheckResultRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def save(self, result: CheckResultTable) -> None:
        self._session.add(result)
        await self._session.flush()

    async def get_latest(self, device_id: str, check_type: str) -> CheckResultTable | None:
        result = await self._session.execute(
            select(CheckResultTable)
            .where(
                CheckResultTable.device_id == device_id,
                CheckResultTable.check_type == check_type,
            )
            .order_by(CheckResultTable.checked_at.desc())
            .limit(1)
        )
        return result.scalar_one_or_none()


class AlertRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def get_active_alerts(self, device_id: str) -> list[AlertTable]:
        result = await self._session.execute(
            select(AlertTable).where(
                AlertTable.device_id == device_id,
                AlertTable.status == AlertStatus.ACTIVE,
            )
        )
        return list(result.scalars().all())

    async def create_alert(self, alert: AlertTable) -> None:
        self._session.add(alert)
        await self._session.flush()

    async def resolve_alert(self, alert_id: int) -> None:
        await self._session.execute(
            update(AlertTable)
            .where(AlertTable.id == alert_id)
            .values(
                status=AlertStatus.RESOLVED,
                resolved_at=datetime.now(timezone.utc),
            )
        )


class SnapshotRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def save(self, record: SnapshotTable) -> None:
        self._session.add(record)
        await self._session.flush()
```

**Step 2: Написать интеграционный тест (запускается при наличии БД)**

```python
# tests/integration/test_repositories.py
"""Integration tests — require running PostgreSQL.
Run with: pytest tests/integration/ -v --run-integration
"""
import pytest

# Integration tests will be implemented when DB fixtures are ready.
# Placeholder to establish test structure.
```

**Step 3: Commit**

```bash
git add src/cctv_monitor/storage/repositories.py tests/integration/
git commit -m "feat: add database repositories for devices, checks, alerts, snapshots"
```

---

### Task 14: Snapshot File Store

**Files:**
- Create: `src/cctv_monitor/storage/snapshot_store.py`
- Test: `tests/unit/storage/__init__.py`
- Test: `tests/unit/storage/test_snapshot_store.py`

**Step 1: Написать тест**

```python
# tests/unit/storage/test_snapshot_store.py
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
```

**Step 2: Запустить — FAIL**

Run: `venv/Scripts/pytest tests/unit/storage/test_snapshot_store.py -v`

**Step 3: Реализовать**

```python
# src/cctv_monitor/storage/snapshot_store.py
import asyncio
from datetime import datetime, timezone
from pathlib import Path


class SnapshotStore:
    def __init__(self, base_dir: str) -> None:
        self._base_dir = Path(base_dir)

    async def save(
        self, device_id: str, channel_id: str, image_data: bytes
    ) -> str:
        now = datetime.now(timezone.utc)
        directory = self._base_dir / device_id / channel_id
        filename = now.strftime("%Y%m%d_%H%M%S") + ".jpg"
        file_path = directory / filename

        await asyncio.to_thread(self._write_file, file_path, image_data)
        return str(file_path)

    @staticmethod
    def _write_file(path: Path, data: bytes) -> None:
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(data)
```

**Step 4: Запустить — PASS**

Run: `venv/Scripts/pytest tests/unit/storage/test_snapshot_store.py -v`
Expected: 2 passed

**Step 5: Commit**

```bash
git add src/cctv_monitor/storage/snapshot_store.py tests/unit/storage/
git commit -m "feat: add file-based snapshot storage"
```

---

## Phase 4: Hikvision Driver

### Task 15: Hikvision Transport Base

**Files:**
- Create: `src/cctv_monitor/drivers/hikvision/__init__.py`
- Create: `src/cctv_monitor/drivers/hikvision/transports/__init__.py`
- Create: `src/cctv_monitor/drivers/hikvision/transports/base.py`
- Create: `src/cctv_monitor/drivers/hikvision/errors.py`

**Step 1: Реализовать transport ABC и errors**

```python
# src/cctv_monitor/drivers/hikvision/__init__.py
```

```python
# src/cctv_monitor/drivers/hikvision/transports/__init__.py
```

```python
# src/cctv_monitor/drivers/hikvision/transports/base.py
from abc import ABC, abstractmethod


class HikvisionTransport(ABC):
    @abstractmethod
    async def connect(self, host: str, port: int, username: str, password: str) -> None: ...

    @abstractmethod
    async def disconnect(self) -> None: ...

    @abstractmethod
    async def get_device_info(self) -> dict: ...

    @abstractmethod
    async def get_channels_status(self) -> list[dict]: ...

    @abstractmethod
    async def get_disk_status(self) -> list[dict]: ...

    @abstractmethod
    async def get_recording_status(self) -> list[dict]: ...

    @abstractmethod
    async def get_snapshot(self, channel_id: str) -> bytes: ...
```

```python
# src/cctv_monitor/drivers/hikvision/errors.py
from cctv_monitor.core.errors import DriverError, DeviceConnectionError


class HikvisionError(DriverError):
    """Base Hikvision-specific error."""


class IsapiError(HikvisionError):
    """ISAPI request failed."""

    def __init__(self, device_id: str, status_code: int, message: str = "") -> None:
        self.status_code = status_code
        super().__init__(device_id, f"ISAPI {status_code}: {message}")


class IsapiAuthError(IsapiError):
    """ISAPI authentication failed."""

    def __init__(self, device_id: str) -> None:
        super().__init__(device_id, 401, "Authentication failed")


class SdkError(HikvisionError):
    """SDK call failed."""

    def __init__(self, device_id: str, error_code: int, message: str = "") -> None:
        self.error_code = error_code
        super().__init__(device_id, f"SDK error {error_code}: {message}")
```

**Step 2: Commit**

```bash
git add src/cctv_monitor/drivers/hikvision/
git commit -m "feat: add Hikvision transport ABC and error types"
```

---

### Task 16: Hikvision ISAPI Mappers

**Files:**
- Create: `src/cctv_monitor/drivers/hikvision/mappers.py`
- Create: `tests/fixtures/hikvision/device_info.xml`
- Create: `tests/fixtures/hikvision/channels_status.xml`
- Create: `tests/fixtures/hikvision/hdd_status.xml`
- Test: `tests/unit/drivers/hikvision/__init__.py`
- Test: `tests/unit/drivers/hikvision/test_mappers.py`

**Step 1: Создать тестовые XML fixtures**

```xml
<!-- tests/fixtures/hikvision/device_info.xml -->
<?xml version="1.0" encoding="UTF-8"?>
<DeviceInfo xmlns="http://www.hikvision.com/ver20/XMLSchema">
    <deviceName>Office NVR</deviceName>
    <deviceID>nvr-01</deviceID>
    <model>DS-7608NI-K2</model>
    <serialNumber>DS-7608NI-K2202012345</serialNumber>
    <macAddress>aa:bb:cc:dd:ee:ff</macAddress>
    <firmwareVersion>V4.30.085 build 200916</firmwareVersion>
    <deviceType>NVR</deviceType>
</DeviceInfo>
```

```xml
<!-- tests/fixtures/hikvision/channels_status.xml -->
<?xml version="1.0" encoding="UTF-8"?>
<InputProxyChannelStatusList xmlns="http://www.hikvision.com/ver20/XMLSchema">
    <InputProxyChannelStatus>
        <id>1</id>
        <name>Front Door</name>
        <sourceInputPortDescriptor>
            <ipAddress>192.168.1.10</ipAddress>
            <managePortNo>8000</managePortNo>
        </sourceInputPortDescriptor>
        <online>true</online>
    </InputProxyChannelStatus>
    <InputProxyChannelStatus>
        <id>2</id>
        <name>Back Yard</name>
        <sourceInputPortDescriptor>
            <ipAddress>192.168.1.11</ipAddress>
            <managePortNo>8000</managePortNo>
        </sourceInputPortDescriptor>
        <online>false</online>
    </InputProxyChannelStatus>
</InputProxyChannelStatusList>
```

```xml
<!-- tests/fixtures/hikvision/hdd_status.xml -->
<?xml version="1.0" encoding="UTF-8"?>
<HDDList xmlns="http://www.hikvision.com/ver20/XMLSchema">
    <HDD>
        <id>1</id>
        <hddName>hdd1</hddName>
        <hddType>SATA</hddType>
        <status>ok</status>
        <capacity>2000</capacity>
        <freeSpace>500</freeSpace>
        <property>RW</property>
    </HDD>
</HDDList>
```

**Step 2: Написать тест**

```python
# tests/unit/drivers/hikvision/test_mappers.py
from pathlib import Path
from datetime import datetime, timezone
from cctv_monitor.drivers.hikvision.mappers import HikvisionMapper


FIXTURES = Path(__file__).parent.parent.parent.parent / "fixtures" / "hikvision"


def test_parse_device_info():
    xml = (FIXTURES / "device_info.xml").read_text()
    info = HikvisionMapper.parse_device_info(xml, device_id="nvr-01")
    assert info.model == "DS-7608NI-K2"
    assert info.serial_number == "DS-7608NI-K2202012345"
    assert info.firmware_version == "V4.30.085 build 200916"
    assert info.device_type == "NVR"
    assert info.mac_address == "aa:bb:cc:dd:ee:ff"


def test_parse_channels_status():
    xml = (FIXTURES / "channels_status.xml").read_text()
    now = datetime.now(timezone.utc)
    statuses = HikvisionMapper.parse_channels_status(xml, device_id="nvr-01", checked_at=now)
    assert len(statuses) == 2
    assert statuses[0].channel_name == "Front Door"
    assert statuses[0].is_online is True
    assert statuses[0].ip_address == "192.168.1.10"
    assert statuses[1].channel_name == "Back Yard"
    assert statuses[1].is_online is False


def test_parse_disk_status():
    xml = (FIXTURES / "hdd_status.xml").read_text()
    now = datetime.now(timezone.utc)
    disks = HikvisionMapper.parse_disk_status(xml, device_id="nvr-01", checked_at=now)
    assert len(disks) == 1
    assert disks[0].disk_id == "1"
    assert disks[0].capacity_bytes == 2000 * 1024 * 1024  # MB to bytes
    assert disks[0].free_bytes == 500 * 1024 * 1024
    assert disks[0].health_status == "ok"
```

**Step 3: Запустить — FAIL**

Run: `venv/Scripts/pytest tests/unit/drivers/hikvision/test_mappers.py -v`

**Step 4: Реализовать mappers**

```python
# src/cctv_monitor/drivers/hikvision/mappers.py
import xml.etree.ElementTree as ET
from datetime import datetime
from cctv_monitor.models.device import DeviceInfo
from cctv_monitor.models.status import CameraStatus, DiskHealth
from cctv_monitor.core.types import DiskStatus

NS = {"hik": "http://www.hikvision.com/ver20/XMLSchema"}


def _find_text(element: ET.Element, tag: str, default: str = "") -> str:
    node = element.find(f"hik:{tag}", NS)
    if node is None:
        node = element.find(tag)
    return node.text.strip() if node is not None and node.text else default


class HikvisionMapper:
    @staticmethod
    def parse_device_info(xml_text: str, device_id: str) -> DeviceInfo:
        root = ET.fromstring(xml_text)
        return DeviceInfo(
            device_id=device_id,
            model=_find_text(root, "model"),
            serial_number=_find_text(root, "serialNumber"),
            firmware_version=_find_text(root, "firmwareVersion"),
            device_type=_find_text(root, "deviceType"),
            mac_address=_find_text(root, "macAddress") or None,
            channels_count=0,  # populated from channels endpoint
        )

    @staticmethod
    def parse_channels_status(
        xml_text: str, device_id: str, checked_at: datetime
    ) -> list[CameraStatus]:
        root = ET.fromstring(xml_text)
        statuses = []
        for ch in root.findall("hik:InputProxyChannelStatus", NS):
            if not ch.findall("hik:InputProxyChannelStatus", NS):
                ip_addr = None
                src = ch.find("hik:sourceInputPortDescriptor", NS)
                if src is not None:
                    ip_addr = _find_text(src, "ipAddress") or None

                online_text = _find_text(ch, "online", "false")
                statuses.append(CameraStatus(
                    device_id=device_id,
                    channel_id=_find_text(ch, "id"),
                    channel_name=_find_text(ch, "name"),
                    is_online=online_text.lower() == "true",
                    ip_address=ip_addr,
                    protocol=None,
                    checked_at=checked_at,
                ))
        return statuses

    @staticmethod
    def parse_disk_status(
        xml_text: str, device_id: str, checked_at: datetime
    ) -> list[DiskHealth]:
        root = ET.fromstring(xml_text)
        disks = []
        for hdd in root.findall("hik:HDD", NS):
            status_text = _find_text(hdd, "status", "unknown")
            try:
                disk_status = DiskStatus(status_text.lower())
            except ValueError:
                disk_status = DiskStatus.UNKNOWN

            capacity_mb = int(_find_text(hdd, "capacity", "0"))
            free_mb = int(_find_text(hdd, "freeSpace", "0"))

            disks.append(DiskHealth(
                device_id=device_id,
                disk_id=_find_text(hdd, "id"),
                status=disk_status,
                capacity_bytes=capacity_mb * 1024 * 1024,
                free_bytes=free_mb * 1024 * 1024,
                health_status=status_text,
                checked_at=checked_at,
            ))
        return disks
```

**Step 5: Запустить — PASS**

Run: `venv/Scripts/pytest tests/unit/drivers/hikvision/test_mappers.py -v`
Expected: 3 passed

**Step 6: Commit**

```bash
git add src/cctv_monitor/drivers/hikvision/mappers.py tests/fixtures/ tests/unit/drivers/hikvision/
git commit -m "feat: add Hikvision ISAPI XML mappers with test fixtures"
```

---

### Task 17: ISAPI Transport

**Files:**
- Create: `src/cctv_monitor/drivers/hikvision/transports/isapi.py`
- Test: `tests/unit/drivers/hikvision/test_isapi_transport.py`

**Step 1: Написать тест с mocked httpx**

```python
# tests/unit/drivers/hikvision/test_isapi_transport.py
import pytest
from unittest.mock import AsyncMock, patch, MagicMock
from pathlib import Path
from cctv_monitor.drivers.hikvision.transports.isapi import IsapiTransport
from cctv_monitor.core.http_client import HttpClientManager

FIXTURES = Path(__file__).parent.parent.parent.parent / "fixtures" / "hikvision"


@pytest.fixture
def mock_client_manager():
    manager = AsyncMock(spec=HttpClientManager)
    mock_client = AsyncMock()
    manager.get_client.return_value = mock_client
    return manager, mock_client


async def test_get_device_info(mock_client_manager):
    manager, mock_client = mock_client_manager
    xml_response = (FIXTURES / "device_info.xml").read_text()

    mock_response = MagicMock()
    mock_response.status_code = 200
    mock_response.text = xml_response
    mock_response.raise_for_status = MagicMock()
    mock_client.get.return_value = mock_response

    transport = IsapiTransport(client_manager=manager)
    await transport.connect("192.168.1.100", 80, "admin", "password")
    result = await transport.get_device_info()

    assert "deviceName" in result["raw_xml"] or len(result["raw_xml"]) > 0
    mock_client.get.assert_called_once()


async def test_get_snapshot(mock_client_manager):
    manager, mock_client = mock_client_manager
    fake_jpeg = b"\xff\xd8\xff\xe0" + b"\x00" * 50

    mock_response = MagicMock()
    mock_response.status_code = 200
    mock_response.content = fake_jpeg
    mock_response.raise_for_status = MagicMock()
    mock_client.get.return_value = mock_response

    transport = IsapiTransport(client_manager=manager)
    await transport.connect("192.168.1.100", 80, "admin", "password")
    result = await transport.get_snapshot("101")

    assert result == fake_jpeg
```

**Step 2: Запустить — FAIL**

Run: `venv/Scripts/pytest tests/unit/drivers/hikvision/test_isapi_transport.py -v`

**Step 3: Реализовать**

```python
# src/cctv_monitor/drivers/hikvision/transports/isapi.py
import httpx
from cctv_monitor.drivers.hikvision.transports.base import HikvisionTransport
from cctv_monitor.drivers.hikvision.errors import IsapiError, IsapiAuthError
from cctv_monitor.core.http_client import HttpClientManager


class IsapiTransport(HikvisionTransport):
    DEVICE_INFO = "/ISAPI/System/deviceInfo"
    CHANNELS_STATUS = "/ISAPI/ContentMgmt/InputProxy/channels/status"
    HDD_STATUS = "/ISAPI/ContentMgmt/Storage/hdd"
    SNAPSHOT = "/ISAPI/Streaming/channels/{channel_id}/picture"
    RECORDING_STATUS = "/ISAPI/ContentMgmt/record/tracks"

    def __init__(self, client_manager: HttpClientManager) -> None:
        self._client_manager = client_manager
        self._base_url: str = ""
        self._auth: httpx.DigestAuth | None = None
        self._device_id: str = ""

    async def connect(self, host: str, port: int, username: str, password: str) -> None:
        scheme = "https" if port == 443 else "http"
        self._base_url = f"{scheme}://{host}:{port}"
        self._auth = httpx.DigestAuth(username, password)
        self._device_id = f"{host}:{port}"

    async def disconnect(self) -> None:
        self._auth = None

    async def _request(self, method: str, path: str, **kwargs) -> httpx.Response:
        client = await self._client_manager.get_client()
        url = f"{self._base_url}{path}"
        response = await client.request(method, url, auth=self._auth, **kwargs)
        if response.status_code == 401:
            raise IsapiAuthError(self._device_id)
        if response.status_code >= 400:
            raise IsapiError(self._device_id, response.status_code, response.text[:200])
        return response

    async def get_device_info(self) -> dict:
        response = await self._request("GET", self.DEVICE_INFO)
        return {"raw_xml": response.text}

    async def get_channels_status(self) -> list[dict]:
        response = await self._request("GET", self.CHANNELS_STATUS)
        return [{"raw_xml": response.text}]

    async def get_disk_status(self) -> list[dict]:
        response = await self._request("GET", self.HDD_STATUS)
        return [{"raw_xml": response.text}]

    async def get_recording_status(self) -> list[dict]:
        response = await self._request("GET", self.RECORDING_STATUS)
        return [{"raw_xml": response.text}]

    async def get_snapshot(self, channel_id: str) -> bytes:
        path = self.SNAPSHOT.format(channel_id=channel_id)
        response = await self._request("GET", path)
        return response.content
```

**Step 4: Запустить — PASS**

Run: `venv/Scripts/pytest tests/unit/drivers/hikvision/test_isapi_transport.py -v`
Expected: 2 passed

**Step 5: Commit**

```bash
git add src/cctv_monitor/drivers/hikvision/transports/isapi.py tests/unit/drivers/hikvision/test_isapi_transport.py
git commit -m "feat: add Hikvision ISAPI transport implementation"
```

---

### Task 18: SDK Transport Stub

**Files:**
- Create: `src/cctv_monitor/drivers/hikvision/transports/sdk.py`

**Step 1: Создать stub**

```python
# src/cctv_monitor/drivers/hikvision/transports/sdk.py
from cctv_monitor.drivers.hikvision.transports.base import HikvisionTransport
from cctv_monitor.drivers.hikvision.errors import SdkError


class SdkTransport(HikvisionTransport):
    """Stub SDK transport — not yet implemented.

    Will use ctypes to interface with HCNetSDK.
    See docs/vendors/hikvision/SDK_INTEGRATION_PLAN.md
    """

    async def connect(self, host: str, port: int, username: str, password: str) -> None:
        raise SdkError(
            device_id=f"{host}:{port}",
            error_code=-1,
            message="SDK transport not yet implemented",
        )

    async def disconnect(self) -> None:
        pass

    async def get_device_info(self) -> dict:
        raise NotImplementedError("SDK transport not yet implemented")

    async def get_channels_status(self) -> list[dict]:
        raise NotImplementedError("SDK transport not yet implemented")

    async def get_disk_status(self) -> list[dict]:
        raise NotImplementedError("SDK transport not yet implemented")

    async def get_recording_status(self) -> list[dict]:
        raise NotImplementedError("SDK transport not yet implemented")

    async def get_snapshot(self, channel_id: str) -> bytes:
        raise NotImplementedError("SDK transport not yet implemented")
```

**Step 2: Commit**

```bash
git add src/cctv_monitor/drivers/hikvision/transports/sdk.py
git commit -m "feat: add Hikvision SDK transport stub"
```

---

### Task 19: Hikvision Driver (основной)

**Files:**
- Create: `src/cctv_monitor/drivers/hikvision/driver.py`
- Test: `tests/unit/drivers/hikvision/test_driver.py`

**Step 1: Написать тест**

```python
# tests/unit/drivers/hikvision/test_driver.py
import pytest
from unittest.mock import AsyncMock, MagicMock
from pathlib import Path
from datetime import datetime, timezone
from cctv_monitor.drivers.hikvision.driver import HikvisionDriver
from cctv_monitor.models.device import DeviceConfig
from cctv_monitor.core.types import TransportMode, Vendor
from cctv_monitor.core.http_client import HttpClientManager

FIXTURES = Path(__file__).parent.parent.parent.parent / "fixtures" / "hikvision"


@pytest.fixture
def device_config():
    return DeviceConfig(
        device_id="nvr-01",
        name="Test NVR",
        vendor=Vendor.HIKVISION,
        host="192.168.1.100",
        port=80,
        username="admin",
        password="password",
        transport_mode=TransportMode.ISAPI,
        polling_policy_id="standard",
        is_active=True,
    )


@pytest.fixture
def mock_transport():
    transport = AsyncMock()
    transport.get_device_info.return_value = {
        "raw_xml": (FIXTURES / "device_info.xml").read_text()
    }
    transport.get_channels_status.return_value = [
        {"raw_xml": (FIXTURES / "channels_status.xml").read_text()}
    ]
    transport.get_disk_status.return_value = [
        {"raw_xml": (FIXTURES / "hdd_status.xml").read_text()}
    ]
    transport.get_snapshot.return_value = b"\xff\xd8\xff\xe0"
    return transport


async def test_get_device_info(device_config, mock_transport):
    driver = HikvisionDriver(transport=mock_transport)
    await driver.connect(device_config)
    info = await driver.get_device_info()
    assert info.model == "DS-7608NI-K2"
    assert info.device_id == "nvr-01"


async def test_get_camera_statuses(device_config, mock_transport):
    driver = HikvisionDriver(transport=mock_transport)
    await driver.connect(device_config)
    statuses = await driver.get_camera_statuses()
    assert len(statuses) == 2
    assert statuses[0].is_online is True
    assert statuses[1].is_online is False


async def test_get_disk_statuses(device_config, mock_transport):
    driver = HikvisionDriver(transport=mock_transport)
    await driver.connect(device_config)
    disks = await driver.get_disk_statuses()
    assert len(disks) == 1
    assert disks[0].health_status == "ok"


async def test_get_snapshot(device_config, mock_transport):
    driver = HikvisionDriver(transport=mock_transport)
    await driver.connect(device_config)
    result = await driver.get_snapshot("101")
    assert result.success is True
    assert result.device_id == "nvr-01"
```

**Step 2: Запустить — FAIL**

Run: `venv/Scripts/pytest tests/unit/drivers/hikvision/test_driver.py -v`

**Step 3: Реализовать**

```python
# src/cctv_monitor/drivers/hikvision/driver.py
from datetime import datetime, timezone
from cctv_monitor.models.device import DeviceConfig, DeviceInfo
from cctv_monitor.models.status import CameraStatus, DiskHealth, RecordingStatus
from cctv_monitor.models.snapshot import SnapshotResult
from cctv_monitor.models.device_health import DeviceHealthSummary
from cctv_monitor.models.capabilities import DeviceCapabilities
from cctv_monitor.drivers.hikvision.transports.base import HikvisionTransport
from cctv_monitor.drivers.hikvision.mappers import HikvisionMapper
from cctv_monitor.core.types import DiskStatus

import structlog

logger = structlog.get_logger()


class HikvisionDriver:
    def __init__(self, transport: HikvisionTransport) -> None:
        self._transport = transport
        self._config: DeviceConfig | None = None

    @property
    def device_id(self) -> str:
        return self._config.device_id if self._config else "unknown"

    async def connect(self, config: DeviceConfig) -> None:
        self._config = config
        await self._transport.connect(
            host=config.host,
            port=config.port,
            username=config.username,
            password=config.password,
        )

    async def disconnect(self) -> None:
        await self._transport.disconnect()

    async def get_device_info(self) -> DeviceInfo:
        raw = await self._transport.get_device_info()
        return HikvisionMapper.parse_device_info(raw["raw_xml"], self.device_id)

    async def get_camera_statuses(self) -> list[CameraStatus]:
        raw_list = await self._transport.get_channels_status()
        now = datetime.now(timezone.utc)
        all_statuses = []
        for raw in raw_list:
            all_statuses.extend(
                HikvisionMapper.parse_channels_status(raw["raw_xml"], self.device_id, now)
            )
        return all_statuses

    async def get_disk_statuses(self) -> list[DiskHealth]:
        raw_list = await self._transport.get_disk_status()
        now = datetime.now(timezone.utc)
        all_disks = []
        for raw in raw_list:
            all_disks.extend(
                HikvisionMapper.parse_disk_status(raw["raw_xml"], self.device_id, now)
            )
        return all_disks

    async def get_recording_statuses(self) -> list[RecordingStatus]:
        # TODO: implement when recording endpoint is verified
        return []

    async def get_snapshot(self, channel_id: str) -> SnapshotResult:
        now = datetime.now(timezone.utc)
        try:
            image_data = await self._transport.get_snapshot(channel_id)
            return SnapshotResult(
                device_id=self.device_id,
                channel_id=channel_id,
                success=True,
                file_path=None,  # set by caller after saving
                file_size_bytes=len(image_data),
                error=None,
                checked_at=now,
            )
        except Exception as exc:
            return SnapshotResult(
                device_id=self.device_id,
                channel_id=channel_id,
                success=False,
                file_path=None,
                file_size_bytes=None,
                error=str(exc),
                checked_at=now,
            )

    async def check_health(self) -> DeviceHealthSummary:
        import time
        now = datetime.now(timezone.utc)
        start = time.monotonic()

        try:
            await self.get_device_info()
            reachable = True
        except Exception:
            reachable = False

        response_time = (time.monotonic() - start) * 1000

        cameras = []
        disks = []
        if reachable:
            try:
                cameras = await self.get_camera_statuses()
            except Exception:
                pass
            try:
                disks = await self.get_disk_statuses()
            except Exception:
                pass

        online = sum(1 for c in cameras if c.is_online)
        disk_ok = all(d.status == DiskStatus.OK for d in disks) if disks else True

        return DeviceHealthSummary(
            device_id=self.device_id,
            reachable=reachable,
            camera_count=len(cameras),
            online_cameras=online,
            offline_cameras=len(cameras) - online,
            disk_ok=disk_ok,
            recording_ok=True,  # TODO: check when recording endpoint is ready
            response_time_ms=response_time,
            checked_at=now,
        )

    async def detect_capabilities(self) -> DeviceCapabilities:
        now = datetime.now(timezone.utc)
        model = ""
        firmware = ""
        supports_snapshot = False
        supports_disk = False
        supports_recording = False

        try:
            info = await self.get_device_info()
            model = info.model
            firmware = info.firmware_version
        except Exception:
            pass

        try:
            await self.get_disk_statuses()
            supports_disk = True
        except Exception:
            pass

        try:
            await self.get_snapshot("101")
            supports_snapshot = True
        except Exception:
            pass

        return DeviceCapabilities(
            device_id=self.device_id,
            model=model,
            firmware_version=firmware,
            supports_isapi=True,  # if we got here via ISAPI
            supports_sdk=False,   # unknown at this point
            supports_snapshot=supports_snapshot,
            supports_recording_status=supports_recording,
            supports_disk_status=supports_disk,
            detected_at=now,
        )
```

**Step 4: Запустить — PASS**

Run: `venv/Scripts/pytest tests/unit/drivers/hikvision/test_driver.py -v`
Expected: 4 passed

**Step 5: Commit**

```bash
git add src/cctv_monitor/drivers/hikvision/driver.py tests/unit/drivers/hikvision/test_driver.py
git commit -m "feat: add HikvisionDriver with ISAPI integration"
```

---

### Task 20: Vendor Stubs (Dahua, Provision)

**Files:**
- Create: `src/cctv_monitor/drivers/dahua/__init__.py`
- Create: `src/cctv_monitor/drivers/provision/__init__.py`

**Step 1: Создать stubs**

```python
# src/cctv_monitor/drivers/dahua/__init__.py
"""Dahua driver — not yet implemented."""
```

```python
# src/cctv_monitor/drivers/provision/__init__.py
"""Provision ISR driver — not yet implemented."""
```

**Step 2: Commit**

```bash
git add src/cctv_monitor/drivers/dahua/ src/cctv_monitor/drivers/provision/
git commit -m "feat: add Dahua and Provision driver stubs"
```

---

## Phase 5: Polling & Alerts

### Task 21: Device Seed Loader

**Files:**
- Create: `devices.seed.yaml`
- Create: `src/cctv_monitor/storage/seed.py`
- Test: `tests/unit/storage/test_seed.py`

**Step 1: Создать seed YAML**

```yaml
# devices.seed.yaml
polling_policies:
  - name: light
    device_info_interval: 600
    camera_status_interval: 300
    disk_status_interval: 900
    snapshot_interval: 1800

  - name: standard
    device_info_interval: 300
    camera_status_interval: 120
    disk_status_interval: 600
    snapshot_interval: 900

  - name: critical
    device_info_interval: 120
    camera_status_interval: 60
    disk_status_interval: 300
    snapshot_interval: 300

devices:
  - device_id: nvr-demo-01
    name: "Demo NVR"
    vendor: hikvision
    host: "192.168.1.100"
    port: 80
    username: admin
    password: "changeme"
    transport_mode: isapi
    polling_policy_id: standard
```

**Step 2: Написать тест**

```python
# tests/unit/storage/test_seed.py
from pathlib import Path
from cctv_monitor.storage.seed import parse_seed_file


def test_parse_seed_file(tmp_path):
    seed_content = """
polling_policies:
  - name: standard
    device_info_interval: 300
    camera_status_interval: 120
    disk_status_interval: 600
    snapshot_interval: 900

devices:
  - device_id: test-01
    name: Test Device
    vendor: hikvision
    host: "10.0.0.1"
    port: 80
    username: admin
    password: "test123"
    transport_mode: isapi
    polling_policy_id: standard
"""
    seed_file = tmp_path / "seed.yaml"
    seed_file.write_text(seed_content)
    result = parse_seed_file(str(seed_file))
    assert len(result["policies"]) == 1
    assert result["policies"][0]["name"] == "standard"
    assert len(result["devices"]) == 1
    assert result["devices"][0]["device_id"] == "test-01"
```

**Step 3: Запустить — FAIL**

Run: `venv/Scripts/pytest tests/unit/storage/test_seed.py -v`

**Step 4: Реализовать**

```python
# src/cctv_monitor/storage/seed.py
from pathlib import Path
import yaml


def parse_seed_file(path: str) -> dict:
    content = Path(path).read_text(encoding="utf-8")
    data = yaml.safe_load(content)
    return {
        "policies": data.get("polling_policies", []),
        "devices": data.get("devices", []),
    }
```

**Step 5: Запустить — PASS**

Run: `venv/Scripts/pytest tests/unit/storage/test_seed.py -v`
Expected: 1 passed

**Step 6: Commit**

```bash
git add devices.seed.yaml src/cctv_monitor/storage/seed.py tests/unit/storage/test_seed.py
git commit -m "feat: add device seed loader from YAML"
```

---

### Task 22: Alert Engine с дедупликацией

**Files:**
- Create: `src/cctv_monitor/alerts/__init__.py`
- Create: `src/cctv_monitor/alerts/engine.py`
- Create: `src/cctv_monitor/alerts/rules.py`
- Test: `tests/unit/alerts/__init__.py`
- Test: `tests/unit/alerts/test_engine.py`

**Step 1: Написать тест**

```python
# tests/unit/alerts/test_engine.py
import pytest
from datetime import datetime, timezone
from cctv_monitor.alerts.engine import AlertEngine
from cctv_monitor.models.device_health import DeviceHealthSummary
from cctv_monitor.models.alert import AlertEvent
from cctv_monitor.core.types import AlertType, Severity, AlertStatus


@pytest.fixture
def engine():
    return AlertEngine()


def _make_health(
    reachable=True, online=4, offline=0, disk_ok=True, recording_ok=True
) -> DeviceHealthSummary:
    return DeviceHealthSummary(
        device_id="nvr-01",
        reachable=reachable,
        camera_count=online + offline,
        online_cameras=online,
        offline_cameras=offline,
        disk_ok=disk_ok,
        recording_ok=recording_ok,
        response_time_ms=100.0,
        checked_at=datetime.now(timezone.utc),
    )


def test_no_alerts_when_healthy(engine):
    health = _make_health()
    new_alerts, resolved = engine.evaluate(health, active_alerts=[])
    assert len(new_alerts) == 0
    assert len(resolved) == 0


def test_alert_on_device_unreachable(engine):
    health = _make_health(reachable=False)
    new_alerts, resolved = engine.evaluate(health, active_alerts=[])
    assert len(new_alerts) == 1
    assert new_alerts[0].alert_type == AlertType.DEVICE_UNREACHABLE


def test_alert_on_camera_offline(engine):
    health = _make_health(offline=2)
    new_alerts, resolved = engine.evaluate(health, active_alerts=[])
    assert any(a.alert_type == AlertType.CAMERA_OFFLINE for a in new_alerts)


def test_no_duplicate_alert(engine):
    health = _make_health(reachable=False)
    existing = AlertEvent(
        id=1,
        device_id="nvr-01",
        alert_type=AlertType.DEVICE_UNREACHABLE,
        severity=Severity.CRITICAL,
        message="Device unreachable",
        source="polling",
        status=AlertStatus.ACTIVE,
        created_at=datetime.now(timezone.utc),
    )
    new_alerts, resolved = engine.evaluate(health, active_alerts=[existing])
    assert len(new_alerts) == 0
    assert len(resolved) == 0


def test_resolve_alert_when_recovered(engine):
    health = _make_health(reachable=True)
    existing = AlertEvent(
        id=1,
        device_id="nvr-01",
        alert_type=AlertType.DEVICE_UNREACHABLE,
        severity=Severity.CRITICAL,
        message="Device unreachable",
        source="polling",
        status=AlertStatus.ACTIVE,
        created_at=datetime.now(timezone.utc),
    )
    new_alerts, resolved = engine.evaluate(health, active_alerts=[existing])
    assert len(resolved) == 1
    assert resolved[0].id == 1


def test_disk_error_alert(engine):
    health = _make_health(disk_ok=False)
    new_alerts, resolved = engine.evaluate(health, active_alerts=[])
    assert any(a.alert_type == AlertType.DISK_ERROR for a in new_alerts)
```

**Step 2: Запустить — FAIL**

Run: `venv/Scripts/pytest tests/unit/alerts/test_engine.py -v`

**Step 3: Реализовать**

```python
# src/cctv_monitor/alerts/__init__.py
```

```python
# src/cctv_monitor/alerts/rules.py
from cctv_monitor.models.device_health import DeviceHealthSummary
from cctv_monitor.core.types import AlertType, Severity


def check_device_unreachable(health: DeviceHealthSummary) -> tuple[AlertType, Severity, str] | None:
    if not health.reachable:
        return (
            AlertType.DEVICE_UNREACHABLE,
            Severity.CRITICAL,
            f"Device {health.device_id} is unreachable",
        )
    return None


def check_camera_offline(health: DeviceHealthSummary) -> tuple[AlertType, Severity, str] | None:
    if health.offline_cameras > 0:
        return (
            AlertType.CAMERA_OFFLINE,
            Severity.WARNING,
            f"{health.offline_cameras} camera(s) offline on {health.device_id}",
        )
    return None


def check_disk_error(health: DeviceHealthSummary) -> tuple[AlertType, Severity, str] | None:
    if not health.disk_ok:
        return (
            AlertType.DISK_ERROR,
            Severity.CRITICAL,
            f"Disk error on {health.device_id}",
        )
    return None


ALL_RULES = [
    check_device_unreachable,
    check_camera_offline,
    check_disk_error,
]
```

```python
# src/cctv_monitor/alerts/engine.py
from datetime import datetime, timezone
from cctv_monitor.models.device_health import DeviceHealthSummary
from cctv_monitor.models.alert import AlertEvent
from cctv_monitor.core.types import AlertStatus, Severity
from cctv_monitor.alerts.rules import ALL_RULES


class AlertEngine:
    def evaluate(
        self,
        health: DeviceHealthSummary,
        active_alerts: list[AlertEvent],
    ) -> tuple[list[AlertEvent], list[AlertEvent]]:
        now = datetime.now(timezone.utc)
        active_types = {a.alert_type for a in active_alerts}
        triggered_types = set()
        new_alerts: list[AlertEvent] = []

        for rule in ALL_RULES:
            result = rule(health)
            if result is not None:
                alert_type, severity, message = result
                triggered_types.add(alert_type)
                if alert_type not in active_types:
                    new_alerts.append(AlertEvent(
                        device_id=health.device_id,
                        alert_type=alert_type,
                        severity=severity,
                        message=message,
                        source="polling",
                        status=AlertStatus.ACTIVE,
                        created_at=now,
                    ))

        resolved = [
            a for a in active_alerts
            if a.alert_type not in triggered_types
        ]

        return new_alerts, resolved
```

**Step 4: Запустить — PASS**

Run: `venv/Scripts/pytest tests/unit/alerts/test_engine.py -v`
Expected: 6 passed

**Step 5: Commit**

```bash
git add src/cctv_monitor/alerts/ tests/unit/alerts/
git commit -m "feat: add alert engine with state-based deduplication"
```

---

### Task 23: Metrics Collector

**Files:**
- Create: `src/cctv_monitor/metrics/__init__.py`
- Create: `src/cctv_monitor/metrics/collector.py`
- Test: `tests/unit/metrics/__init__.py`
- Test: `tests/unit/metrics/test_collector.py`

**Step 1: Написать тест**

```python
# tests/unit/metrics/test_collector.py
from cctv_monitor.metrics.collector import MetricsCollector


def test_record_and_get_summary():
    collector = MetricsCollector()
    collector.record_poll_result("nvr-01", "device_info", success=True)
    collector.record_poll_result("nvr-01", "device_info", success=True)
    collector.record_poll_result("nvr-01", "device_info", success=False)
    summary = collector.get_summary()
    assert summary["total_polls"] == 3
    assert summary["successful_polls"] == 2
    assert summary["failed_polls"] == 1


def test_record_response_time():
    collector = MetricsCollector()
    collector.record_device_response_time("nvr-01", 150.0)
    collector.record_device_response_time("nvr-01", 250.0)
    summary = collector.get_summary()
    assert summary["devices"]["nvr-01"]["avg_response_ms"] == 200.0
```

**Step 2: Запустить — FAIL**

Run: `venv/Scripts/pytest tests/unit/metrics/test_collector.py -v`

**Step 3: Реализовать**

```python
# src/cctv_monitor/metrics/__init__.py
```

```python
# src/cctv_monitor/metrics/collector.py
from collections import defaultdict

import structlog

logger = structlog.get_logger()


class MetricsCollector:
    def __init__(self) -> None:
        self._total_polls = 0
        self._successful_polls = 0
        self._failed_polls = 0
        self._response_times: dict[str, list[float]] = defaultdict(list)

    def record_poll_result(self, device_id: str, check_type: str, success: bool) -> None:
        self._total_polls += 1
        if success:
            self._successful_polls += 1
        else:
            self._failed_polls += 1
        logger.debug(
            "poll.result",
            device_id=device_id,
            check_type=check_type,
            success=success,
        )

    def record_poll_duration(self, device_id: str, duration_ms: float) -> None:
        logger.debug("poll.duration", device_id=device_id, duration_ms=duration_ms)

    def record_device_response_time(self, device_id: str, ms: float) -> None:
        self._response_times[device_id].append(ms)
        logger.debug("device.response_time", device_id=device_id, response_time_ms=ms)

    def get_summary(self) -> dict:
        devices = {}
        for device_id, times in self._response_times.items():
            devices[device_id] = {
                "avg_response_ms": sum(times) / len(times) if times else 0,
                "poll_count": len(times),
            }
        return {
            "total_polls": self._total_polls,
            "successful_polls": self._successful_polls,
            "failed_polls": self._failed_polls,
            "devices": devices,
        }
```

**Step 4: Запустить — PASS**

Run: `venv/Scripts/pytest tests/unit/metrics/test_collector.py -v`
Expected: 2 passed

**Step 5: Commit**

```bash
git add src/cctv_monitor/metrics/ tests/unit/metrics/
git commit -m "feat: add metrics collector with structured logging"
```

---

### Task 24: Polling Jobs и Scheduler

**Files:**
- Create: `src/cctv_monitor/polling/__init__.py`
- Create: `src/cctv_monitor/polling/scheduler.py`
- Create: `src/cctv_monitor/polling/jobs.py`

**Step 1: Реализовать polling jobs**

```python
# src/cctv_monitor/polling/__init__.py
```

```python
# src/cctv_monitor/polling/jobs.py
import time
from datetime import datetime, timezone

import structlog

from cctv_monitor.core.interfaces import DeviceDriver
from cctv_monitor.models.device import DeviceConfig
from cctv_monitor.models.check_result import DeviceCheckResult
from cctv_monitor.core.types import CheckType
from cctv_monitor.metrics.collector import MetricsCollector

logger = structlog.get_logger()


async def poll_device_health(
    driver: DeviceDriver,
    config: DeviceConfig,
    metrics: MetricsCollector,
) -> DeviceCheckResult:
    start = time.monotonic()
    now = datetime.now(timezone.utc)
    try:
        await driver.connect(config)
        health = await driver.check_health()
        duration = (time.monotonic() - start) * 1000

        metrics.record_poll_result(config.device_id, "health_check", success=True)
        metrics.record_device_response_time(config.device_id, health.response_time_ms)
        metrics.record_poll_duration(config.device_id, duration)

        logger.info(
            "poll.health.ok",
            device_id=config.device_id,
            reachable=health.reachable,
            cameras=health.camera_count,
            online=health.online_cameras,
            duration_ms=round(duration, 1),
        )

        return DeviceCheckResult(
            device_id=config.device_id,
            check_type=CheckType.DEVICE_INFO,
            success=True,
            duration_ms=duration,
            checked_at=now,
        )
    except Exception as exc:
        duration = (time.monotonic() - start) * 1000
        metrics.record_poll_result(config.device_id, "health_check", success=False)

        logger.error(
            "poll.health.error",
            device_id=config.device_id,
            error=str(exc),
            duration_ms=round(duration, 1),
        )

        return DeviceCheckResult(
            device_id=config.device_id,
            check_type=CheckType.DEVICE_INFO,
            success=False,
            error_type=type(exc).__name__,
            duration_ms=duration,
            checked_at=now,
        )
    finally:
        try:
            await driver.disconnect()
        except Exception:
            pass
```

```python
# src/cctv_monitor/polling/scheduler.py
import structlog
from apscheduler.schedulers.asyncio import AsyncIOScheduler

logger = structlog.get_logger()


def create_scheduler() -> AsyncIOScheduler:
    scheduler = AsyncIOScheduler(
        job_defaults={
            "coalesce": True,
            "max_instances": 1,
            "misfire_grace_time": 60,
        }
    )
    return scheduler
```

**Step 2: Commit**

```bash
git add src/cctv_monitor/polling/
git commit -m "feat: add polling jobs and APScheduler setup"
```

---

## Phase 6: API & Integration

### Task 25: Minimal FastAPI

**Files:**
- Create: `src/cctv_monitor/api/__init__.py`
- Create: `src/cctv_monitor/api/app.py`
- Test: `tests/unit/api/__init__.py`
- Test: `tests/unit/api/test_app.py`

**Step 1: Написать тест**

```python
# tests/unit/api/test_app.py
import pytest
from httpx import AsyncClient, ASGITransport
from cctv_monitor.api.app import create_app


async def test_health_endpoint():
    app = create_app()
    transport = ASGITransport(app=app)
    async with AsyncClient(transport=transport, base_url="http://test") as client:
        response = await client.get("/health")
    assert response.status_code == 200
    assert response.json()["status"] == "ok"
```

**Step 2: Запустить — FAIL**

Run: `venv/Scripts/pytest tests/unit/api/test_app.py -v`

**Step 3: Реализовать**

```python
# src/cctv_monitor/api/__init__.py
```

```python
# src/cctv_monitor/api/app.py
from fastapi import FastAPI


def create_app() -> FastAPI:
    app = FastAPI(title="CCTV Monitor", version="0.1.0")

    @app.get("/health")
    async def health():
        return {"status": "ok", "service": "cctv-monitor"}

    return app
```

**Step 4: Запустить — PASS**

Run: `venv/Scripts/pytest tests/unit/api/test_app.py -v`
Expected: 1 passed

**Step 5: Commit**

```bash
git add src/cctv_monitor/api/ tests/unit/api/
git commit -m "feat: add minimal FastAPI with health endpoint"
```

---

### Task 26: Финальная сборка main.py

**Files:**
- Modify: `src/cctv_monitor/main.py`

**Step 1: Обновить main.py**

```python
# src/cctv_monitor/main.py
"""Application entry point."""

import asyncio

import structlog
import uvicorn

from cctv_monitor.api.app import create_app
from cctv_monitor.core.config import Settings
from cctv_monitor.core.http_client import HttpClientManager
from cctv_monitor.drivers.registry import DriverRegistry
from cctv_monitor.drivers.hikvision.driver import HikvisionDriver
from cctv_monitor.drivers.hikvision.transports.isapi import IsapiTransport
from cctv_monitor.core.types import Vendor
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

    def make_hikvision_driver():
        transport = IsapiTransport(client_manager=http_client)
        return HikvisionDriver(transport=transport)

    registry.register(Vendor.HIKVISION, make_hikvision_driver)

    # Scheduler
    scheduler = create_scheduler()
    scheduler.start()

    # API
    app = create_app()

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

**Step 2: Commit**

```bash
git add src/cctv_monitor/main.py
git commit -m "feat: wire up main.py with all components"
```

---

### Task 27: Run All Tests

**Step 1: Запустить полный набор тестов**

Run: `venv/Scripts/pytest tests/ -v --tb=short`
Expected: все тесты проходят

**Step 2: Запустить linter**

Run: `venv/Scripts/ruff check src/ tests/`
Expected: no issues found (или исправить найденные)

**Step 3: Commit если были исправления**

```bash
git add -A
git commit -m "fix: resolve linter issues"
```

---

## Итог

После выполнения всех 27 задач в проекте будут:

| Компонент | Статус |
|-----------|--------|
| Python project с зависимостями | готов |
| Docker Compose PostgreSQL | готов |
| Core (errors, types, config, crypto, retry, http_client) | готов |
| Все normalized models | готовы |
| SQLAlchemy tables + Alembic | готов |
| Repositories | готовы |
| Snapshot file store | готов |
| DeviceDriver Protocol + Registry | готов |
| Hikvision ISAPI Transport | готов |
| Hikvision SDK Transport | stub |
| Hikvision Driver | готов |
| Hikvision XML Mappers | готовы с тест-fixtures |
| Alert Engine с дедупликацией | готов |
| Metrics Collector | готов |
| Polling Jobs + Scheduler | готов |
| FastAPI (health endpoint) | готов |
| main.py (wiring) | готов |
| Unit tests | готовы |
| Dahua / Provision | stubs |
