# Hikvision SDK Transport Design

**Goal:** Add SDK-based transport for Hikvision NVR devices that lack HTTP/HTTPS (ISAPI) access, using the Device Network SDK via ctypes.

**Architecture:** ctypes wrapper over HCNetSDK native library (.dll/.so), hidden behind the existing `HikvisionTransport` abstraction. The `HikvisionDriver` remains unchanged — it works with any transport implementation.

**Tech Stack:** Python ctypes, asyncio.to_thread() for async wrapping of blocking SDK calls.

---

## Architecture

```
HikvisionDriver (unchanged)
    └── HikvisionTransport (ABC, unchanged)
            ├── IsapiTransport (HTTP/HTTPS, unchanged)
            └── SdkTransport (NEW — delegates to sdk_bindings)
                    └── sdk_bindings.py (NEW — ctypes wrapper)
```

## New Files

### 1. `sdk_bindings.py` — Low-level ctypes wrapper

Location: `src/cctv_monitor/drivers/hikvision/transports/sdk_bindings.py`

Responsibilities:
- Load `HCNetSDK.dll` (Windows) or `libhcnetsdk.so` (Linux) based on platform
- Library path from `HCNETSDK_LIB_PATH` env var or Settings
- Define ctypes structures:
  - `NET_DVR_USER_LOGIN_INFO`
  - `NET_DVR_DEVICEINFO_V40` (contains `NET_DVR_DEVICEINFO_V30`)
  - `NET_DVR_DEVICECFG_V40`
  - `NET_DVR_HDCFG` (contains `NET_DVR_SINGLE_HD` array)
- Expose synchronous functions:
  - `sdk_init()` / `sdk_cleanup()` — called once at app start/stop
  - `sdk_login(host, port, username, password) -> (user_id, device_info_dict)`
  - `sdk_logout(user_id)`
  - `sdk_get_device_config(user_id) -> dict`
  - `sdk_get_hdd_config(user_id) -> list[dict]`
  - `sdk_get_last_error() -> int`

### 2. `sdk.py` — SdkTransport (rewrite existing stub)

Uses `asyncio.to_thread()` to wrap blocking SDK calls:
- `connect()` → `sdk_login()`
- `disconnect()` → `sdk_logout()`
- `get_device_info()` → `sdk_get_device_config()` → returns dict
- `get_disk_status()` → `sdk_get_hdd_config()` → returns list[dict]
- `get_channels_status()` → channel info from login device_info
- `get_video_inputs()` → NotImplementedError (MVP)
- `get_recording_status()` → NotImplementedError (MVP)
- `get_snapshot()` → NotImplementedError (MVP)

## Modified Files

### 3. `main.py` — SDK lifecycle

- Call `sdk_init()` on startup (if SDK path configured)
- Call `sdk_cleanup()` on shutdown
- Store SDK availability on `app.state`

### 4. `devices.py` (poll endpoint) — Transport selection

Currently hardcoded to `IsapiTransport`. Change to select based on `device.transport_mode`:
- `"isapi"` → `IsapiTransport`
- `"sdk"` → `SdkTransport`
- `"auto"` → try ISAPI first, fall back to SDK

### 5. `config.py` (Settings) — SDK library path

Add `HCNETSDK_LIB_PATH: str | None = None` to Settings.

### 6. Frontend — transport_mode field

Add transport mode selector (ISAPI / SDK / Auto) to:
- `AddDevice.tsx`
- `EditDevice.tsx`
- `DeviceCreate` / `DeviceUpdate` TypeScript types

### 7. Backend schemas — transport_mode

Add `transport_mode` field to `DeviceCreate` and `DeviceUpdate` Pydantic schemas.

## Data Flow

SDK returns structured data (not XML). `SdkTransport` normalizes SDK responses to dict format compatible with existing mappers, or the mapper gets a small extension to handle SDK-format dicts.

## SDK Lifecycle

- `NET_DVR_Init()` — once at application startup
- Per-poll: `Login → GetDVRConfig → Logout` (stateless, like ISAPI)
- `NET_DVR_Cleanup()` — once at application shutdown

## Platform Support

Auto-detect OS at runtime:
- Windows: load `HCNetSDK.dll`
- Linux: load `libhcnetsdk.so`

Library path from `HCNETSDK_LIB_PATH` setting. If not set and SDK not found, `SdkTransport` raises a clear error.

## Error Handling

All SDK errors go through existing `SdkError` exception class. `sdk_get_last_error()` is called after any failed SDK function to get the error code.

## Testing

- Unit tests: mock `sdk_bindings` module entirely, test `SdkTransport` logic
- No integration tests in CI (requires physical NVR device)

## Out of Scope (MVP)

- Alarm/event subscription
- Live video streaming
- Snapshot via SDK
- Recording status via SDK
- Auto-transport failover (simple selection only for MVP)
