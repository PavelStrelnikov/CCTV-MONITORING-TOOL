# Dashboard MVP Design

**Goal:** Add a web dashboard to manage CCTV devices and view their statuses — replacing manual CLI/curl workflows.

**Approach:** React SPA (Vite + TypeScript) frontend + FastAPI REST API backend. API-first design enables future Telegram/WhatsApp/mobile clients.

## Architecture

```
Browser (React SPA, port 5173)
   │
   ▼  fetch /api/*
FastAPI REST API (port 8000)
   │
   ▼  async SQLAlchemy
PostgreSQL (port 5432)
   │
   ▼  polling jobs
Hikvision NVR/DVR (ISAPI over HTTP/HTTPS)
```

Frontend and backend are separate processes. Vite dev server proxies `/api` to FastAPI during development.

## Backend — FastAPI REST API

### New files

```
src/cctv_monitor/api/
├── app.py           # Extend existing: CORS, lifespan, include routers
├── deps.py          # Dependency injection (db session, services)
├── schemas.py       # Pydantic request/response models
└── routes/
    ├── devices.py   # Device CRUD + poll
    └── status.py    # Health overview
```

### API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/devices` | List all devices with last health summary |
| `POST` | `/api/devices` | Add a new device |
| `GET` | `/api/devices/{device_id}` | Device detail: cameras, disks, alerts |
| `DELETE` | `/api/devices/{device_id}` | Remove device |
| `POST` | `/api/devices/{device_id}/poll` | Trigger immediate poll |
| `GET` | `/api/overview` | System-wide summary (total devices, online, offline) |

### Request/Response Schemas

```python
# POST /api/devices
class DeviceCreate:
    device_id: str       # e.g. "nvr-brosh-40"
    name: str            # e.g. "Brosh 40"
    vendor: str          # "hikvision"
    host: str            # "192.168.1.100"
    port: int            # 80, 8443
    username: str
    password: str        # plaintext in request, encrypted at rest

# GET /api/devices response item
class DeviceOut:
    device_id: str
    name: str
    vendor: str
    host: str
    port: int
    is_active: bool
    last_health: HealthSummary | None

class HealthSummary:
    reachable: bool
    camera_count: int
    online_cameras: int
    offline_cameras: int
    disk_ok: bool
    response_time_ms: float
    checked_at: datetime

# GET /api/devices/{id} response
class DeviceDetail:
    device: DeviceOut
    cameras: list[CameraChannel]
    disks: list[DiskInfo]
    alerts: list[Alert]

# POST /api/devices/{id}/poll response
class PollResult:
    device_id: str
    health: HealthSummary
```

### Dependency Injection

- `get_db_session()` — yields `AsyncSession` per request
- `get_driver_registry()` — returns the singleton `DriverRegistry`
- `get_settings()` — returns `Settings`

Services shared between `main.py` and API via `app.state`.

### Lifespan

Extend `create_app()` with async lifespan that:
1. Creates DB engine + session factory
2. Registers drivers
3. Starts scheduler
4. Stores everything in `app.state`
5. On shutdown: stops scheduler, closes connections

This replaces the current `main.py` setup — everything lives in the FastAPI lifespan.

## Frontend — React SPA

### New directory

```
frontend/
├── index.html
├── package.json
├── tsconfig.json
├── vite.config.ts
└── src/
    ├── main.tsx
    ├── App.tsx
    ├── api/
    │   └── client.ts        # fetch wrapper
    ├── pages/
    │   ├── DeviceList.tsx    # Main table
    │   ├── DeviceDetail.tsx  # Cameras + disks
    │   └── AddDevice.tsx     # Add device form
    ├── components/
    │   ├── StatusBadge.tsx   # Online/Offline/Unreachable badge
    │   ├── DeviceTable.tsx   # Sortable table
    │   └── Layout.tsx        # Header + nav
    └── types.ts              # TypeScript interfaces matching API schemas
```

### Pages

1. **DeviceList** — Table showing all devices: name, host, cameras (online/total), disk status, last check time, actions (poll, delete). Auto-refreshes every 30s.

2. **AddDevice** — Form: device_id, name, vendor (dropdown), host, port, username, password. Submit → POST `/api/devices` → redirect to list.

3. **DeviceDetail** — Two sections: camera channels table (id, name, IP, status) and disk table (id, capacity, free, health). Plus active alerts list. "Poll Now" button at top.

### Routing

- `/` → DeviceList
- `/devices/add` → AddDevice
- `/devices/:id` → DeviceDetail

Using `react-router-dom` v6.

### Styling

Plain CSS modules. Minimal, functional. No UI framework for MVP.

### API Client

Simple `fetch` wrapper with base URL from Vite env. Handles JSON parsing, error responses.

## Repository Changes

Add missing methods to `DeviceRepository`:
- `list_all()` — all devices (not just active)
- `create(device: DeviceTable)` — insert device
- `delete(device_id: str)` — delete device

## What's NOT in MVP

- Authentication / login
- Device editing (only add/delete)
- Snapshot viewer
- Recording status
- Telegram/WhatsApp integration (next phase)
- Mobile app (future)
