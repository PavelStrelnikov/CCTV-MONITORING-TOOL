# CCTV Monitoring Platform — Архитектурный отчёт MVP

> Дата: 2026-03-07
> Версия: 1.0
> Тип: технический отчёт по реализованной системе

---

## 1. Общая архитектура системы

### Назначение

CCTV Monitoring Platform — специализированная система мониторинга состояния инфраструктуры видеонаблюдения. Система предназначена для сервисной компании, обслуживающей CCTV-оборудование на множестве объектов.

Система **не является** VMS, CRM или ticketing system. Её задача — периодический опрос NVR/DVR устройств, сбор статусов камер, дисков и записи, получение контрольных снимков и генерация алертов при отклонениях.

### Архитектурный стиль

Модульный монолит на Python с плоской структурой пакетов. Все компоненты работают в одном процессе, взаимодействуя через прямые вызовы. Асинхронная модель выполнения на базе `asyncio`.

### Основные компоненты и их роли

| Компонент | Пакет | Роль |
|-----------|-------|------|
| Core | `core/` | Базовые абстракции: конфигурация, типы, ошибки, HTTP-клиент, retry, криптография |
| Models | `models/` | Нормализованные доменные модели (dataclass), вендоронезависимые |
| Drivers | `drivers/` | Реализация DeviceDriver Protocol для каждого вендора |
| Transports | `drivers/*/transports/` | Низкоуровневый доступ к устройству (HTTP/ISAPI, SDK) |
| Storage | `storage/` | SQLAlchemy таблицы, репозитории, файловое хранилище снапшотов |
| Polling | `polling/` | Задачи опроса устройств и планировщик APScheduler |
| Alerts | `alerts/` | Движок алертов с дедупликацией по состоянию |
| Metrics | `metrics/` | In-memory коллектор метрик с логированием через structlog |
| API | `api/` | FastAPI — минимальный REST API |

### Логический поток данных

```
APScheduler (cron/interval)
    │
    ▼
poll_device_health(driver, config, metrics)
    │
    ├─► driver.connect(config)
    │       │
    │       ▼
    │   transport.connect(host, port, user, pass)
    │       │ (ISAPI: httpx + DigestAuth)
    │       │ (SDK: ctypes → HCNetSDK — stub)
    │
    ├─► driver.check_health()
    │       │
    │       ├─► transport.get_device_info()  → raw dict
    │       │       │
    │       │       ▼
    │       │   HikvisionMapper.parse_device_info(xml) → DeviceInfo
    │       │
    │       ├─► transport.get_channels_status() → raw dict
    │       │       │
    │       │       ▼
    │       │   HikvisionMapper.parse_channels_status(xml) → list[CameraChannelStatus]
    │       │
    │       └─► transport.get_disk_status() → raw dict
    │               │
    │               ▼
    │           HikvisionMapper.parse_disk_status(xml) → list[DiskHealthStatus]
    │
    ├─► DeviceCheckResult (success/error, duration)
    │
    ├─► MetricsCollector.record_*(...)
    │
    ├─► AlertEngine.evaluate(health, active_alerts)
    │       │
    │       ├─► new_alerts[]    → AlertRepository.create_alert()
    │       └─► resolved[]      → AlertRepository.resolve_alert()
    │
    └─► driver.disconnect()
```

---

## 2. Структура проекта

