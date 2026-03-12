# Paginated Snapshots Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Paginate snapshot loading so NVRs with 16+ channels load fast (only current page's channels fetched).

**Architecture:** New batch endpoint returns paginated snapshots (only requested page's channels sent to SDK subprocess). Frontend adds page navigation to camera grid. Existing single-snapshot endpoint unchanged.

**Tech Stack:** Python/FastAPI backend, React/MUI frontend, SDK subprocess batch capture.

---

### Task 1: Backend — paginated batch snapshots endpoint

**Files:**
- Modify: `src/cctv_monitor/api/routes/devices.py:1187` (add new endpoint before existing single snapshot)

**Step 1: Add the paginated snapshots endpoint**

Add this endpoint above the existing `get_snapshot` at line 1187:

```python
@router.get("/devices/{device_id}/snapshots")
async def get_snapshots_page(
    device_id: str,
    page: int = Query(1, ge=1),
    page_size: int = Query(16, ge=1, le=32),
    session: AsyncSession = Depends(get_session),
    settings: Settings = Depends(get_settings),
):
    """Return base64 JPEG snapshots for one page of channels."""
    import base64 as _b64

    repo = DeviceRepository(session)
    device = await repo.get_by_id(device_id)
    if device is None:
        raise HTTPException(status_code=404, detail="Device not found")

    # Build channel list (excluding ignored)
    all_channels = _get_channel_ids_from_health(device.last_health_json)
    ignored = set(device.ignored_channels or [])
    channels = [ch for ch in all_channels if ch not in ignored]

    total = len(channels)
    total_pages = max(1, (total + page_size - 1) // page_size)
    page = min(page, total_pages)
    start = (page - 1) * page_size
    page_channels = channels[start : start + page_size]

    if not page_channels:
        return {
            "snapshots": {},
            "total": total,
            "page": page,
            "page_size": page_size,
            "pages": total_pages,
        }

    password = decrypt_value(device.password_encrypted, settings.ENCRYPTION_KEY)
    sdk_available = bool(device.sdk_port and settings.HCNETSDK_LIB_PATH)
    snapshots: dict[str, str | None] = {}

    semaphore = _get_snapshot_semaphore(device_id)
    async with semaphore:
        if sdk_available:
            results, errors = await _sdk_batch_snapshot_subprocess(
                device.host, device.sdk_port, device.username, password,
                page_channels, settings.HCNETSDK_LIB_PATH,
            )
            for ch in page_channels:
                img = results.get(ch)
                snapshots[ch] = _b64.b64encode(img).decode() if img else None
        else:
            # ISAPI fallback — fetch one by one (already fast via HTTP)
            from .devices import _isapi_snapshot  # noqa: will be defined below
            for ch in page_channels:
                snapshots[ch] = None  # placeholder — ISAPI path handled by existing single endpoint

    return {
        "snapshots": snapshots,
        "total": total,
        "page": page,
        "page_size": page_size,
        "pages": total_pages,
    }
```

**Important:** Add `Query` to FastAPI imports at the top of the file if not already present.

**Step 2: Run the server to verify no import errors**

Run: `cd src && python -m cctv_monitor.main` (Ctrl+C after startup)
Expected: Server starts without import errors.

**Step 3: Commit**

```bash
git add src/cctv_monitor/api/routes/devices.py
git commit -m "feat: add paginated batch snapshots endpoint"
```

---

### Task 2: Frontend — add pagination state to DeviceDetail

**Files:**
- Modify: `frontend/src/pages/DeviceDetail.tsx:265-736`

**Step 1: Add pagination state variables**

Inside the `DeviceDetail` component (after existing state declarations around line 280), add:

```typescript
const SNAPSHOT_PAGE_SIZE = 16;
const [snapshotPage, setSnapshotPage] = useState(1);
```

Reset page when device changes — add to the `useEffect` that fetches device detail:

```typescript
setSnapshotPage(1);
```

**Step 2: Compute page slice of cameras**

Before the cameras grid (around line 705), compute the visible cameras:

```typescript
const visibleCameras = cameras.filter(c => !ignoredChannels.has(c.channel_id));
const totalSnapshotPages = Math.max(1, Math.ceil(visibleCameras.length / SNAPSHOT_PAGE_SIZE));
const pagedCameras = visibleCameras.slice(
  (snapshotPage - 1) * SNAPSHOT_PAGE_SIZE,
  snapshotPage * SNAPSHOT_PAGE_SIZE,
);
```

**Step 3: Replace cameras.map with pagedCameras.map**

Change line 722 from:
```typescript
{cameras.map((c) => (
```
to:
```typescript
{pagedCameras.map((c) => (
```

**Step 4: Add pagination controls after the grid**

After the closing `</Box>` of the grid (line 733), add:

```tsx
{totalSnapshotPages > 1 && (
  <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 2, mt: 2 }}>
    <Button
      size="small"
      disabled={snapshotPage <= 1}
      onClick={() => setSnapshotPage(p => p - 1)}
    >
      {t('common.back', '← Back')}
    </Button>
    <Typography variant="body2">
      {snapshotPage} / {totalSnapshotPages}
    </Typography>
    <Button
      size="small"
      disabled={snapshotPage >= totalSnapshotPages}
      onClick={() => setSnapshotPage(p => p + 1)}
    >
      {t('common.next', 'Next →')}
    </Button>
  </Box>
)}
```

Add `Button` to MUI imports if not already imported.

**Step 5: Build frontend to verify**

Run: `cd frontend && npm run build`
Expected: Build succeeds.

**Step 6: Commit**

```bash
git add frontend/src/pages/DeviceDetail.tsx
git commit -m "feat: paginate camera snapshots grid (16 per page)"
```

---

### Task 3: Frontend — update camera tab label to show page info

**Files:**
- Modify: `frontend/src/pages/DeviceDetail.tsx:697`

**Step 1: Update tab label**

The cameras tab label at line 697 already shows total count. No further changes needed — the count stays correct since `cameras.length` is the full list. The pagination only affects rendering.

This task is a no-op — skip to Task 4.

---

### Task 4: Test manually

**Step 1: Start backend**

Run: `cd src && python -m cctv_monitor.main`

**Step 2: Start frontend**

Run: `cd frontend && npm run dev`

**Step 3: Verify**

1. Open a device with 16+ channels
2. Confirm only 16 snapshots shown on first page
3. Click "Next" — next batch loads
4. Click "Back" — previous batch shows (from browser cache / re-fetch)
5. Open device with <16 channels — no pagination buttons shown

**Step 4: Final commit if any adjustments needed**

```bash
git add -A
git commit -m "fix: adjustments from manual testing"
```
