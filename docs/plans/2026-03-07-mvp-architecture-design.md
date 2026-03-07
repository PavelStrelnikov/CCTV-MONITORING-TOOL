# CCTV Monitoring Platform — MVP Architecture Design

> Статус: утверждён
> Дата: 2026-03-07
> Автор: совместная проработка

---

## 1. Назначение системы

Специализированная платформа мониторинга состояния CCTV-инфраструктуры для сервисной компании.

Система **не является** VMS, CRM или ticketing system.

### Что мониторится

- Доступность NVR / DVR
- Статус камер (online / offline)
- Статус записи
- Состояние дисков
- Контрольные снимки (snapshot)
- События тревог

### Целевой масштаб MVP

50-200 устройств, один оператор/компания.

---

## 2. Принятые технологические решения

| Параметр | Решение | Обоснование |
|----------|---------|-------------|
| Язык | Python | Быстрый MVP, asyncio, httpx, ctypes для SDK |
| Архитектура | Модульный монолит | Простота, без overengineering |
| БД | PostgreSQL 16 | JSONB, concurrent writes, time-series ready |
| Инфраструктура БД | Docker Compose (`cctv_monitoring_postgres`) | Zero-config dev setup |
| Структура | Flat module (`cctv_monitor/`) | Читаемо, навигируемо |
| Driver abstraction | Python Protocol / ABC | Вендоронезависимое ядро |
| Transport selection | Config-based (`isapi` / `sdk` / `auto`) | ISAPI first, SDK fallback |
| Конфигурация устройств | PostgreSQL + seed из YAML | Production в БД, dev из файла |
| Credentials | Fernet (cryptography), ключ из env | Безопасно для MVP |
| Scheduling | APScheduler | Job persistence, без отдельного брокера |
| HTTP client | httpx.AsyncClient с connection pooling | Переиспользование соединений |
| Retry | Exponential backoff, network errors only | Устойчивость к transient errors |
| Snapshots | Файловая система, метаданные в БД | Не раздувать PostgreSQL |
| Метрики | Structured logging (MVP), Prometheus позже | Минимальные зависимости |

---

## 3. Архитектура

```
┌─────────────────────────────────────────────────────────┐
│                   CCTV Monitor                          │
│                                                         │
│  ┌────────────┐  ┌────────────┐  ┌──────────────────┐   │
│  │ Polling    │  │ Alert      │  │ API (FastAPI)    │   │
│  │ Scheduler  │  │ Engine     │  │ health, status   │   │
│  │ + Policies │  │ + Dedup    │  └──────────────────┘   │
│  └─────┬──────┘  └─────┬──────┘                         │
│        │               │                                │
│  ┌─────▼───────────────▼─────────────────────────────┐  │
│  │                 Core Layer                         │  │
│  │  DeviceDriver Protocol    Normalized Models        │  │
│  │  Unified Errors           RetryPolicy              │  │
│  │  HttpClientManager        Crypto                   │  │
│  └─────┬─────────────────────────────────────────┬───┘  │
│        │                                         │      │
│  ┌─────▼───────────┐                  ┌──────────▼───┐  │
│  │ Storage         │                  │ Drivers      │  │
│  │ ├─ Repositories │                  │              │  │
│  │ ├─ CheckResults │                  │ Hikvision    │  │
│  │ ├─ Snapshots(fs)│                  │  ├─ ISAPI    │  │
│  │ └─ Migrations   │                  │  └─ SDK      │  │
│  └─────────────────┘                  │ Dahua (stub) │  │
│                                       │ Provision(s) │  │
│  ┌─────────────────┐                  └──────────────┘  │
│  │ Metrics         │                                    │
│  │ Collector       │──► structured logs                 │
│  └─────────────────┘                                    │
└─────────────────────────────────────────────────────────┘
```

### Поток данных

1. APScheduler запускает polling job по PollingPolicy
2. Job загружает DeviceConfig из БД, определяет драйвер по vendor
3. Драйвер подключается через выбранный transport (ISAPI / SDK / auto)
4. RetryPolicy оборачивает каждый запрос
5. Ответ маппится в normalized model через mappers
6. DeviceCheckResult сохраняется в БД (история)
7. DeviceHealthSummary формируется из результатов
8. Alert Engine сравнивает текущее состояние с активными алертами (deduplication)
9. MetricsCollector записывает poll_duration, response_time, success/error

---

## 4. Структура проекта

