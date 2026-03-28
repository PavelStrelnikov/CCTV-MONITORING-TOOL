# CCTV Monitoring Tool — Deployment Info

## 1. Общая структура проекта

```
CCTV Monitoring Tool/
├── src/cctv_monitor/                  # Backend (Python/FastAPI)
│   ├── main.py                        # Точка входа (порт 8001)
│   ├── api/
│   │   ├── app.py                     # FastAPI app factory + JWT middleware
│   │   ├── deps.py                    # Dependency injection + SDK init
│   │   ├── auth.py                    # JWT-авторизация
│   │   └── routes/
│   │       ├── devices.py             # Устройства и опрос
│   │       ├── status.py              # Системный статус
│   │       ├── tags.py                # Теги устройств
│   │       ├── history.py             # История событий
│   │       ├── alerts_routes.py       # Оповещения
│   │       ├── settings.py            # Системные настройки
│   │       ├── telegram.py            # Telegram-интеграция
│   │       └── folders.py             # Папки устройств
│   ├── core/
│   │   ├── config.py                  # Settings (pydantic-settings)
│   │   ├── http_client.py             # HTTP-клиент
│   │   ├── crypto.py                  # Шифрование
│   │   ├── types.py                   # Доменные типы
│   │   ├── interfaces.py              # Абстрактные интерфейсы
│   │   ├── errors.py                  # Исключения
│   │   └── retry.py                   # Retry-логика
│   ├── drivers/
│   │   ├── registry.py                # Реестр драйверов
│   │   └── hikvision/
│   │       ├── driver.py              # HikvisionDriver
│   │       ├── mappers.py             # XML/SDK → доменные модели
│   │       └── transports/
│   │           ├── isapi.py           # HTTP ISAPI транспорт
│   │           ├── sdk.py             # SDK транспорт (ctypes)
│   │           └── sdk_bindings.py    # HCNetSDK ctypes-биндинги
│   ├── storage/
│   │   ├── database.py                # SQLAlchemy async engine
│   │   ├── tables.py                  # ORM-модели
│   │   └── repositories.py            # Data access layer
│   ├── polling/
│   │   ├── scheduler.py               # APScheduler
│   │   ├── background.py              # Фоновый опрос (30с)
│   │   └── sdk_worker.py              # SDK subprocess (crash-safe)
│   ├── telegram/                      # Telegram бот
│   │   ├── main.py                    # Точка входа бота
│   │   ├── bot.py                     # Runtime бота
│   │   ├── handlers.py                # Обработчики команд
│   │   ├── api_client.py              # Клиент внутреннего API
│   │   ├── formatters.py              # Форматирование сообщений
│   │   ├── report_pdf.py              # PDF-отчёты (Playwright)
│   │   ├── auth.py                    # Авторизация Telegram
│   │   ├── notifier.py                # Уведомления
│   │   └── scheduler.py               # Планировщик бота
│   ├── alerts/                        # Управление оповещениями
│   ├── models/                        # Модели данных
│   └── metrics/                       # Метрики
│
├── frontend/                          # React SPA (TypeScript + Vite)
│   ├── package.json
│   ├── vite.config.ts                 # Dev-сервер (порт 5173)
│   ├── index.html
│   ├── src/
│   │   ├── main.tsx
│   │   ├── App.tsx
│   │   ├── components/
│   │   ├── pages/
│   │   ├── api/
│   │   └── i18n.ts                    # i18next (русский + иврит RTL)
│   └── dist/                          # Production build
│
├── deploy/                            # Production Docker
│   ├── docker-compose.prod.yml        # Docker Compose (production)
│   ├── Dockerfile.backend             # Python backend image
│   ├── Dockerfile.caddy               # Caddy + frontend build
│   ├── Caddyfile                      # Reverse proxy конфиг
│   ├── docker-entrypoint.sh           # Миграции + запуск
│   ├── .env.example                   # Шаблон переменных
│   ├── .dockerignore
│   └── scripts/
│       └── db-backup.sh               # Бэкап/восстановление БД
│
├── migrations/                        # Alembic миграции (13 версий)
│   ├── env.py
│   ├── script.py.mako
│   └── versions/
│       ├── 0001_initial_tables.py
│       ├── ...
│       └── 0013_device_web_protocol.py
│
├── third_party/hikvision/sdk/         # Hikvision SDK
│   ├── linux-64/                      # Linux .so (production)
│   └── win-64/                        # Windows .dll (development)
│
├── tests/unit/                        # Тесты
├── docker-compose.yml                 # Development Docker Compose
├── alembic.ini                        # Конфиг Alembic
├── pyproject.toml                     # Python зависимости
├── .env.example                       # Шаблон переменных (dev)
└── devices.seed.yaml                  # Seed-данные устройств
```