```
cctv-monitoring/
├── pyproject.toml                  # Зависимости, настройки pytest/ruff
├── docker-compose.yml              # PostgreSQL 16 (cctv_monitoring_postgres)
├── alembic.ini                     # Конфигурация Alembic
├── devices.seed.yaml               # Seed-данные для dev (устройства + политики)
├── .env.example                    # Шаблон переменных окружения
│
├── src/cctv_monitor/
│   ├── main.py                     # Точка входа: wiring всех компонентов
│   │
│   ├── core/
│   │   ├── config.py               # Settings (pydantic-settings, из .env)
│   │   ├── types.py                # StrEnum: DeviceVendor, DeviceTransport,
│   │   │                           #   CameraStatus, DiskStatus, AlertType,
│   │   │                           #   AlertStatus, AlertSeverity, CheckType
│   │   ├── errors.py               # CCTVMonitorError + ErrorCode + подклассы
│   │   ├── interfaces.py           # DeviceDriver Protocol (9 методов)
│   │   ├── crypto.py               # Fernet encrypt/decrypt для credentials
│   │   ├── http_client.py          # HttpClientManager (httpx AsyncClient pool)
│   │   └── retry.py                # RetryPolicy + with_retry (exp. backoff)
│   │
│   ├── models/
│   │   ├── device.py               # DeviceConfig, DeviceInfo
│   │   ├── status.py               # CameraChannelStatus, DiskHealthStatus,
│   │   │                           #   ChannelRecordingStatus
│   │   ├── device_health.py        # DeviceHealthSummary
│   │   ├── check_result.py         # DeviceCheckResult
│   │   ├── capabilities.py         # DeviceCapabilities
│   │   ├── polling_policy.py       # PollingPolicy
│   │   ├── snapshot.py             # SnapshotResult
│   │   └── alert.py                # AlertEvent
│   │
│   ├── drivers/
│   │   ├── registry.py             # DriverRegistry (vendor → driver class)
│   │   ├── hikvision/
│   │   │   ├── driver.py           # HikvisionDriver (DeviceDriver impl)
│   │   │   ├── mappers.py          # HikvisionMapper (XML → models)
│   │   │   ├── errors.py           # IsapiError, IsapiAuthError, SdkError
│   │   │   └── transports/
│   │   │       ├── base.py         # HikvisionTransport ABC (7 методов)
│   │   │       ├── isapi.py        # IsapiTransport (httpx, DigestAuth)
│   │   │       └── sdk.py          # SdkTransport (stub)
│   │   ├── dahua/                  # stub
│   │   └── provision/              # stub
│   │
│   ├── storage/
│   │   ├── database.py             # create_engine, create_session_factory
│   │   ├── tables.py               # 6 ORM таблиц (SQLAlchemy 2.0)
│   │   ├── repositories.py         # Device, CheckResult, Alert, Snapshot repos
│   │   ├── snapshot_store.py       # SnapshotStore (файловая система)
│   │   └── seed.py                 # parse_seed_file (YAML → dict)
│   │
│   ├── alerts/
│   │   ├── engine.py               # AlertEngine.evaluate()
│   │   └── rules.py                # Правила: unreachable, offline, disk_error
│   │
│   ├── metrics/
│   │   └── collector.py            # MetricsCollector (in-memory + structlog)
│   │
│   ├── polling/
│   │   ├── jobs.py                 # poll_device_health()
│   │   └── scheduler.py            # create_scheduler() → AsyncIOScheduler
│   │
│   └── api/
│       └── app.py                  # create_app() → FastAPI (/health)
│
├── migrations/
│   ├── env.py                      # Alembic env (собирает URL из env vars)
│   └── versions/
│       └── 0001_initial_tables.py  # Начальная миграция (6 таблиц)
│
├── tests/
│   ├── fixtures/hikvision/         # XML-ответы устройств для тестов
│   │   ├── device_info.xml
│   │   ├── channels_status.xml
│   │   └── hdd_status.xml
│   ├── unit/                       # 118 unit тестов
│   │   ├── core/                   # config, crypto, errors, http_client,
│   │   │                           #   retry, types
│   │   ├── models/                 # Все модели
│   │   ├── drivers/                # registry, hikvision (driver, transport,
│   │   │                           #   mappers)
│   │   ├── storage/                # seed, snapshot_store
│   │   ├── alerts/                 # engine
│   │   ├── metrics/                # collector
│   │   ├── polling/                # jobs
│   │   └── api/                    # app (health endpoint)
│   └── integration/                # repository tests (требуют PostgreSQL)
│
└── docs/
    ├── architecture/               # Архитектурные документы
    ├── plans/                      # Дизайн + план реализации
    ├── research/                   # Исследование CCTV мониторинга
    └── vendors/                    # Документация вендоров, SDK
```

### Взаимодействие пакетов

Зависимости направлены строго вниз:

```
api, polling → drivers → core, models
         ↓
    alerts, metrics → models → core/types
         ↓
      storage → models, core
```

Пакет `core` не зависит ни от кого. `models` зависит только от `core/types`. Все остальные пакеты зависят от `core` и `models`, но не зависят друг от друга (кроме `polling`, который использует `metrics`).