```
cctv-monitoring/
├── docker-compose.yml
├── .env.example
├── pyproject.toml
├── alembic.ini
├── devices.seed.yaml
│
├── src/
│   └── cctv_monitor/
│       ├── __init__.py
│       ├── main.py
│       │
│       ├── core/
│       │   ├── interfaces.py            # DeviceDriver Protocol
│       │   ├── types.py                 # enums, общие типы
│       │   ├── errors.py               # unified error model
│       │   ├── crypto.py               # Fernet encrypt/decrypt
│       │   ├── http_client.py           # httpx AsyncClient manager
│       │   └── retry.py                # retry with exponential backoff
│       │
│       ├── models/
│       │   ├── device.py                # DeviceInfo, DeviceConfig
│       │   ├── status.py               # CameraStatus, DiskHealth, RecordingStatus
│       │   ├── device_health.py         # DeviceHealthSummary
│       │   ├── check_result.py          # DeviceCheckResult
│       │   ├── capabilities.py          # DeviceCapabilities
│       │   ├── polling_policy.py        # PollingPolicy
│       │   ├── snapshot.py              # SnapshotResult, SnapshotRecord
│       │   └── alert.py                # AlertEvent, AlertType, Severity
│       │
│       ├── drivers/
│       │   ├── registry.py              # driver lookup по vendor name
│       │   ├── hikvision/
│       │   │   ├── __init__.py
│       │   │   ├── driver.py            # HikvisionDriver(DeviceDriver)
│       │   │   ├── transports/
│       │   │   │   ├── __init__.py
│       │   │   │   ├── base.py          # HikvisionTransport ABC
│       │   │   │   ├── isapi.py         # IsapiTransport
│       │   │   │   └── sdk.py           # SdkTransport (stub)
│       │   │   ├── mappers.py           # vendor XML/JSON → normalized models
│       │   │   └── errors.py            # Hikvision-specific errors
│       │   ├── dahua/                   # stub
│       │   │   └── __init__.py
│       │   └── provision/               # stub
│       │       └── __init__.py
│       │
│       ├── polling/
│       │   ├── scheduler.py             # APScheduler setup
│       │   └── jobs.py                  # polling job functions
│       │
│       ├── storage/
│       │   ├── database.py              # engine, session factory
│       │   ├── tables.py               # SQLAlchemy table definitions
│       │   ├── repositories.py          # DeviceRepo, CheckResultRepo, AlertRepo
│       │   └── snapshot_store.py        # file-based snapshot storage
│       │
│       ├── alerts/
│       │   ├── engine.py                # state-based alert evaluation
│       │   └── rules.py                # alert rule definitions
│       │
│       ├── metrics/
│       │   └── collector.py             # MetricsCollector
│       │
│       └── api/
│           └── app.py                   # FastAPI: health, device status
│
├── migrations/
│   └── alembic/
│
├── tests/
│   ├── unit/
│   ├── integration/
│   └── fixtures/                        # test payloads, mock responses
│
├── data/
│   └── snapshots/                       # runtime snapshot storage
│
├── third_party/
│   └── hikvision/
│       └── device-network-sdk/
│
└── docs/
    ├── architecture/
    ├── plans/
    ├── research/
    └── vendors/
```

---

## 5. Ключевые модели

### DeviceDriver Protocol

```python
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

### Normalized Models

```python
# DeviceConfig — конфигурация подключения
device_id: str
name: str
vendor: str                          # "hikvision", "dahua", "provision"
host: str
port: int
transport_mode: TransportMode        # isapi, sdk, auto
credentials: EncryptedCredentials
polling_policy_id: str
is_active: bool

# DeviceInfo — информация об устройстве
device_id: str
model: str
serial_number: str
firmware_version: str
device_type: str                     # NVR, DVR, IPC
mac_address: str | None
channels_count: int

# CameraStatus
device_id: str
channel_id: str
channel_name: str
is_online: bool
ip_address: str | None
protocol: str | None
checked_at: datetime

# DiskHealth
device_id: str
disk_id: str
status: DiskStatus                   # ok, error, unformatted, idle
capacity_bytes: int
free_bytes: int
health_status: str
checked_at: datetime

# RecordingStatus
device_id: str
channel_id: str
is_recording: bool
record_type: str | None              # continuous, motion, schedule
checked_at: datetime

# SnapshotResult
device_id: str
channel_id: str
success: bool
file_path: str | None
file_size_bytes: int | None
error: str | None
checked_at: datetime

# DeviceHealthSummary
device_id: str
reachable: bool
camera_count: int
online_cameras: int
offline_cameras: int
disk_ok: bool
recording_ok: bool
response_time_ms: float
checked_at: datetime

# DeviceCheckResult
id: int
device_id: str
check_type: CheckType
success: bool
error_type: str | None
duration_ms: float
payload_json: dict | None
checked_at: datetime

