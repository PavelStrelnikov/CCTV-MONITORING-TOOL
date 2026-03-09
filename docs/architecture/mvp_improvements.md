# Architecture Addendum: MVP Improvements

> Статус: утверждён
> Дата: 2026-03-07
> Контекст: расширение базового MVP-дизайна CCTV Monitoring Platform
> Совместимость: модульный монолит Python, без изменения архитектурного стиля

---

## Цель

Дополнить базовую архитектуру MVP набором улучшений, которые обеспечат устойчивую работу системы при 50-200 устройствах. Все изменения остаются в рамках текущего модульного монолита и не требуют новых сервисов или инфраструктуры.

---

## 1. DeviceHealthSummary

### Проблема

Метод `check_health()` в `DeviceDriver` Protocol не имеет соответствующей агрегированной модели. Потребители (Alert Engine, API) вынуждены сами агрегировать данные из отдельных статусов камер, дисков и записи.

### Решение

Новая модель: `models/device_health.py`

```python
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

### Где используется

- `DeviceDriver.check_health()` возвращает `DeviceHealthSummary`
- Alert Engine принимает решения на основе этой модели
- API `/devices/{id}/health` отдаёт эту модель
- Polling job формирует summary после каждого цикла проверки устройства

### Правила формирования

| Поле | Логика |
|------|--------|
| `reachable` | `True` если устройство ответило на любой запрос |
| `disk_ok` | `True` если все диски в статусе `ok` / `normal` |
| `recording_ok` | `True` если все ожидаемые каналы пишут |
| `response_time_ms` | время ответа на `get_device_info()` |

---

## 2. DeviceCheckResult (история проверок)

### Проблема

Текущий дизайн хранит только последний статус. Нет возможности анализировать историю, искать паттерны сбоев, измерять latency.

### Решение

Новая модель и таблица: `DeviceCheckResult`

```python
@dataclass
class DeviceCheckResult:
    id: int                    # auto-increment PK
    device_id: str
    check_type: CheckType      # enum
    success: bool
    error_type: str | None     # "timeout", "auth_failed", "connection_refused", etc.
    duration_ms: float
    payload_json: dict | None  # raw response от устройства
    checked_at: datetime
```

### CheckType enum

```python
class CheckType(str, Enum):
    DEVICE_INFO = "device_info"
    CAMERA_STATUS = "camera_status"
    DISK_STATUS = "disk_status"
    RECORDING_STATUS = "recording_status"
    SNAPSHOT = "snapshot"
```

### Назначение

- История polling для диагностики
- Анализ latency по устройствам
- Debugging (raw payload сохраняется)
- Основа для будущих отчётов

### Хранение

- `payload_json` — PostgreSQL JSONB
- Индекс по `(device_id, check_type, checked_at)`
- Политика retention: определяется позже (предположение: 30 дней)

---

## 3. DeviceCapabilities

### Проблема

Разные модели и прошивки Hikvision поддерживают разный набор endpoint'ов. Без knowledge о capabilities драйвер будет тратить время на заведомо неуспешные запросы и генерировать ложные ошибки.

### Решение

Новая модель: `DeviceCapabilities`

```python
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

### Логика определения

1. При первом подключении драйвер выполняет capability detection
2. Пробует каждый endpoint, фиксирует результат
3. Сохраняет capabilities в БД
4. При следующих polling-циклах пропускает неподдерживаемые проверки
5. Re-detection по запросу или при смене прошивки

### Влияние на polling

```
if not capabilities.supports_recording_status:
    skip get_recording_statuses()
```

### Открытые вопросы

- Как определять `supports_sdk` без попытки SDK-подключения? — **требует проверки**
- Нужно ли re-detect capabilities периодически? — **предположение: при обновлении firmware_version**

---

## 4. PollingPolicy (профили опроса)

### Проблема

Единый `polling_interval` в конфигурации устройства не позволяет дифференцировать частоту разных проверок. Snapshot раз в минуту — избыточно. Camera status раз в час — недостаточно.

### Решение

Новая модель: `PollingPolicy`

```python
@dataclass
class PollingPolicy:
    name: str                          # "light", "standard", "critical"
    device_info_interval: int          # seconds
    camera_status_interval: int
    disk_status_interval: int
    snapshot_interval: int
```

В `Device` добавляется поле `polling_policy_id` (FK на PollingPolicy).

### Предустановленные профили

| Профиль | device_info | camera_status | disk_status | snapshot |
|---------|-------------|---------------|-------------|----------|
| `light` | 600 (10 мин) | 300 (5 мин) | 900 (15 мин) | 1800 (30 мин) |
| `standard` | 300 (5 мин) | 120 (2 мин) | 600 (10 мин) | 900 (15 мин) |
| `critical` | 120 (2 мин) | 60 (1 мин) | 300 (5 мин) | 300 (5 мин) |

> Интервалы — **предположение**. Подбираются после тестирования на реальных устройствах.

### Seed

Профили загружаются из YAML вместе с устройствами.

---

## 5. HTTP Client Manager

### Проблема

