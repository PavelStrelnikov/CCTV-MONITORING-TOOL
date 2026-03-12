from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession

from cctv_monitor.api.deps import get_session
from cctv_monitor.api.schemas import OverviewOut, OverviewDeviceSummary
from cctv_monitor.storage.repositories import DeviceRepository

router = APIRouter(tags=["status"])


@router.get("/overview", response_model=OverviewOut)
async def get_overview(session: AsyncSession = Depends(get_session)):
    repo = DeviceRepository(session)
    devices = await repo.list_all()

    total_devices = len(devices)
    reachable = 0
    total_cameras = 0
    online_cameras = 0
    offline_cameras = 0
    total_disks = 0
    disks_ok_count = 0
    disks_error_count = 0
    recording_total = 0
    recording_ok = 0
    time_drift_issues = 0
    device_summaries: list[OverviewDeviceSummary] = []

    for d in devices:
        cached = d.last_health_json or {}
        health = cached.get("health", {})
        cameras_data = cached.get("cameras", [])
        disks_data = cached.get("disks", [])
        ignored = set(d.ignored_channels or [])

        is_reachable = health.get("reachable", False)
        if is_reachable:
            reachable += 1

        # Cameras (exclude ignored channels from counts)
        monitored_cameras = [
            c for c in cameras_data
            if c.get("channel_id") not in ignored
        ]
        dev_cam_count = len(monitored_cameras)

        if is_reachable:
            # Only count camera/disk/recording stats for reachable devices
            dev_online = sum(
                1 for c in monitored_cameras
                if c.get("status", "").lower() == "online"
            )
            dev_offline = dev_cam_count - dev_online
        else:
            # Unreachable — we can't know actual camera/disk state
            dev_online = 0
            dev_offline = 0

        total_cameras += dev_cam_count
        online_cameras += dev_online
        offline_cameras += dev_offline

        # Disks
        dev_disk_count = len(disks_data)
        if is_reachable:
            dev_disks_ok = sum(
                1 for dk in disks_data
                if dk.get("status", "").lower() in ("ok", "normal")
            )
            dev_disks_err = dev_disk_count - dev_disks_ok
        else:
            # Unreachable — don't report disk errors from stale data
            dev_disks_ok = 0
            dev_disks_err = 0

        total_disks += dev_disk_count
        disks_ok_count += dev_disks_ok
        disks_error_count += dev_disks_err

        # Recordings (exclude ignored channels)
        dev_rec_total = 0
        dev_rec_ok = 0
        if is_reachable:
            for c in monitored_cameras:
                rec = c.get("recording")
                if rec is not None:
                    dev_rec_total += 1
                    if rec == "recording":
                        dev_rec_ok += 1
        recording_total += dev_rec_total
        recording_ok += dev_rec_ok

        # Time drift
        time_check = health.get("time_check")
        dev_drift: int | None = None
        if time_check and time_check.get("drift_seconds") is not None:
            dev_drift = time_check["drift_seconds"]
            if is_reachable and abs(dev_drift) > 300:
                time_drift_issues += 1

        # For unreachable devices: disk_ok = None (unknown), not False
        if is_reachable:
            dev_disk_ok = dev_disks_err == 0 if dev_disk_count > 0 else True
        else:
            dev_disk_ok = True  # unknown — don't flag as error

        device_summaries.append(OverviewDeviceSummary(
            device_id=d.device_id,
            name=d.name,
            reachable=is_reachable,
            camera_count=dev_cam_count,
            online_cameras=dev_online,
            offline_cameras=dev_offline,
            disk_ok=dev_disk_ok,
            recording_total=dev_rec_total,
            recording_ok=dev_rec_ok,
            time_drift=dev_drift,
            last_poll_at=d.last_poll_at,
        ))

    return OverviewOut(
        total_devices=total_devices,
        reachable_devices=reachable,
        unreachable_devices=total_devices - reachable,
        total_cameras=total_cameras,
        online_cameras=online_cameras,
        offline_cameras=offline_cameras,
        total_disks=total_disks,
        disks_ok_count=disks_ok_count,
        disks_error_count=disks_error_count,
        disks_ok=disks_error_count == 0,
        recording_total=recording_total,
        recording_ok=recording_ok,
        time_drift_issues=time_drift_issues,
        devices=device_summaries,
    )