# DeviceCapabilities
device_id: str
model: str
firmware_version: str
supports_isapi: bool
supports_sdk: bool
supports_snapshot: bool
supports_recording_status: bool
supports_disk_status: bool
detected_at: datetime

# PollingPolicy
name: str
device_info_interval: int
camera_status_interval: int
disk_status_interval: int
snapshot_interval: int

# AlertEvent
id: int
device_id: str
channel_id: str | None
alert_type: AlertType
severity: Severity
message: str
source: str
status: AlertStatus                  # active, resolved
created_at: datetime
resolved_at: datetime | None
```

---

## 6. Hikvision Driver — Transport Abstraction

```
HikvisionDriver
├── config.transport_mode = "isapi" | "sdk" | "auto"
│
├── IsapiTransport
│   └── httpx.AsyncClient (Digest Auth)
│       └── ISAPI endpoints → XML → normalized models
│
├── SdkTransport (stub для MVP)
│   └── ctypes → HCNetSDK → normalized models
│
└── Auto mode:
    1. Попробовать ISAPI
    2. Если неудача → fallback на SDK
    3. Запомнить рабочий transport в capabilities
```

### HikvisionTransport ABC

```python
class HikvisionTransport(ABC):
    @abstractmethod
    async def get_device_info(self) -> dict: ...
    @abstractmethod
    async def get_channels_status(self) -> list[dict]: ...
    @abstractmethod
    async def get_disk_status(self) -> list[dict]: ...
    @abstractmethod
    async def get_snapshot(self, channel_id: str) -> bytes: ...
```

Transport возвращает raw dict/bytes. Маппинг в normalized models делается в `mappers.py` на уровне драйвера.

---

## 7. Инфраструктура

### docker-compose.yml

```yaml
services:
  cctv_monitoring_postgres:
    image: postgres:16
    container_name: cctv_monitoring_postgres
    environment:
      POSTGRES_DB: cctv_monitoring
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

volumes:
  pgdata:
```

### .env.example

```env
POSTGRES_USER=cctv_admin
POSTGRES_PASSWORD=changeme
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DB=cctv_monitoring
ENCRYPTION_KEY=generate-with-fernet
SNAPSHOT_BASE_DIR=./data/snapshots
```

---

## 8. Scope MVP

### В scope

- Hikvision ISAPI transport (полная реализация)
- Hikvision SDK transport (stub, подготовка к PoC)
- DeviceDriver Protocol + driver registry
- Polling scheduler с PollingPolicy профилями
- Все normalized models
- DeviceCheckResult (история проверок)
- DeviceCapabilities detection
- Snapshot storage (файловая система)
- Alert Engine с state-based deduplication
- Retry policy с exponential backoff
- HTTP client с connection pooling
- Fernet-шифрование credentials
- PostgreSQL + Alembic миграции
- Docker Compose для PostgreSQL
- Seed устройств из YAML
- Minimal API (health check, device status)
- Structured logging + metrics
- Unit tests для core и mappers

### Out of scope

- Dashboard / UI
- Авторизация пользователей, роли, permissions
- Dahua / Provision драйверы (только stubs)
- Event stream / push от устройств
- Multi-tenant
- Cloud deployment, Kubernetes
- Notification channels (email, Telegram, webhook)
- Отчёты, аналитика, графики
- ONVIF integration
- Видеопоток, живое видео
- Celery / message broker
- Prometheus / Grafana

---

## 9. Приоритет вендоров

| Приоритет | Вендор | Статус MVP |
|-----------|--------|------------|
| 1 | Hikvision | Полная реализация ISAPI, SDK stub |
| 2 | Dahua | Stub (пустой драйвер) |
| 3 | Provision ISR | Stub (пустой драйвер) |

---

## 10. Ссылки на связанные документы

- [CCTV Monitoring Research Draft](../research/CCTV_MONITORING_RESEARCH_DRAFT.md)
- [Verified Endpoints](../vendors/VERIFIED_ENDPOINTS.md)
- [Hikvision Live Test Plan](../vendors/hikvision/HIKVISION_LIVE_TEST_PLAN.md)
- [MVP Improvements (Architecture Addendum)](../architecture/mvp_improvements.md)

---

## 11. Открытые вопросы

| Вопрос | Статус |
|--------|--------|
| Retention policy для DeviceCheckResult | требует решения |
| Retention policy для snapshots | требует решения |
| Конкретные интервалы в PollingPolicy | предположение, подбор после тестов |
| Capability detection для SDK | требует проверки |
| Periodic re-detection capabilities | предположение |
| Формат structured logging | требует решения |
| FastAPI или другой фреймворк для API | предположение: FastAPI |
| SQLAlchemy версия (1.4 vs 2.0) | требует решения |
| Python minimum version (3.11 vs 3.12) | требует решения |
