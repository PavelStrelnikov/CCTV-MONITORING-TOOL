# Пошаговая инструкция по запуску CCTV Monitoring Platform

> Для разработчика, который впервые запускает систему для тестирования с реальными устройствами Hikvision.

---

## 1. Подготовка окружения

### Требования

- **Python 3.12+**
- **Docker** (для PostgreSQL)
- **pip** (или uv/pdm)
- **psycopg2-binary** (для Alembic миграций — синхронный драйвер)

### Установка зависимостей

```bash
cd "CCTV Monitoring Tool"

# Создаём виртуальное окружение
python -m venv .venv

# Активация (Windows PowerShell)
.venv\Scripts\Activate.ps1
# Или (Windows CMD)
.venv\Scripts\activate.bat
# Или (Git Bash / WSL)
source .venv/Scripts/activate

# Установка проекта + dev-зависимости
pip install -e ".[dev]"

# Alembic использует синхронный psycopg2, установи отдельно
pip install psycopg2-binary
```

### Проверка

```bash
python --version   # >= 3.12
pytest --version   # >= 8.0
alembic --version  # >= 1.13
```

---

## 2. Запуск и проверка PostgreSQL

### Запуск контейнера

```bash
docker compose up -d
```

### Проверка статуса

```bash
docker ps --filter name=cctv_monitoring_postgres
```

Ожидаемый вывод — контейнер `cctv_monitoring_postgres` со статусом `Up ... (healthy)`.

### Проверка подключения

```bash
docker exec -it cctv_monitoring_postgres psql -U cctv_admin -d cctv_monitoring -c "SELECT 1;"
```

Ожидаемый вывод:

```
 ?column?
----------
        1
```

> Если контейнер не поднимается, проверь что порт 5432 не занят другим PostgreSQL.

---

## 3. Настройка .env файла

### Создание .env из примера

```bash
cp .env.example .env
```

### Генерация ENCRYPTION_KEY (Fernet)

```bash
python -c "from cryptography.fernet import Fernet; print(Fernet.generate_key().decode())"
```

### Заполнение .env

Открой `.env` и заполни:

```ini
POSTGRES_USER=cctv_admin
POSTGRES_PASSWORD=changeme
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DB=cctv_monitoring
ENCRYPTION_KEY=<вставь сгенерированный ключ>
SNAPSHOT_BASE_DIR=./data/snapshots
LOG_LEVEL=DEBUG
```

**Обязательные поля** (без дефолтов):
- `POSTGRES_PASSWORD` — пароль к БД (в docker-compose стоит `changeme`)
- `ENCRYPTION_KEY` — ключ шифрования паролей устройств (Fernet)

> `LOG_LEVEL=DEBUG` рекомендуется при первом запуске для отладки.

### Создание директории для снапшотов

```bash
mkdir -p data/snapshots
```

---

## 4. Применение миграций

### Запуск Alembic

```bash
alembic upgrade head
```

Ожидаемый вывод:

```
INFO  [alembic.runtime.migration] Context impl PostgresqlImpl.
INFO  [alembic.runtime.migration] Will assume transactional DDL.
INFO  [alembic.runtime.migration] Running upgrade  -> 0001, initial tables
```

### Проверка таблиц

```bash
docker exec -it cctv_monitoring_postgres psql -U cctv_admin -d cctv_monitoring -c "\dt"
```

Ожидаемые таблицы:

```
 Schema |       Name           | Type  |   Owner
--------+----------------------+-------+-----------
 public | alerts               | table | cctv_admin
 public | alembic_version      | table | cctv_admin
 public | check_results        | table | cctv_admin
 public | device_capabilities  | table | cctv_admin
 public | devices              | table | cctv_admin
 public | polling_policies     | table | cctv_admin
 public | snapshots            | table | cctv_admin
```

---

## 5. Добавление тестового устройства

### Конфигурация seed-файла

Отредактируй `devices.seed.yaml`, указав реальные данные NVR:

```yaml
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
  - device_id: nvr-office-01
    name: "NVR Офис"
    vendor: hikvision
    host: "192.168.1.64"       # <-- IP твоего NVR
    port: 80                    # <-- порт ISAPI (обычно 80)
    username: admin             # <-- логин NVR
    password: "YourPassword"    # <-- пароль NVR
    transport_mode: isapi
    polling_policy_id: standard
```

### Загрузка seed-данных в БД

> Seed-загрузка пока не автоматизирована в main.py. Загрузи вручную через SQL:

```bash
docker exec -it cctv_monitoring_postgres psql -U cctv_admin -d cctv_monitoring
```