---

## 3. Driver Architecture

### DeviceDriver Protocol

Центральная абстракция системы — `DeviceDriver` Protocol (`core/interfaces.py`). Это структурный протокол Python (duck typing), определяющий 9 асинхронных методов:

```python
class DeviceDriver(Protocol):
    async def connect(self, config: DeviceConfig) -> None
    async def disconnect(self) -> None
    async def get_device_info(self) -> DeviceInfo
    async def get_camera_statuses(self) -> list[CameraChannelStatus]
    async def get_disk_statuses(self) -> list[DiskHealthStatus]
    async def get_recording_statuses(self) -> list[ChannelRecordingStatus]
    async def get_snapshot(self, channel_id: ChannelId) -> SnapshotResult
    async def check_health(self) -> DeviceHealthSummary
    async def detect_capabilities(self) -> DeviceCapabilities
```

Любой класс, реализующий эти методы, автоматически удовлетворяет протоколу — наследование не требуется.

Все методы возвращают **нормализованные модели**, а не вендор-специфичные структуры. Это позволяет остальной системе (polling, alerts, storage) работать с любым вендором единообразно.

### Driver Registry

`DriverRegistry` — простой реестр, связывающий `DeviceVendor` enum с классом драйвера:

```python
registry = DriverRegistry()
registry.register(DeviceVendor.HIKVISION, HikvisionDriver)
driver_cls = registry.get(DeviceVendor.HIKVISION)
```

При запуске системы в `main.py` регистрируются все доступные драйверы. При polling система определяет vendor устройства из конфигурации и запрашивает нужный класс из реестра.

### Разделение Driver / Transport

Архитектура Hikvision-драйвера имеет два уровня абстракции:

```
┌──────────────────────┐
│   HikvisionDriver    │  ← Бизнес-логика: маппинг, health check,
│                      │     capabilities detection
│   (DeviceDriver)     │
└──────────┬───────────┘
           │ делегирует
┌──────────▼───────────┐
│  HikvisionTransport  │  ← Протокол доступа к устройству
│       (ABC)          │
├──────────────────────┤
│  IsapiTransport      │  ← HTTP/ISAPI + Digest Auth (реализован)
│  SdkTransport        │  ← HCNetSDK через ctypes (stub)
└──────────────────────┘
```

**Зачем это разделение:**

1. **Один и тот же Hikvision NVR** может быть доступен через HTTP (ISAPI) или через проприетарный SDK. Разные модели и прошивки поддерживают разные транспорты.
2. **Driver** содержит бизнес-логику (health check, capabilities, маппинг XML в модели), которая одинакова для обоих транспортов.
3. **Transport** содержит только протокольную логику (HTTP-запросы или ctypes-вызовы). Он возвращает сырые данные (`dict` с XML, `bytes` для снапшотов).
4. Это позволяет реализовать **auto mode** — попробовать ISAPI, при неудаче переключиться на SDK — без дублирования бизнес-логики.

---

## 4. Hikvision Implementation

### Текущее состояние

| Компонент | Статус | Описание |
|-----------|--------|----------|
| IsapiTransport | реализован | HTTP/ISAPI, httpx, Digest Auth |
| SdkTransport | stub | Все методы выбрасывают `NotImplementedError` |
| HikvisionDriver | реализован | Полная реализация DeviceDriver Protocol |
| HikvisionMapper | реализован | Парсинг XML, 3 метода |
| Hikvision Errors | реализован | IsapiError, IsapiAuthError, SdkError |

### ISAPI Transport

`IsapiTransport` — полностью функциональный HTTP-транспорт для Hikvision ISAPI.

**Эндпоинты:**

| Константа | Путь | Назначение |
|-----------|------|------------|
| `DEVICE_INFO` | `/ISAPI/System/deviceInfo` | Информация об устройстве |
| `CHANNELS_STATUS` | `/ISAPI/ContentMgmt/InputProxy/channels/status` | Статусы каналов/камер |
| `HDD_STATUS` | `/ISAPI/ContentMgmt/Storage/hdd` | Состояние HDD |
| `SNAPSHOT` | `/ISAPI/Streaming/channels/{channel_id}/picture` | Снимок с камеры |
| `RECORDING_STATUS` | `/ISAPI/ContentMgmt/record/tracks` | Статус записи |