Создание нового `httpx` клиента на каждый запрос — wasteful. Нет connection pooling, нет переиспользования TCP-соединений.

### Решение

Новый модуль: `core/http_client.py`

```python
class HttpClientManager:
    """Manages shared httpx.AsyncClient instances with connection pooling."""

    async def get_client(self) -> httpx.AsyncClient
    async def close(self) -> None
```

### Требования

- Один `httpx.AsyncClient` на приложение (или per-device при необходимости)
- Connection pooling включён по умолчанию
- Configurable timeouts из конфигурации:
  - `connect_timeout`: 10s
  - `read_timeout`: 30s
  - `pool_timeout`: 10s
- Digest Auth настраивается per-request (разные credentials на разные устройства)
- Lifecycle привязан к lifecycle приложения (startup/shutdown)

### Где используется

- `IsapiTransport` получает client из `HttpClientManager`
- Polling jobs не создают свои клиенты

---

## 6. Retry Policy

### Проблема

Одиночный сбой сети вызывает false alert. Без retry нет устойчивости к transient errors.

### Решение

Новый модуль: `core/retry.py`

```python
@dataclass
class RetryPolicy:
    max_retries: int = 3
    base_delay: float = 1.0        # seconds
    max_delay: float = 30.0        # seconds
    exponential_base: float = 2.0
    retry_on: tuple[type[Exception], ...] = (
        ConnectionError,
        TimeoutError,
        httpx.ConnectError,
        httpx.ReadTimeout,
    )
```

### Логика

```
attempt 1: immediate
attempt 2: delay 1s
attempt 3: delay 2s
attempt 4: fail → mark as error
```

- Retry только для network errors
- НЕ retry для auth errors, 4xx responses
- Jitter добавляется к delay (±20%) для предотвращения thundering herd

### Реализация

Декоратор `@with_retry(policy)` или utility function. Без внешних зависимостей, реализуется на `asyncio.sleep`.

---

## 7. Alert Deduplication

### Проблема

Без дедупликации каждый polling-цикл с проблемой генерирует новый alert. При 100 устройствах и polling раз в минуту — поток дублей.

### Решение

State-based alert generation в Alert Engine.

### Логика

```
Состояние    | Предыдущее  | Действие
─────────────┼─────────────┼──────────────────
camera OFF   | was ON      | создать alert
camera OFF   | was OFF     | ничего (alert уже есть)
camera ON    | was OFF     | resolve alert
camera ON    | was ON      | ничего
```

### Реализация

- Alert Engine хранит `active_alerts` в БД (таблица `alerts`)
- Каждый alert имеет `status`: `active` / `resolved`
- При новой проверке Engine сравнивает текущее состояние с active alerts
- Новый alert создаётся только при **переходе из OK в PROBLEM**
- Alert разрешается при **переходе из PROBLEM в OK**

### Поля AlertEvent (обновлённые)

```python
@dataclass
class AlertEvent:
    id: int
    device_id: str
    channel_id: str | None
    alert_type: AlertType        # camera_offline, disk_error, device_unreachable, etc.
    severity: Severity           # warning, critical
    message: str
    source: str                  # "polling", "event_stream"
    status: AlertStatus          # active, resolved
    created_at: datetime
    resolved_at: datetime | None
```

---

## 8. Snapshot Storage

### Проблема

Хранение JPEG-изображений в PostgreSQL — неэффективно по производительности и размеру БД.

### Решение

Файловая система для изображений, метаданные в БД.

### Структура файлов

```
data/
└── snapshots/
    └── {device_id}/
        └── {channel_id}/
            └── {YYYYMMDD_HHMMSS}.jpg
```

### Модель в БД

```python
@dataclass
class SnapshotRecord:
    id: int
    device_id: str
    channel_id: str
    file_path: str               # relative path от base snapshot dir
    file_size_bytes: int
    checked_at: datetime
```

### Конфигурация

- `SNAPSHOT_BASE_DIR` — из env var, по умолчанию `./data/snapshots`
- Retention policy: определяется позже (предположение: хранить последние N снимков на канал)

### SnapshotResult (обновлённая модель)

```python
@dataclass
class SnapshotResult:
    device_id: str
    channel_id: str
    success: bool
    file_path: str | None        # путь к сохранённому файлу
    file_size_bytes: int | None
    error: str | None
    checked_at: datetime
```

---

## 9. Metrics

### Проблема

Без внутренних метрик невозможно понять производительность системы: сколько устройств опрашивается, какой latency, сколько ошибок.

### Решение

Новый модуль: `metrics/collector.py`

### Метрики

| Метрика | Тип | Описание |
|---------|-----|----------|
| `poll_duration_seconds` | histogram | время полного цикла опроса устройства |
| `device_response_time_ms` | histogram | время ответа устройства |
| `poll_success_total` | counter | успешные проверки |
| `poll_error_total` | counter | неуспешные проверки (по error_type) |
| `active_alerts_count` | gauge | текущее количество активных алертов |
| `devices_monitored_count` | gauge | количество устройств в мониторинге |

### Реализация MVP