```sql
-- Политики опроса
INSERT INTO polling_policies (name, device_info_interval, camera_status_interval, disk_status_interval, snapshot_interval)
VALUES
  ('light',    600, 300, 900, 1800),
  ('standard', 300, 120, 600, 900),
  ('critical', 120, 60,  300, 300)
ON CONFLICT (name) DO NOTHING;

-- Тестовое устройство (пароль будет зашифрован приложением при чтении)
-- Пока вставляем plain-text — для MVP достаточно
INSERT INTO devices (device_id, name, vendor, host, port, username, password_encrypted, transport_mode, polling_policy_id, is_active)
VALUES (
  'nvr-office-01',
  'NVR Офис',
  'hikvision',
  '192.168.1.64',    -- IP твоего NVR
  80,
  'admin',
  'YourPassword',    -- пароль (в prod будет Fernet-encrypted)
  'isapi',
  'standard',
  true
)
ON CONFLICT (device_id) DO NOTHING;

\q
```

---

## 6. Запуск приложения

```bash
python -m cctv_monitor.main
```

Ожидаемый вывод:

```
cctv_monitor.starting
cctv_monitor.started
INFO:     Started server process
INFO:     Uvicorn running on http://0.0.0.0:8000
```

> Приложение запустится на порту 8000. FastAPI + APScheduler стартуют вместе.

---

## 7. Проверка API

### Health-check

```bash
curl http://localhost:8000/health
```

Ожидаемый ответ:

```json
{"status": "ok", "service": "cctv-monitor"}
```

### В браузере

Открой: `http://localhost:8000/health`

### Swagger UI (автогенерация FastAPI)

Открой: `http://localhost:8000/docs`

---

## 8. Проверка поллинга (логи)

При уровне `LOG_LEVEL=DEBUG` в логах будут видны события:

```
poll.result    device_id=nvr-office-01 check_type=device_info success=true
poll.duration  device_id=nvr-office-01 duration_ms=245.3
device.response_time device_id=nvr-office-01 response_time_ms=245.3
```

> **Примечание:** В текущей версии MVP поллинг-задачи (jobs) не зарегистрированы автоматически в scheduler. Scheduler создан и запущен, но конкретные jobs нужно добавить (следующий этап разработки). Пока можно протестировать ISAPI-запросы вручную (раздел 9).

---

## 9. Ручное тестирование ISAPI (curl / браузер)

Убедись, что NVR доступен по сети и проверь ISAPI-эндпоинты напрямую.

### Проверка связи с NVR

```bash
ping 192.168.1.64
```

### Device Info

```bash
curl --digest -u admin:YourPassword http://192.168.1.64/ISAPI/System/deviceInfo
```

Ожидаемый ответ — XML с информацией об устройстве:

```xml
<DeviceInfo xmlns="http://www.hikvision.com/ver20/XMLSchema">
  <deviceName>NVR</deviceName>
  <model>DS-7616NI-K2</model>
  <serialNumber>DS-7616NI-K2...</serialNumber>
  <firmwareVersion>V4.x.x</firmwareVersion>
  ...
</DeviceInfo>
```

### Статус каналов (камер)

```bash
curl --digest -u admin:YourPassword http://192.168.1.64/ISAPI/ContentMgmt/InputProxy/channels/status
```

### Статус дисков

```bash
curl --digest -u admin:YourPassword http://192.168.1.64/ISAPI/ContentMgmt/Storage/hdd
```

### Снапшот с канала 101

```bash
curl --digest -u admin:YourPassword \
  http://192.168.1.64/ISAPI/Streaming/channels/101/picture \
  --output snapshot_ch1.jpg
```

> Канал 101 = камера 1 (основной поток). Канал 102 = камера 1 (субпоток). Канал 201 = камера 2 и т.д.

### Статус записи

```bash
curl --digest -u admin:YourPassword http://192.168.1.64/ISAPI/ContentMgmt/record/tracks
```

### В браузере

Большинство ISAPI-эндпоинтов работают и через браузер — откройте URL, введите логин/пароль в диалоге Basic/Digest Auth.

---

## Частые проблемы

| Проблема | Решение |
|----------|---------|
| `Connection refused` на порту 5432 | `docker compose up -d` — PostgreSQL не запущен |
| `ENCRYPTION_KEY` ошибка | Сгенерируй ключ: `python -c "from cryptography.fernet import Fernet; print(Fernet.generate_key().decode())"` |
| `ModuleNotFoundError: cctv_monitor` | Убедись, что установил `pip install -e ".[dev]"` |
| `psycopg2` not found при Alembic | `pip install psycopg2-binary` |
| ISAPI curl возвращает 401 | Проверь логин/пароль, используй `--digest` флаг |
| ISAPI curl: connection timeout | NVR недоступен по сети, проверь IP и порт |
| Порт 8000 занят | Измени порт в `main.py` (строка `port=8000`) |

---

## Запуск тестов

```bash
# Все тесты
pytest -v

# С покрытием
pytest --cov=cctv_monitor --cov-report=term-missing

# Только конкретный модуль
pytest tests/drivers/hikvision/test_mappers.py -v

# Линтер
ruff check src/ tests/
```

Ожидаемо: **118 тестов, все PASSED**, ruff — 0 ошибок.
