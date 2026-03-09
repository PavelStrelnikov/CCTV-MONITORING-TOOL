# CCTV Monitoring + Telegram - Technical Design and Implementation Plan

Author: Codex  
Date: 2026-03-09  
Status: Proposed

## 1. Goal

Add a Telegram interface for the CCTV Monitoring platform so operators can:
- request current status and reports,
- receive critical alerts,
- trigger selected actions (for example, poll a device),
- work without direct access to the web UI.

## 2. Current System Baseline

Backend already has useful endpoints:
- `GET /api/overview`
- `GET /api/alerts`
- `GET /api/poll-logs`
- `GET /api/devices`
- `GET /api/devices/{device_id}`
- `POST /api/devices/{device_id}/poll`
- `GET /api/devices/{device_id}/history`

Conclusion: Telegram bot should integrate through existing API first, not direct DB access.

## 3. Architecture Decision

Use a separate bot service (`telegram-bot`) that talks to backend API.

### 3.1 Why this approach

- Lower risk: bot does not hold DB credentials.
- Better separation: API remains system-of-record boundary.
- Easier rollout: can deploy/rollback bot independently.
- Future-ready: later can add Telegram WebApp without redesign.

### 3.2 Target topology

1. User sends command in Telegram.
2. Bot validates user access.
3. Bot calls internal API endpoint.
4. API returns normalized response.
5. Bot formats and sends message.

For notifications:
1. Alert appears in backend.
2. Notification worker finds unsent critical events.
3. Worker sends Telegram messages to subscribed users/chats.
4. Worker marks delivery status.

## 4. Scope

### 4.1 MVP in scope

- AuthZ by Telegram user ID / chat ID allowlist.
- Commands:
  - `/start`
  - `/help`
  - `/overview`
  - `/alerts`
  - `/device <device_id>`
  - `/poll <device_id>`
  - `/report day`
  - `/report week`
  - `/subscribe critical`
  - `/unsubscribe critical`
- Daily summary push (scheduled).
- Critical alert push (near real-time, with dedup/cooldown).
- Audit log of bot commands.

### 4.2 Out of scope (phase 2+)

- Multi-tenant isolation by customer.
- Inline camera snapshots in Telegram.
- Rich workflow UI (Telegram Mini App/WebApp).
- Two-way incident management/ticketing.

## 5. Security Design

### 5.1 Authentication and authorization

- Bot token from env: `TELEGRAM_BOT_TOKEN`.
- Every request mapped to Telegram `user_id` and `chat_id`.
- Access only for allowlisted records in DB.
- Role model for future:
  - `viewer`: read-only commands.
  - `operator`: can run `/poll`.
  - `admin`: subscription and settings commands.

### 5.2 API-to-bot trust

- Bot uses internal API token in header `X-Internal-Token`.
- Backend validates token for bot-only endpoints.
- Restrict bot service network access to internal backend host only.

### 5.3 Data protection

- Do not return device credentials in any Telegram flow.
- Redact sensitive fields in logs.
- Store only required Telegram metadata.

## 6. Data Model Changes

Add migrations:

1. `telegram_users`
- `id` (pk)
- `telegram_user_id` (unique, bigint)
- `username` (nullable)
- `display_name` (nullable)
- `role` (`viewer|operator|admin`)
- `is_active` (bool)
- `created_at`, `updated_at`

2. `telegram_chats`
- `id` (pk)
- `telegram_chat_id` (unique, bigint)
- `chat_type` (`private|group|supergroup|channel`)
- `title` (nullable)
- `is_active` (bool)
- `created_at`, `updated_at`

3. `telegram_subscriptions`
- `id` (pk)
- `telegram_user_id` (fk -> telegram_users.telegram_user_id)
- `subscription_type` (`critical_alerts|daily_report`)
- `is_enabled` (bool)
- `schedule_cron` (nullable)
- `timezone` (default `Asia/Jerusalem`)
- unique `(telegram_user_id, subscription_type)`

4. `telegram_audit_log`
- `id` (pk)
- `telegram_user_id`
- `telegram_chat_id`
- `command`
- `args_json`
- `status` (`ok|denied|error`)
- `error_message` (nullable)
- `created_at`

5. `telegram_delivery_log`
- `id` (pk)
- `alert_id` (nullable)
- `telegram_user_id`
- `message_type` (`critical_alert|daily_report`)
- `dedup_key`
- `status` (`sent|failed`)
- `error_message` (nullable)
- `created_at`

## 7. API Extensions

Keep current endpoints. Add only what Telegram needs and cannot get efficiently.

### 7.1 New backend routes

- `GET /api/reports/summary?period=day|week`
  - returns aggregate health/alerts stats.
- `GET /api/telegram/subscriptions/{telegram_user_id}`
- `PUT /api/telegram/subscriptions/{telegram_user_id}`
- `POST /api/telegram/notify/test` (admin-only internal testing)

### 7.2 Service contracts

- Use stable response DTOs in `api/schemas.py`.
- Keep report payload small and text-friendly for Telegram formatting.

## 8. Bot Service Design

New package:
- `src/cctv_monitor/telegram/`
  - `main.py` (entrypoint)
  - `bot.py` (router/dispatcher setup)
  - `handlers.py` (command handlers)
  - `auth.py` (allowlist + role checks)
  - `api_client.py` (calls backend API)
  - `formatters.py` (Telegram text output)
  - `scheduler.py` (daily report job)
  - `notifier.py` (critical alert push worker)
  - `repositories.py` (telegram-specific DB access if needed)

