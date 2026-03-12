# Paginated Snapshots for Large NVRs

## Problem

NVRs with 16+ channels cause long wait times when loading snapshots.
The SDK batch subprocess captures ALL channels at once (up to 2 minutes for 32 channels),
blocking the UI and Telegram bot.

## Solution

Paginate snapshot loading — request only the channels for the current page.

## Backend

### New endpoint: `GET /api/devices/{id}/snapshots?page=1&page_size=16`

- Reads channel list from `last_health_json["cameras"]` (excluding ignored)
- Computes slice for requested page
- Passes only those channel IDs to SDK batch subprocess
- Returns JSON: `{ "snapshots": { "<ch_id>": "<base64>" }, "total": N, "page": P, "page_size": S, "pages": T }`

### Batch cache

- Cache key includes the set of channel IDs (not just device ID)
- TTL stays at 25 seconds
- Timeout: `max(30, min(120, 5 * page_size))` — 16 channels = 80s max

### Existing endpoints

- `GET /api/devices/{id}/snapshot/{channel_id}` — unchanged (single snapshot)

## Frontend

### Snapshot grid with pagination

- Display up to 16 snapshots per page
- Navigation: "Back" / "Next" buttons + "Page X of Y" indicator
- Skeleton loaders while batch loads
- Page state resets on device change

## Telegram

No changes needed — already paginates channels (8 per page) and requests
snapshots one at a time via single endpoint.

## Out of scope

- Auto-poll optimization
- SDK worker internals (just receives fewer channels)
- Single snapshot endpoint changes