**Ключевые решения:**

- **Digest Auth** (`httpx.DigestAuth`) — стандартный метод аутентификации Hikvision ISAPI.
- **HTTP/HTTPS** — автоматический выбор схемы по порту (443 → HTTPS, остальное → HTTP).
- **Connection pooling** — через общий `HttpClientManager` (один `httpx.AsyncClient` на всё приложение, `verify=False` для самоподписанных сертификатов).
- **Централизованная обработка ошибок** — метод `_request()` выбрасывает `IsapiAuthError` при 401 и `IsapiError` при любом 4xx/5xx.

### XML Mapping

`HikvisionMapper` — статический класс с тремя методами парсинга:

```
parse_device_info(xml, device_id)       → DeviceInfo
parse_channels_status(xml, device_id, t) → list[CameraChannelStatus]
parse_disk_status(xml, device_id, t)     → list[DiskHealthStatus]
```

Маппер обрабатывает XML с namespace `http://www.hikvision.com/ver20/XMLSchema` через helper `_find_text()`, который сначала пробует namespaced-поиск, затем fallback без namespace (для совместимости с разными прошивками).

**Конвертации:**
- `<online>true</online>` → `CameraStatus.ONLINE` enum
- `<status>ok</status>` → `DiskStatus.OK` enum (через lookup-таблицу)
- `<capacity>2000</capacity>` (MB) → `capacity_bytes = 2000 * 1024 * 1024`

### Поток данных Hikvision

```
ISAPI XML Response
    │
    ▼
IsapiTransport._request("GET", "/ISAPI/...")
    │
    ▼
{"raw_xml": "<DeviceInfo>...</DeviceInfo>"}
    │
    ▼
HikvisionMapper.parse_*(raw_xml, device_id, checked_at)
    │
    ▼
DeviceInfo / CameraChannelStatus / DiskHealthStatus
    │  (нормализованные dataclass модели)
    ▼
Используются polling, alerts, storage — вендоронезависимо
```

---

## 5. Нормализованные модели данных

Все модели — Python `dataclass`, расположены в `models/`.

### Перечень моделей

| Модель | Файл | Назначение |
|--------|------|------------|
| `DeviceConfig` | device.py | Конфигурация подключения к устройству |
| `DeviceInfo` | device.py | Информация об устройстве (модель, серийный номер, прошивка) |
| `CameraChannelStatus` | status.py | Статус одного канала/камеры (online/offline, IP) |
| `DiskHealthStatus` | status.py | Состояние одного HDD (capacity, free, status) |
| `ChannelRecordingStatus` | status.py | Статус записи на канале |
| `DeviceHealthSummary` | device_health.py | Агрегированная сводка по устройству |
| `DeviceCheckResult` | check_result.py | Результат одной проверки (success, duration, error) |
| `DeviceCapabilities` | capabilities.py | Возможности устройства (ISAPI/SDK, snapshot, disk) |
| `PollingPolicy` | polling_policy.py | Интервалы опроса (device_info, cameras, disks, snapshot) |
| `SnapshotResult` | snapshot.py | Результат получения снимка |
| `AlertEvent` | alert.py | Событие алерта (тип, severity, статус) |

### Зачем нормализация

Слой нормализации решает три задачи:

1. **Вендоронезависимость.** Hikvision возвращает XML с `<InputProxyChannelStatus>`, Dahua будет возвращать JSON с другими именами полей. Нормализованные модели единообразны для всех вендоров.

2. **Стабильный контракт.** Polling, alerts, storage и API работают с одними и теми же моделями. Изменения в формате ответа устройства затрагивают только маппер конкретного вендора, а не всю систему.

3. **Типизация.** Модели используют строго типизированные поля: `CameraStatus` enum вместо строки `"true"/"false"`, `DiskStatus` enum вместо произвольного текста, `DeviceId`/`ChannelId` type alias для читаемости.

---

## 6. Polling Architecture

### Общая схема