- Метрики логируются через structured logging (`structlog` или стандартный `logging` с JSON formatter)
- Никаких внешних зависимостей (Prometheus, Grafana) на этом этапе
- Модуль предоставляет простой API:

```python
class MetricsCollector:
    def record_poll_duration(self, device_id: str, duration_ms: float) -> None
    def record_poll_result(self, device_id: str, check_type: str, success: bool) -> None
    def record_device_response_time(self, device_id: str, ms: float) -> None
    def get_summary(self) -> dict
```

### Эволюция

Когда понадобится Prometheus — `MetricsCollector` заменяется на реализацию с `prometheus_client`. Интерфейс остаётся тем же.

---

## Обновлённая структура проекта

Изменения относительно базового MVP-дизайна выделены комментарием `# NEW`.

```
src/
└── cctv_monitor/
    ├── __init__.py
    ├── main.py
    │
    ├── core/
    │   ├── interfaces.py
    │   ├── types.py
    │   ├── errors.py
    │   ├── crypto.py
    │   ├── http_client.py          # NEW: connection pooling
    │   └── retry.py                # NEW: retry with backoff
    │
    ├── models/
    │   ├── device.py
    │   ├── status.py
    │   ├── device_health.py        # NEW: DeviceHealthSummary
    │   ├── check_result.py         # NEW: DeviceCheckResult
    │   ├── capabilities.py         # NEW: DeviceCapabilities
    │   ├── polling_policy.py       # NEW: PollingPolicy
    │   ├── snapshot.py             # UPDATED: file_path instead of image_data
    │   └── alert.py                # UPDATED: status, deduplication fields
    │
    ├── drivers/
    │   ├── registry.py
    │   └── hikvision/
    │       ├── driver.py
    │       ├── transports/
    │       │   ├── base.py
    │       │   ├── isapi.py
    │       │   └── sdk.py
    │       ├── mappers.py
    │       └── errors.py
    │
    ├── polling/
    │   ├── scheduler.py
    │   └── jobs.py
    │
    ├── storage/
    │   ├── database.py
    │   ├── tables.py
    │   ├── repositories.py
    │   └── snapshot_store.py       # NEW: file-based snapshot storage
    │
    ├── alerts/
    │   ├── engine.py               # UPDATED: state-based deduplication
    │   └── rules.py
    │
    ├── metrics/                    # NEW
    │   └── collector.py
    │
    └── api/
        └── app.py
```

---

## Обновлённая диаграмма потока данных

```
┌──────────────┐
│  APScheduler │
│  + Polling   │
│    Policy    │──────────────────────────────┐
└──────┬───────┘                              │
       │                                      │
       ▼                                      ▼
┌──────────────┐  RetryPolicy    ┌───────────────────┐
│ Polling Job  │────────────────►│ DeviceDriver      │
│              │                 │  ├─ IsapiTransport │
│              │◄────────────────│  │   (HttpClient)  │
│              │  Normalized     │  └─ SdkTransport   │
└──────┬───────┘  Models         └───────────────────┘
       │                                      │
       │                              capability
       │                              detection
       ▼                                      │
┌──────────────┐                              ▼
│ Storage      │                  ┌───────────────────┐
│ ├─ CheckResult│◄────────────────│ DeviceCapabilities│
│ ├─ Statuses  │                  └───────────────────┘
│ ├─ Snapshots │──► filesystem
│ └─ Alerts    │
└──────┬───────┘
       │
       ▼
┌──────────────┐    state        ┌──────────────┐
│ Alert Engine │───comparison───►│ Active Alerts│
│ (dedup)      │◄────────────────│ (in DB)      │
└──────┬───────┘                 └──────────────┘
       │
       ▼
┌──────────────┐
│ Metrics      │──► structured logs
│ Collector    │
└──────────────┘
```

---

## Совместимость с базовым MVP

| Улучшение | Новые зависимости | Новая инфраструктура | Ломает существующее |
|-----------|-------------------|----------------------|---------------------|
| DeviceHealthSummary | нет | нет | нет |
| DeviceCheckResult | нет | нет | нет |
| DeviceCapabilities | нет | нет | нет |
| PollingPolicy | нет | нет | нет (расширяет Device) |
| HTTP Client Manager | httpx (уже есть) | нет | нет |
| Retry Policy | нет | нет | нет |
| Alert Deduplication | нет | нет | нет (расширяет Alert) |
| Snapshot Storage | нет | каталог `data/snapshots/` | нет |
| Metrics | structlog (опционально) | нет | нет |

Все улучшения аддитивны. Ни одно не требует изменения архитектурного подхода.

---

## Открытые вопросы

| Вопрос | Статус |
|--------|--------|
| Retention policy для DeviceCheckResult (сколько дней хранить) | требует решения |
| Retention policy для snapshots (сколько снимков на канал) | требует решения |
| Интервалы в PollingPolicy — подбор после тестирования | предположение |
| Как определять `supports_sdk` без SDK-подключения | требует проверки |
| Нужен ли periodic re-detection capabilities | предположение: при смене firmware |
| Формат structured logging (JSON vs text) | требует решения |