**Компоненты:**
- **Backend** — FastAPI (Python 3.12), порт 8001
- **Frontend** — React 19 + TypeScript + Vite, порт 5173 (dev) / Caddy (prod)
- **База данных** — PostgreSQL 16
- **Telegram бот** — aiogram 3.4, отдельный сервис
- **Reverse proxy** — Caddy 2 (production), автоматический SSL

---

## 2. Backend

### Фреймворк
**FastAPI** + **Uvicorn** (ASGI-сервер)

### Python версия
**3.12+** (`requires-python = ">=3.12"`)

### Зависимости (pyproject.toml)

| Пакет | Версия | Назначение |
|---|---|---|
| sqlalchemy[asyncio] | >=2.0,<3.0 | Async ORM |
| asyncpg | >=0.29 | PostgreSQL async driver |
| alembic | >=1.13 | Миграции БД |
| httpx | >=0.27 | Async HTTP-клиент |
| apscheduler | >=3.10,<4.0 | Фоновый планировщик (опрос 30с) |
| fastapi | >=0.110 | Web-фреймворк |
| uvicorn | >=0.29 | ASGI-сервер |
| cryptography | >=42.0 | Шифрование паролей устройств |
| pydantic | >=2.6 | Валидация данных |
| pydantic-settings | >=2.2 | Загрузка .env |
| pyyaml | >=6.0 | Парсинг YAML |
| structlog | >=24.1 | Структурированный логгинг |
| pyjwt | >=2.8 | JWT-токены |
| bcrypt | >=4.1 | Хеширование паролей |
| aiogram | >=3.4,<4.0 | Telegram бот |
| jinja2 | >=3.1 | Шаблоны |
| playwright | >=1.40.0 | PDF-отчёты (headless Chromium) |

**Dev-зависимости:** pytest, pytest-asyncio, pytest-cov, ruff

### Точка входа
`src/cctv_monitor/main.py` → `python -m cctv_monitor.main`

Последовательность запуска:
1. Загрузка настроек из .env (pydantic-settings)
2. Создание async PostgreSQL engine
3. Автоматический запуск миграций Alembic
4. Инициализация реестра драйверов (Hikvision)
5. Запуск APScheduler (фоновый опрос каждые 30с)
6. Запуск Uvicorn на `0.0.0.0:8001`

SDK инициализируется **лениво** — только при первом запросе, чтобы не загружать DLL до момента использования.

### Порт
**8001** (HTTP)

### Переменные окружения

```ini
# === База данных (обязательные) ===
POSTGRES_USER=
POSTGRES_PASSWORD=
POSTGRES_HOST=
POSTGRES_PORT=
POSTGRES_DB=

# === Шифрование (обязательное) ===
ENCRYPTION_KEY=                    # Fernet-ключ

# === Аутентификация (обязательные для production) ===
JWT_SECRET_KEY=                    # Если не задан — авторизация отключена (dev)
JWT_ALGORITHM=                     # По умолчанию: HS256
JWT_EXPIRY_HOURS=                  # По умолчанию: 24
ADMIN_USERNAME=                    # По умолчанию: admin
ADMIN_PASSWORD_HASH=               # bcrypt-хеш пароля

# === CORS ===
CORS_ORIGINS=                      # http://localhost:5173 (dev) / https://domain.com (prod)

# === Hikvision SDK ===
HCNETSDK_LIB_PATH=                # Путь к SDK (/opt/hikvision-sdk в Docker)

# === Telegram бот (опционально) ===
TELEGRAM_BOT_TOKEN=
INTERNAL_API_BASE_URL=             # http://localhost:8001 (dev) / http://backend:8001 (Docker)
INTERNAL_API_TOKEN=                # Токен для внутреннего API
TELEGRAM_DEFAULT_TIMEZONE=         # По умолчанию: Asia/Jerusalem

# === Пути ===
SNAPSHOT_BASE_DIR=                 # По умолчанию: ./data/snapshots

# === Логгирование ===
LOG_LEVEL=                         # По умолчанию: INFO
```