```
APScheduler (AsyncIOScheduler)
    │
    │  trigger: interval (из PollingPolicy)
    │
    ▼
poll_device_health(driver, config, metrics)
    │
    ├─► driver.connect(config)
    ├─► driver.check_health()
    │       ├─► get_device_info()      → DeviceInfo
    │       ├─► get_camera_statuses()  → list[CameraChannelStatus]
    │       └─► get_disk_statuses()    → list[DiskHealthStatus]
    │
    ├─► metrics.record_poll_result()
    ├─► metrics.record_device_response_time()
    ├─► metrics.record_poll_duration()
    │
    ├─► return DeviceCheckResult(success=True/False, duration_ms=...)
    │
    └─► driver.disconnect()  (в finally)
```

### Polling Jobs

На текущий момент реализована одна job-функция — `poll_device_health()`. Она:

1. Подключается к устройству через драйвер
2. Выполняет комплексную проверку здоровья (`check_health`)
3. Записывает метрики (успех/ошибка, время ответа, длительность)
4. Возвращает `DeviceCheckResult` с результатом
5. В `finally` гарантированно отключается от устройства

При ошибке подключения job не падает — возвращает `DeviceCheckResult` с `success=False` и `error_type`.

### APScheduler

`create_scheduler()` создаёт `AsyncIOScheduler` с настройками:

- `coalesce: True` — при накоплении пропущенных запусков выполнить только один
- `max_instances: 1` — не запускать параллельные инстансы одной job
- `misfire_grace_time: 60` — допустимая задержка запуска (60 секунд)

### Polling Policies

Seed-файл содержит три профиля опроса:

| Профиль | device_info | cameras | disks | snapshot |
|---------|-------------|---------|-------|----------|
| light | 600с (10м) | 300с (5м) | 900с (15м) | 1800с (30м) |
| standard | 300с (5м) | 120с (2м) | 600с (10м) | 900с (15м) |
| critical | 120с (2м) | 60с (1м) | 300с (5м) | 300с (5м) |

Каждое устройство привязано к одной политике через `polling_policy_id`.

---

## 7. Alert Engine

### Архитектура

Alert Engine реализован как stateless evaluator — не хранит состояние между вызовами. Вместо этого получает текущее состояние и список активных алертов, возвращает два списка: новые алерты и разрешённые.

```python
new_alerts, resolved = engine.evaluate(health, active_alerts)
```

### Правила (rules.py)

Три правила, каждое — чистая функция:

| Правило | Условие | Тип алерта | Severity |
|---------|---------|------------|----------|
| `check_device_unreachable` | `not health.reachable` | `DEVICE_UNREACHABLE` | CRITICAL |
| `check_camera_offline` | `health.offline_cameras > 0` | `CAMERA_OFFLINE` | WARNING |
| `check_disk_error` | `not health.disk_ok` | `DISK_ERROR` | CRITICAL |

Правила объединены в список `ALL_RULES`. Для добавления нового правила достаточно написать функцию и добавить её в этот список.

### State-based Deduplication

Алгоритм дедупликации предотвращает создание дублирующих алертов:

1. Собрать `active_types` — множество `alert_type` из текущих активных алертов
2. Для каждого сработавшего правила:
   - Если `alert_type` уже есть в `active_types` → **пропустить** (дубликат)
   - Если `alert_type` нет в `active_types` → **создать** новый алерт
3. Для каждого активного алерта:
   - Если его `alert_type` **не сработал** ни в одном правиле → **разрешить** (устройство восстановилось)

### Пример цикла

```
Цикл 1: устройство недоступно
  → new_alerts: [DEVICE_UNREACHABLE]
  → resolved: []

Цикл 2: устройство всё ещё недоступно
  → new_alerts: []             ← дедупликация, алерт уже активен
  → resolved: []

Цикл 3: устройство восстановилось
  → new_alerts: []
  → resolved: [DEVICE_UNREACHABLE]  ← автоматическое разрешение
```

---

## 8. Storage Layer

### SQLAlchemy Tables

6 таблиц, определённых через SQLAlchemy 2.0 Mapped columns:

