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


def format_devices(devices: list[dict]) -> str:
    if not devices:
        return "No devices found."
    lines = ["Devices:"]
    for i, d in enumerate(devices, start=1):
        name = d.get("name", "Unknown")
        device_id = d.get("device_id", "unknown")
        lines.append(f"{i}. {name} ({device_id})")
    return "\n".join(lines)


def format_network_info(payload: dict) -> str:
    device = payload.get("device", {})
    name = device.get("name", "Unknown")
    device_id = device.get("device_id", "unknown")
    host = device.get("host", "unknown")
    web_port = device.get("web_port", "-")
    sdk_port = device.get("sdk_port", "-")
    username = device.get("username", "(hidden)")
    model = device.get("model", "-")
    serial = device.get("serial_number", "-")
    firmware = device.get("firmware_version", "-")
    return (
        f"{name} ({device_id})\n"
        f"IP/Host: {host}\n"
        f"Web port: {web_port}\n"
        f"SDK port: {sdk_port}\n"
        f"Model: {model}\n"
        f"Serial: {serial}\n"
        f"Firmware: {firmware}\n"
        f"Username: {username}"
    )


def format_credentials(payload: dict) -> str:
    return (
        "Credentials:\n"
        f"Username: {payload.get('username', '')}\n"
        f"Password: {payload.get('password', '')}"
    )


def format_disks(payload: dict) -> str:
    disks = payload.get("disks", []) or []
    if not disks:
        return "No disk data."
    lines = ["Disks:"]
    for i, d in enumerate(disks, start=1):
        disk_id = d.get("disk_id", str(i))
        status = d.get("status", "unknown")
        smart = d.get("smart_status", "n/a")
        temp = d.get("temperature", "n/a")
        lines.append(f"{i}. Disk {disk_id}: {status}, SMART={smart}, Temp={temp}")
    return "\n".join(lines)


def format_channels(payload: dict) -> str:
    cameras = payload.get("cameras", []) or []
    if not cameras:
        return "No channels found."
    lines = ["Channels:"]
    for i, c in enumerate(cameras, start=1):
        ch_id = c.get("channel_id", str(i))
        name = c.get("channel_name", f"Channel {ch_id}")
        status = c.get("status", "unknown")
        rec = c.get("recording", "n/a")
        ip_addr = c.get("ip_address", "n/a")
        lines.append(f"{i}. {name} (id={ch_id}) - {status}, rec={rec}, ip={ip_addr}")
    return "\n".join(lines)