### Запуск в development

```bash
# 1. Создать venv
python -m venv venv
venv\Scripts\activate       # Windows
source venv/bin/activate    # Linux

# 2. Установить зависимости
pip install -e ".[dev]"

# 3. Запустить PostgreSQL (через docker-compose.yml)
docker compose up -d postgres

# 4. Создать .env
cp .env.example .env
# Заполнить POSTGRES_PASSWORD и ENCRYPTION_KEY

# 5. Запустить backend
python -m cctv_monitor.main
```

---

## 3. Frontend

### React версия
**React 19.2** + TypeScript 5.9

### Зависимости (package.json)

| Пакет | Версия | Назначение |
|---|---|---|
| react | ^19.2.0 | UI-фреймворк |
| react-dom | ^19.2.0 | DOM-рендеринг |
| react-router-dom | ^7.13.1 | Маршрутизация (SPA) |
| @mui/material | ^7.3.9 | Material UI компоненты |
| @mui/icons-material | ^7.3.9 | Иконки |
| @mui/x-charts | ^8.27.4 | Графики |
| @mui/x-data-grid | ^8.27.4 | Таблицы |
| @emotion/react | ^11.14.0 | CSS-in-JS |
| @emotion/styled | ^11.14.1 | Styled компоненты |
| @dnd-kit/core | ^6.3.1 | Drag & drop |
| @dnd-kit/sortable | ^10.0.0 | Сортировка перетаскиванием |
| react-i18next | ^16.5.6 | Интернационализация |
| i18next | ^25.8.17 | i18n-движок |
| stylis-plugin-rtl | ^2.1.1 | RTL поддержка (иврит) |

**Dev-зависимости:** vite ^7.3.1, typescript ~5.9.3, eslint ^9.39.1

### Команда сборки

```bash
cd frontend
npm ci            # Установка зависимостей
npm run build     # tsc -b && vite build → dist/
```

### Переменные окружения
Нет специфичных переменных. API URL определяется через Vite proxy (dev) или Caddy (prod).

### Порт (dev)
**5173** (Vite dev server, `host: 0.0.0.0`)

### Обращение к backend

**Development** (vite.config.ts):
```typescript
server: {
  proxy: {
    '/api': {
      target: 'http://localhost:8001',
      changeOrigin: true,
    },
  },
}
```
Фронтенд обращается к `/api/*` → Vite проксирует на `localhost:8001`.

**Production**: Caddy проксирует `/api/*` → `backend:8001` (Docker internal DNS).

---

## 4. База данных

### PostgreSQL версия
**PostgreSQL 16**

### Миграции
**Alembic** — 13 миграций.

```bash
# Запуск миграций вручную
alembic upgrade head

# В Docker — автоматически при старте (docker-entrypoint.sh)
```

**Миграции запускаются автоматически при запуске backend** (в production через `docker-entrypoint.sh`).

### История миграций

| № | Файл | Описание |
|---|---|---|
| 1 | 0001_initial_tables.py | Device, Channel, HealthRecord, Settings |
| 2 | 0002_v2_schema.py | Реструктуризация устройств v2 |
| 3 | 0003_poll_interval.py | Per-device poll intervals |
| 4 | 0004_system_settings.py | Глобальные настройки |
| 5 | 0005_ignored_channels.py | Игнорирование каналов |
| 6 | 0006_tag_definitions.py | Теги устройств |
| 7 | 0007_telegram_integration.py | Таблицы Telegram |
| 8 | 0008_folders.py | Папки устройств |
| 9 | 0009_regenerate_device_ids.py | Регенерация ID |
| 10 | 0010_folder_color.py | Цвет папок |
| 11 | 0011_folder_icon.py | Иконки папок |
| 12 | 0012_device_display_order.py | Порядок отображения |
| 13 | 0013_device_web_protocol.py | Поле web_protocol (http/https) |

### Seed-данные
`devices.seed.yaml` — начальные устройства для разработки.

---

## 5. Docker

### docker-compose.yml (Development)