| Таблица | Назначение | Ключевые поля |
|---------|------------|---------------|
| `polling_policies` | Профили интервалов опроса | name (PK), интервалы |
| `devices` | Конфигурация устройств | device_id (PK), host, vendor, credentials |
| `device_capabilities` | Возможности устройств | device_id (FK/PK), supports_* флаги |
| `check_results` | История проверок | id (PK), device_id, check_type, success, duration |
| `snapshots` | Метаданные снапшотов | id (PK), device_id, channel_id, file_path |
| `alerts` | Алерты | id (PK), device_id, alert_type, severity, status |

**Индексы:**
- `ix_check_results_device_type_time` — для быстрого поиска по `(device_id, check_type, checked_at)`
- `ix_alerts_device_status` — для быстрого поиска активных алертов по `(device_id, status)`

**Связи:**
- `devices.polling_policy_id` → `polling_policies.name`
- `device_capabilities.device_id` → `devices.device_id`
- `check_results.device_id` → `devices.device_id`
- `snapshots.device_id` → `devices.device_id`
- `alerts.device_id` → `devices.device_id`

### Alembic Migrations

Одна начальная миграция `0001_initial_tables.py` создаёт все 6 таблиц. Alembic env настроен на сборку URL из переменных окружения (`POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_HOST`, `POSTGRES_PORT`, `POSTGRES_DB`).

### Repositories

4 репозитория, каждый принимает `AsyncSession`:

| Репозиторий | Методы |
|-------------|--------|
| `DeviceRepository` | `get_active_devices()`, `get_by_id(device_id)` |
| `CheckResultRepository` | `save(result)`, `get_latest(device_id, check_type)` |
| `AlertRepository` | `get_active_alerts(device_id)`, `create_alert(alert)`, `resolve_alert(alert_id)` |
| `SnapshotRepository` | `save(record)` |

### Хранение данных

**История проверок** (`check_results`): каждый вызов `poll_device_health` создаёт запись с `success`, `duration_ms`, `error_type`. Таблица растёт линейно — retention policy пока не реализован.

**Алерты** (`alerts`): алерты создаются со статусом `active`, при восстановлении обновляются на `resolved` с заполнением `resolved_at`. Индекс по `(device_id, status)` обеспечивает быстрый поиск активных алертов.

**Пароли устройств** (`devices.password_encrypted`): шифруются Fernet (симметричное шифрование), ключ берётся из переменной окружения `ENCRYPTION_KEY`.

---

## 9. Snapshot Storage

### Архитектура

Снапшоты хранятся **на файловой системе**, а не в PostgreSQL. Это осознанное решение — JPEG-изображения раздули бы базу.

`SnapshotStore` сохраняет файлы в иерархическую структуру:

```
{SNAPSHOT_BASE_DIR}/
  └── {device_id}/
      └── {channel_id}/
          └── 20260307_143022.jpg
```

**Метаданные** (путь к файлу, размер, время) сохраняются в таблицу `snapshots` через `SnapshotRepository`.

### Процесс

1. `driver.get_snapshot(channel_id)` — получает `bytes` от транспорта
2. `SnapshotStore.save(device_id, channel_id, image_data)` — записывает файл асинхронно (`asyncio.to_thread`)
3. `SnapshotRepository.save(record)` — сохраняет метаданные в БД

Запись файла выполняется в отдельном потоке через `asyncio.to_thread`, чтобы не блокировать event loop.

---

## 10. Metrics

### MetricsCollector

In-memory коллектор, работающий в рамках процесса. Не использует внешних систем (Prometheus запланирован на следующий этап).

**Что собирается:**

| Метод | Данные |
|-------|--------|
| `record_poll_result(device_id, check_type, success)` | Счётчики: total, success, failed |
| `record_poll_duration(device_id, duration_ms)` | Длительность polling job (лог) |
| `record_device_response_time(device_id, ms)` | Время ответа устройства (хранится для avg) |

**Доступ к метрикам:**

`get_summary()` возвращает агрегированные данные:

```python
{
    "total_polls": 150,
    "successful_polls": 142,
    "failed_polls": 8,
    "devices": {
        "nvr-01": {"avg_response_ms": 85.3, "poll_count": 50},
        "nvr-02": {"avg_response_ms": 120.1, "poll_count": 50},
    }
}
```

Все записи дублируются в structlog (`logger.debug`) для structured logging.

---

## 11. API Layer

### Текущая реализация

Минимальный FastAPI с одним эндпоинтом:

