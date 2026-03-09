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


def format_alerts(alerts: list[dict]) -> str:
    if not alerts:
        return "No active alerts."
    lines = ["Active Alerts:"]
    for alert in alerts:
        severity = alert.get("severity", "unknown")
        device_name = alert.get("device_name") or alert.get("device_id", "unknown-device")
        message = alert.get("message", "no-message")
        lines.append(f"- [{severity}] {device_name}: {message}")
    return "\n".join(lines)


def format_device_detail(payload: dict) -> str:
    device = payload.get("device", {})
    health = payload.get("health", {}) or {}
    name = device.get("name", "Unknown")
    device_id = device.get("device_id", "unknown")
    reachable = health.get("reachable", False)
    online_cameras = health.get("online_cameras", 0)
    offline_cameras = health.get("offline_cameras", 0)
    disk_ok = health.get("disk_ok", False)
    return (
        f"Device {name} ({device_id})\n"
        f"Reachable: {reachable}\n"
        f"Cameras: {online_cameras} online, {offline_cameras} offline\n"
        f"Disk OK: {disk_ok}"
    )


def format_poll_result(payload: dict) -> str:
    health = payload.get("health", {}) or {}
    reachable = health.get("reachable", False)
    response_time = health.get("response_time_ms", 0)
    online_cameras = health.get("online_cameras", 0)
    offline_cameras = health.get("offline_cameras", 0)
    return (
        "Poll completed\n"
        f"Reachable: {reachable}\n"
        f"Response: {response_time} ms\n"
        f"Cameras: {online_cameras} online, {offline_cameras} offline"
    )
