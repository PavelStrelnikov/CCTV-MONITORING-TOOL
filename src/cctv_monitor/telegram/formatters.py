"""Text formatters for Telegram messages."""


def format_overview(payload: dict) -> str:
    total = payload.get("total_devices", 0)
    reachable = payload.get("reachable_devices", 0)
    unreachable = payload.get("unreachable_devices", 0)
    online_cameras = payload.get("online_cameras", 0)
    offline_cameras = payload.get("offline_cameras", 0)

    return (
        "CCTV Overview\n"
        f"Devices: {reachable}/{total} online, {unreachable} offline\n"
        f"Cameras: {online_cameras} online, {offline_cameras} offline"
    )