```
GET /health → {"status": "ok", "service": "cctv-monitor"}
```

Приложение создаётся через фабрику `create_app()`, что позволяет:
- Создавать изолированные инстансы для тестов
- Добавлять middleware и зависимости при расширении

### Интеграция

FastAPI запускается через uvicorn внутри `main.py`:

```python
app = create_app()
config = uvicorn.Config(app, host="0.0.0.0", port=8000)
server = uvicorn.Server(config)
await server.serve()
```

API работает в том же asyncio event loop, что и polling scheduler.

---

## 12. Application Wiring

### main.py — точка входа

`main.py` выполняет инициализацию и связывание всех компонентов:

```python
async def main():
    # 1. Logging
    structlog.configure(...)

    # 2. Configuration
    settings = Settings()          # из .env / переменных окружения

    # 3. Infrastructure
    http_client = HttpClientManager()
    metrics = MetricsCollector()
    engine = create_engine(settings.database_url)
    session_factory = create_session_factory(engine)

    # 4. Drivers
    registry = DriverRegistry()
    registry.register(DeviceVendor.HIKVISION, HikvisionDriver)

    # 5. Scheduler
    scheduler = create_scheduler()
    scheduler.start()

    # 6. API
    app = create_app()
    server = uvicorn.Server(uvicorn.Config(app, ...))

    # 7. Run + graceful shutdown
    try:
        await server.serve()
    finally:
        scheduler.shutdown()
        await http_client.close()
        await engine.dispose()
```

### Порядок инициализации

1. **Logging** — structlog с фильтрацией по уровню INFO
2. **Settings** — pydantic-settings загружает из `.env`
3. **HTTP Client** — единый httpx.AsyncClient с connection pooling
4. **Metrics** — in-memory коллектор
5. **Database** — async SQLAlchemy engine + session factory
6. **Drivers** — регистрация в реестре
7. **Scheduler** — APScheduler с asyncio backend
8. **API** — FastAPI + uvicorn

### Graceful Shutdown

В `finally` блоке последовательно:
1. Остановка планировщика (`scheduler.shutdown()`)
2. Закрытие HTTP-соединений (`http_client.close()`)
3. Закрытие пула БД (`engine.dispose()`)

---

## 13. Текущий статус MVP

### Полностью реализовано

- Core framework: конфигурация, типы, ошибки, retry, crypto, HTTP client
- 11 нормализованных моделей данных
- DeviceDriver Protocol + Driver Registry
- Hikvision ISAPI transport (полностью функциональный)
- Hikvision driver с маппингом XML → модели
- 6 SQLAlchemy таблиц + начальная миграция Alembic
- 4 репозитория (Device, CheckResult, Alert, Snapshot)
- Файловое хранилище снапшотов
- Alert engine с state-based deduplication (3 правила)
- In-memory metrics collector с structlog
- Polling job `poll_device_health`
- APScheduler конфигурация
- FastAPI `/health` endpoint
- `main.py` wiring
- 118 unit-тестов (все проходят)
- Ruff linter (все проверки пройдены)
- Seed-файл с 3 polling policy профилями

### Stubs (заглушки)

- **Hikvision SDK Transport** — все методы выбрасывают `NotImplementedError`. Подготовлена документация (`SDK_INTEGRATION_PLAN.md`).
- **Dahua driver** — пустой пакет
- **Provision ISR driver** — пустой пакет
- **Recording status** — `get_recording_statuses()` возвращает `[]` (endpoint не верифицирован)

### Готово к использованию

Система готова к запуску с Hikvision NVR/DVR через ISAPI. Для production необходимо:
1. Запустить PostgreSQL (docker-compose up)
2. Выполнить миграцию (alembic upgrade head)
3. Настроить `.env` с credentials
4. Заполнить устройства (seed или прямая вставка в БД)
5. Запустить приложение (`python -m cctv_monitor.main`)

---

## 14. Потенциальные архитектурные риски

### 14.1. Масштабируемость polling

**Риск:** При 200+ устройствах все polling jobs выполняются в одном asyncio event loop. Если устройства отвечают медленно (timeout 30с), очередь заданий будет накапливаться.