```yaml
services:
  postgres:
    image: postgres:16
    container_name: cctv_monitoring_postgres
    environment:
      POSTGRES_USER: cctv_admin
      POSTGRES_PASSWORD: changeme
      POSTGRES_DB: cctv_monitoring
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U cctv_admin -d cctv_monitoring"]
      interval: 5s
      timeout: 5s
      retries: 5

volumes:
  pgdata:
```

### deploy/docker-compose.prod.yml (Production)

```yaml
services:
  postgres:
    image: postgres:16
    restart: unless-stopped
    ports:
      - "127.0.0.1:5432:5432"       # Только localhost
    environment:
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: ${POSTGRES_DB}
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER} -d ${POSTGRES_DB}"]
      interval: 5s
      timeout: 5s
      retries: 5

  backend:
    build:
      context: ..
      dockerfile: deploy/Dockerfile.backend
    restart: unless-stopped
    expose:
      - "8001"                        # Внутренний порт
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      POSTGRES_HOST: postgres
    env_file:
      - .env
    volumes:
      - ../data/snapshots:/app/data/snapshots
      - ./backups:/app/backups

  telegram-bot:
    build:
      context: ..
      dockerfile: deploy/Dockerfile.backend
    restart: unless-stopped
    depends_on:
      backend:
        condition: service_started
    command: python -m cctv_monitor.telegram.main
    environment:
      POSTGRES_HOST: postgres
      INTERNAL_API_BASE_URL: http://backend:8001
    env_file:
      - .env

  caddy:
    build:
      context: ..
      dockerfile: deploy/Dockerfile.caddy
    restart: unless-stopped
    ports:
      - "80:80"
      - "443:443"
    environment:
      DOMAIN: ${DOMAIN}
    volumes:
      - caddy_data:/data
      - caddy_config:/config

volumes:
  pgdata:
  caddy_data:
  caddy_config:
```

### deploy/Dockerfile.backend

```dockerfile
FROM python:3.12-slim

RUN apt-get update && apt-get install -y --no-install-recommends \
    libpq-dev gcc \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY pyproject.toml .
RUN pip install --no-cache-dir .

RUN playwright install --with-deps chromium

COPY src/ src/
COPY migrations/ migrations/
COPY alembic.ini .

COPY third_party/hikvision/sdk/linux-64/ /opt/hikvision-sdk/
ENV LD_LIBRARY_PATH=/opt/hikvision-sdk

COPY deploy/docker-entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

EXPOSE 8001
ENTRYPOINT ["/entrypoint.sh"]
```

### deploy/Dockerfile.caddy

```dockerfile
# Stage 1: Сборка фронтенда
FROM node:22-alpine AS build
WORKDIR /app
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ .
RUN npm run build

# Stage 2: Caddy + статика
FROM caddy:2-alpine
COPY --from=build /app/dist /srv/frontend
COPY deploy/Caddyfile /etc/caddy/Caddyfile
EXPOSE 80 443
```

### deploy/Caddyfile

```
{$DOMAIN:localhost} {
    handle /api/* {
        reverse_proxy backend:8001 {
            flush_interval -1          # SSE streaming
        }
    }

    handle /health {
        reverse_proxy backend:8001
    }

    handle {
        root * /srv/frontend
        try_files {path} /index.html   # SPA fallback
        file_server
    }
}
```

### deploy/docker-entrypoint.sh

```bash
#!/bin/bash
set -e
echo "Running database migrations..."
alembic upgrade head
echo "Starting CCTV Monitor backend..."
exec python -m cctv_monitor.main
```

### deploy/.dockerignore

```
.git
.env
.venv
venv
node_modules
__pycache__
*.pyc
tests
backups
.pytest_cache
.coverage
third_party/hikvision/sdk/win-64/
```

---

## 6. Другие сервисы

### Telegram бот (aiogram 3.4)
- **Точка входа:** `python -m cctv_monitor.telegram.main`
- **Запуск:** Отдельный Docker-сервис `telegram-bot`
- **Функции:** команды, статусы, PDF-отчёты (Playwright + Chromium)
- **Связь с backend:** HTTP-запросы на `INTERNAL_API_BASE_URL` с заголовком `X-Internal-Token`

