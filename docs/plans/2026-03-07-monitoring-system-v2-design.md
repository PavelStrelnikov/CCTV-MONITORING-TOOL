# CCTV Monitoring System v2 — Design

## Goal

Transform the current basic CCTV polling tool into a full monitoring system with rich device details, automatic background polling, health history with charts, tags for organization, snapshots, and a professional MUI-based frontend.

## Decisions

- **Organization**: Flat device list with tags for filtering (city, client, site)
- **Metrics**: History in own DB + charts in frontend (no Prometheus for now)
- **UI Framework**: MUI (Material UI) for React
- **Polling**: Background scheduler (APScheduler) + manual Poll Now
- **Data model**: Cameras/disks stored as JSON snapshot per device (not separate tables)

---

## 1. Architecture

**Backend** (FastAPI) — extend existing:
- New API endpoints for detailed info, history, tags, snapshots
- Background scheduler polls all active devices every 2 minutes
- Results saved to `device_health_log` + `devices.last_health_json`
- Alert engine (existing) generates alerts on problems

**Frontend** (React + Vite + MUI) — rebuild UI:
- Dashboard: overview cards, recent alerts, response time chart
- Device List: MUI DataGrid with tag filters, search, sort
- Device Detail: full info (system, cameras, disks, history, snapshots)
- Alerts: filterable alert feed

---

## 2. Database Changes

### New tables

**`device_tags`**
| Column | Type | Notes |
|--------|------|-------|
| id | Integer PK | auto-increment |
| device_id | String FK | -> devices |
| tag | String(100) | e.g. "Haifa", "Client-A" |
| | | UNIQUE(device_id, tag) |

**`device_health_log`**
| Column | Type | Notes |
|--------|------|-------|
| id | Integer PK | auto-increment |
| device_id | String FK | -> devices |
| reachable | Boolean | |
| camera_count | Integer | |
| online_cameras | Integer | |
| offline_cameras | Integer | |
| disk_ok | Boolean | |
| response_time_ms | Float | |
| checked_at | DateTime(tz) | INDEX(device_id, checked_at) |

Auto-cleanup: records older than 30 days.

### Changes to `devices` table

| Column | Type | Notes |
|--------|------|-------|
| model | String(255), nullable | filled from poll |
| serial_number | String(255), nullable | filled from poll |
| firmware_version | String(255), nullable | filled from poll |
| last_poll_at | DateTime(tz), nullable | last successful poll |
| last_health_json | JSON, nullable | full snapshot: {health, cameras, disks} |

---

## 3. Backend API

### Modified endpoints

- `GET /api/devices` — add model, serial, firmware, last_poll_at, tags[] to response. Query params: `?tag=X&search=Y`
- `GET /api/devices/{id}` — return full last_health_json (cameras, disks, health)
- `POST /api/devices/{id}/poll` — save result to last_health_json + device_health_log + update model/serial/firmware

### New endpoints

- `GET /api/devices/{id}/history?hours=24` — health_log entries for charts
- `POST /api/devices/{id}/tags` — add tag `{tag: "..."}`
- `DELETE /api/devices/{id}/tags/{tag}` — remove tag
- `GET /api/tags` — all unique tags (for filter dropdown)
- `GET /api/devices/{id}/snapshot/{channel_id}` — proxy snapshot from device
- `GET /api/alerts?status=active&device_id=...` — alerts with filtering

---

## 4. Background Scheduler

- APScheduler (already wired in main.py)
- Every **2 minutes**: poll all active devices sequentially
- Each poll: connect -> get_device_info + get_cameras + get_disks -> disconnect
- Update: `devices.last_health_json`, `last_poll_at`, `model/serial/firmware`
- Insert into `device_health_log`
- Run alert engine
- Log results via structlog

---

## 5. Frontend Pages (MUI)

### Dashboard (`/`)
- Summary cards: total devices, online, offline, active alerts
- Recent alerts table
- Response time mini-chart (MUI LineChart)

### Device List (`/devices`)
- MUI DataGrid: name, host, status, cameras (online/total), disks, response_time, last_poll
- Tag filter (MUI Chips), search by name/IP
- Actions: Poll Now, Edit, Delete

### Device Detail (`/devices/:id`)
- **Header**: name, model, serial, firmware, status badge
- **Connection Info**: host, ports, transport, tags (editable chips)
- **Health Summary**: reachable, response_time, last_poll_at
- **Cameras Tab**: channel cards with status icon (green/red/gray), name, IP, snapshot button
- **Disks Tab**: DataGrid — disk_id, capacity (GB), free (GB), used %, status, health
- **History Tab**: 24h charts — response_time, online cameras (MUI LineChart)
- **Alerts Tab**: device alerts list

### Add/Edit Device (`/devices/add`, `/devices/:id/edit`)
- MUI form with validation
- Tag management

### Alerts (`/alerts`)
- DataGrid with filters: status, severity, device

---

## 6. Tech Stack

- **Backend**: Python 3.14, FastAPI, SQLAlchemy (async), APScheduler, structlog
- **Frontend**: React 19, TypeScript, Vite, MUI 6, MUI X DataGrid, MUI X Charts
- **Database**: SQLite (existing, via aiosqlite)
- **SDK**: Hikvision HCNetSDK via ctypes (existing)