### 8.1 Library choice

Recommended: `aiogram` (async-native, good fit for FastAPI async stack).

### 8.2 Runtime mode

- MVP: long polling (simple setup, no public webhook required).
- Later: webhook mode behind reverse proxy.

## 9. Command Behavior Specification

1. `/overview`
- Calls `GET /api/overview`.
- Returns counts: devices online/offline, cameras online/offline, disk issues.

2. `/alerts`
- Calls `GET /api/alerts?status=active` (or current API status model).
- Returns latest active alerts (limit top N in message, for example 10).

3. `/device <device_id>`
- Calls `GET /api/devices/{device_id}`.
- Returns reachability, camera summary, disk summary, last poll.

4. `/poll <device_id>`
- Role: `operator` or `admin`.
- Calls `POST /api/devices/{device_id}/poll`.
- Returns polling result and response time.

5. `/report day|week`
- Calls new report summary endpoint.
- Returns compact KPI block.

6. `/subscribe critical` and `/unsubscribe critical`
- Updates `telegram_subscriptions`.

## 10. Delivery and Dedup Rules

For critical alerts:
- dedup key = `device_id + alert_type + status`.
- cooldown: suppress duplicates for 10 minutes.
- if alert resolved, optionally send one "resolved" message.

For daily report:
- default time: 08:00 local timezone.
- one message per user with `daily_report` enabled.

## 11. Phased Implementation Plan

## Phase 1 - Foundations (1-2 days)

1. Add dependencies (`aiogram`) to `pyproject.toml`.
2. Add config vars:
   - `TELEGRAM_BOT_TOKEN`
   - `INTERNAL_API_BASE_URL`
   - `INTERNAL_API_TOKEN`
   - `TELEGRAM_DEFAULT_TIMEZONE`
3. Add DB migrations for Telegram tables.
4. Add repository layer for telegram allowlist/subscriptions/audit.

Acceptance:
- migrations apply cleanly,
- unit tests for repositories pass.

## Phase 2 - Read-Only Commands (2-3 days)

1. Implement bot bootstrap and dispatcher.
2. Implement auth guard for allowlisted users.
3. Implement `/start`, `/help`, `/overview`, `/alerts`, `/device`.
4. Add command audit logging.

Acceptance:
- allowlisted user can run commands,
- non-allowlisted user gets denied message,
- command errors are handled and logged.

## Phase 3 - Action + Reports (2 days)

1. Implement `/poll <device_id>` with role checks.
2. Add `GET /api/reports/summary`.
3. Implement `/report day|week`.
4. Add formatter tests.

Acceptance:
- poll command works end-to-end,
- report command returns correct aggregates.

## Phase 4 - Push Notifications (2-3 days)

1. Implement critical notifier worker.
2. Implement `/subscribe` and `/unsubscribe`.
3. Implement daily scheduled report sender.
4. Add dedup/cooldown logic + delivery logs.

Acceptance:
- new critical alert triggers one message per subscribed user,
- duplicate alerts inside cooldown are suppressed,
- daily report sent at scheduled time.

## Phase 5 - Production Hardening (1-2 days)

1. Add retry and timeout policies to bot API client.
2. Add structured logs and basic metrics counters.
3. Add docker-compose service for bot.
4. Add runbook in docs.

Acceptance:
- bot restarts safely,
- health checks available,
- operational docs ready.

## 12. Testing Strategy

Unit tests:
- command parsing,
- formatter output,
- auth/role checks,
- dedup logic.

Integration tests:
- bot handler -> API client mock -> output text,
- notifier worker -> delivery logs.

Smoke tests:
- `/overview`,
- `/alerts`,
- `/poll <id>`,
- daily report schedule trigger.

## 13. Deployment Plan

Add new service in `docker-compose.yml`:
- `cctv_monitoring_api` (existing)
- `cctv_monitoring_bot` (new)
- `cctv_monitoring_postgres` (existing)

Bot startup command:
- `python -m cctv_monitor.telegram.main`

Required env:
- bot token,
- internal API URL/token,
- DB credentials.

## 14. Risks and Mitigations

1. Risk: alert spam in busy incidents.
- Mitigation: dedup key + cooldown + top-N cap in message body.

2. Risk: unauthorized access from unknown Telegram users.
- Mitigation: strict allowlist and denied-by-default policy.

3. Risk: bot/API downtime coupling.
- Mitigation: separate process, retries, graceful degradation messages.

4. Risk: timezone mistakes for reports.
- Mitigation: explicit per-user timezone, default `Asia/Jerusalem`, log schedule calculations.

## 15. Definition of Done (MVP)

- Allowlisted operators can use Telegram commands for status and reports.
- Critical alerts are pushed automatically with dedup.
- Command actions are audited.
- New migrations and tests are merged.
- Runbook explains setup, rollout, rollback.

## 16. First Execution Checklist (for a beginner)

1. Create migrations for Telegram tables.
2. Add config values to `.env.example`.
3. Add `telegram` package skeleton under `src/cctv_monitor/`.
4. Implement `/start`, `/help`, `/overview`.
5. Run tests and manual smoke checks.
6. Add `/alerts` and `/device`.
7. Add `/poll` with role guard.
8. Implement report endpoint and `/report`.
9. Implement subscriptions + notifier worker.
10. Final pass: docs + docker-compose + monitoring.