### APScheduler (фоновый планировщик)
- Встроен в backend-процесс
- Тикает каждые **30 секунд**
- Проверяет `poll_interval_seconds` для каждого устройства
- Опрашивает устройства через ISAPI → SDK fallback

### SDK subprocess (crash-safe)
- SDK-опрос запускается в **отдельном процессе** (`polling/sdk_worker.py`)
- Если SDK-процесс падает — основной backend не затрагивается
- Двухфазный подход: сначала основные данные, затем рискованные операции (SMART, запись)

### Нет Redis, Celery, WebSocket, внешних очередей

---

## 7. Сетевое взаимодействие

### Схема портов

| Сервис | Порт | Протокол | Доступ | Назначение |
|---|---|---|---|---|
| Frontend (dev) | 5173 | HTTP | Локально | Vite dev server |
| Backend API | 8001 | HTTP | Внутренний (Docker) | FastAPI + Uvicorn |
| Caddy (prod) | 80 | HTTP | Публичный | Редирект на HTTPS |
| Caddy (prod) | 443 | HTTPS | Публичный | Reverse proxy + SSL |
| PostgreSQL | 5432 | TCP | 127.0.0.1 (prod) | База данных |

### Development

```
Браузер (localhost:5173)
    │
    ▼
Vite Dev Server (0.0.0.0:5173)
    │  proxy /api/*
    ▼
Backend (0.0.0.0:8001)
    │
    ▼
PostgreSQL (localhost:5432)

Telegram Bot ──HTTP──▶ Backend (localhost:8001)
```

### Production (Docker)

```
Интернет
    │  порты 80, 443
    ▼
┌─────────────────────────────────┐
│  Caddy (reverse proxy + SSL)    │
│  - /* → /srv/frontend (SPA)     │
│  - /api/* → backend:8001        │
│  - /health → backend:8001       │
└─────────┬───────────────────────┘
          │ Docker internal network
          ▼
┌─────────────────────────────────┐
│  Backend (FastAPI :8001)        │
│  - API endpoints                │
│  - APScheduler (polling 30s)    │
│  - SDK subprocess (Hikvision)   │
└─────────┬───────────────────────┘
          │
          ▼
┌─────────────────────────────────┐
│  PostgreSQL 16 (:5432)          │
│  127.0.0.1 only — не публичный  │
└─────────────────────────────────┘

┌─────────────────────────────────┐
│  Telegram Bot (aiogram)         │
│  → HTTP → backend:8001          │
│  → Telegram API (polling)       │
└─────────────────────────────────┘
```

---

## Быстрый деплой на Ubuntu 24.04

### 1. Подготовка сервера

```bash
# Docker
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER

# Клонирование
git clone <repo-url> /opt/cctv-monitor
cd /opt/cctv-monitor/deploy
```

### 2. Настройка переменных

```bash
cp .env.example .env
nano .env
```

### 3. Генерация секретов

```bash
# Пароль БД
openssl rand -base64 32

# Ключ шифрования (Fernet)
python3 -c "from cryptography.fernet import Fernet; print(Fernet.generate_key().decode())"

# JWT secret
openssl rand -base64 64

# Хеш пароля админа
python3 -c "import bcrypt; print(bcrypt.hashpw(b'YOUR_PASSWORD', bcrypt.gensalt()).decode())"

# Внутренний API-токен
openssl rand -base64 32
```

### 4. Запуск

```bash
docker compose -f docker-compose.prod.yml up -d --build
docker compose -f docker-compose.prod.yml ps
docker compose -f docker-compose.prod.yml logs -f
```

### 5. Проверка

```bash
curl http://localhost/health
# или https://your-domain.com/health
```

### 6. Бэкап БД

```bash
./scripts/db-backup.sh backup
# → backups/cctv_YYYYMMDD_HHMMSS.dump

# Восстановление
./scripts/db-backup.sh restore backups/cctv_YYYYMMDD_HHMMSS.dump
```

---

## Системные требования (Production)

- **ОС:** Ubuntu 22.04+ / Debian 12+
- **Docker:** Engine 24+ с Compose v2
- **RAM:** Минимум 2 ГБ (рекомендуется 4 ГБ)
- **Диск:** Минимум 10 ГБ (больше для снимков)
- **Сеть:** Порты 80 и 443 открыты
- **Домен:** Опционально (для автоматического SSL через Let's Encrypt)