**Смягчение:** APScheduler с `max_instances=1` и `coalesce=True` предотвращает перегрузку. Однако при большом количестве устройств может потребоваться: batch polling, concurrent workers или переход на Celery.

### 14.2. HTTP Connection Management

**Риск:** Один `httpx.AsyncClient` на всё приложение. При 200 устройствах параллельные запросы могут исчерпать connection pool.

**Смягчение:** `pool_size` настраивается в `HttpClientManager`. Текущий default pool httpx — 100 соединений, что достаточно для MVP (50-200 устройств с последовательным опросом).

**Риск:** `verify=False` отключает проверку SSL-сертификатов. Это необходимо для самоподписанных сертификатов CCTV-устройств, но создаёт потенциальную уязвимость MITM.

### 14.3. Отсутствие retry на уровне polling

**Риск:** `RetryPolicy` реализован, но не интегрирован в `poll_device_health()`. Текущая реализация делает одну попытку — при ошибке сразу записывает failure.

**Смягчение:** Retry policy готов к использованию. Требуется обернуть вызовы транспорта в `with_retry()`.

### 14.4. In-memory метрики

**Риск:** `MetricsCollector` хранит все response_time в памяти без ограничений. При длительной работе и большом количестве устройств потребление памяти будет расти линейно.

**Смягчение:** Реализовать ring buffer или periodic flush. Переход на Prometheus решит проблему системно.

### 14.5. Retention отсутствует

**Риск:** Таблицы `check_results` и `snapshots` растут неограниченно.

**Смягчение:** Необходимо реализовать retention policy (удаление записей старше N дней, ротация снапшотов).

### 14.6. Отсутствие health check для самих компонентов

**Риск:** `/health` endpoint возвращает статический `"ok"`. Не проверяет реальное состояние PostgreSQL, scheduler, HTTP client.

**Смягчение:** Расширить health endpoint проверками readiness компонентов.

### 14.7. Credentials в DeviceConfig

**Риск:** `DeviceConfig.password` передаётся как plain text внутри приложения. Шифрование Fernet используется только при хранении в БД (`password_encrypted`), но при извлечении пароль расшифровывается и живёт в памяти.

**Смягчение:** Это стандартный подход для MVP. Для production рассмотреть Vault-интеграцию.

---

## 15. Следующие логические этапы развития

### Этап 1: Production Hardening

- Интеграция `with_retry()` в polling jobs
- Расширение `/health` endpoint (readiness check: DB, scheduler)
- Retention policy для `check_results` и `snapshots`
- Logging конфигурация (JSON output, уровни по модулям)
- Error reporting (Sentry или аналог)

### Этап 2: Расширение API

- `GET /api/devices` — список устройств с текущим статусом
- `GET /api/devices/{id}` — детальная информация
- `GET /api/devices/{id}/health` — последний health check
- `GET /api/alerts` — активные алерты с фильтрацией
- `GET /api/metrics` — метрики системы
- `GET /api/snapshots/{device_id}/{channel_id}/latest` — последний снапшот

### Этап 3: Web UI / Dashboard

- React/Vue dashboard для оператора
- Карта объектов с визуальным статусом устройств
- Timeline алертов
- Галерея снапшотов
- Ручной trigger проверки устройства

### Этап 4: Device Onboarding

- API для добавления/редактирования устройств
- Auto-discovery устройств в сети (ONVIF, ARP scan)
- Автоматическое определение вендора и capabilities
- Bulk import из CSV/Excel

### Этап 5: Notification Channels

- Email уведомления
- Telegram бот
- Webhook интеграция
- Configurable правила маршрутизации алертов (кому, при каком severity)

### Этап 6: Поддержка новых вендоров

- Dahua driver (ISAPI-совместимый, но с отличиями)
- Provision ISR driver
- ONVIF fallback для универсальной поддержки
- Hikvision SDK transport (ctypes → HCNetSDK)

### Этап 7: Observability

- Prometheus exporter (замена in-memory metrics)
- Grafana dashboards
- Alertmanager интеграция
- Distributed tracing (OpenTelemetry)

### Этап 8: Масштабирование

- Distributed polling (Celery / task queue)
- Multi-tenant поддержка
- Database sharding / partitioning для check_results
- Horizontal scaling через Kubernetes
