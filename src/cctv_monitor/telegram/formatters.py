"""Text formatters for Telegram messages."""
from html import escape


def format_overview(payload: dict) -> str:
    total = payload.get("total_devices", 0)
    reachable = payload.get("reachable_devices", 0)
    unreachable = payload.get("unreachable_devices", 0)
    online_cameras = payload.get("online_cameras", 0)
    offline_cameras = payload.get("offline_cameras", 0)

    return (
        "<b>CCTV OVERVIEW</b>\n"
        f"Devices: <b>{reachable}</b>/<b>{total}</b> online, <b>{unreachable}</b> offline\n"
        f"Cameras: <b>{online_cameras}</b> online, <b>{offline_cameras}</b> offline"
    )


def format_alerts(alerts: list[dict]) -> str:
    if not alerts:
        return "<b>Active Alerts</b>\nNo active alerts."
    lines = ["<b>ACTIVE ALERTS</b>"]
    severity_icon = {
        "critical": "🔴",
        "warning": "🟠",
        "info": "🔵",
    }
    for i, alert in enumerate(alerts, start=1):
        severity_raw = str(alert.get("severity", "unknown")).lower()
        icon = severity_icon.get(severity_raw, "⚪")
        severity = escape(severity_raw.upper())
        device_name = escape(str(alert.get("device_name") or alert.get("device_id", "unknown-device")))
        message = escape(str(alert.get("message", "no-message")))
        lines.append(
            f"\n<b>{i}. {icon} {severity}</b>\n"
            f"<b>Device:</b> {device_name}\n"
            f"<b>Issue:</b> {message}"
        )
    return "\n".join(lines)


def format_device_detail(payload: dict) -> str:
    device = payload.get("device", {})
    health = payload.get("health", {}) or {}
    name = escape(str(device.get("name", "Unknown")))
    device_id = escape(str(device.get("device_id", "unknown")))
    reachable = health.get("reachable", False)
    online_cameras = health.get("online_cameras", 0)
    offline_cameras = health.get("offline_cameras", 0)
    disk_ok = health.get("disk_ok", False)
    return (
        f"<b>DEVICE</b> {name}\n"
        f"ID: <code>{device_id}</code>\n"
        f"Reachable: <b>{reachable}</b>\n"
        f"Cameras: <b>{online_cameras}</b> online, <b>{offline_cameras}</b> offline\n"
        f"Disk OK: <b>{disk_ok}</b>"
    )


def format_poll_result(payload: dict) -> str:
    health = payload.get("health", {}) or {}
    reachable = health.get("reachable", False)
    response_time = health.get("response_time_ms", 0)
    online_cameras = health.get("online_cameras", 0)
    offline_cameras = health.get("offline_cameras", 0)
    return (
        "<b>POLL COMPLETED</b>\n"
        f"Reachable: <b>{reachable}</b>\n"
        f"Response: <b>{response_time}</b> ms\n"
        f"Cameras: <b>{online_cameras}</b> online, <b>{offline_cameras}</b> offline"
    )


def format_devices(devices: list[dict], *, page: int, page_size: int) -> str:
    if not devices:
        return "<b>DEVICES</b>\nNo devices found."
    total = len(devices)
    total_pages = max(1, (total + page_size - 1) // page_size)
    page = max(0, min(page, total_pages - 1))
    start = page * page_size
    end = min(start + page_size, total)
    page_devices = devices[start:end]

    # Group by folder_path for display
    groups: dict[str, list[tuple[int, dict]]] = {}
    for i, d in enumerate(page_devices, start=start + 1):
        folder = d.get("folder_path") or ""
        groups.setdefault(folder, []).append((i, d))

    lines = [f"<b>DEVICES</b>  (page {page + 1}/{total_pages})"]

    # Sort: folders first (alphabetically), then no-folder
    sorted_keys = sorted(groups.keys(), key=lambda k: (k == "", k))
    for folder_key in sorted_keys:
        if folder_key:
            lines.append(f"\n📁 <b>{escape(folder_key)}</b>")
        elif len(sorted_keys) > 1:
            lines.append(f"\n📂 <b>No folder</b>")
        for i, d in groups[folder_key]:
            name = escape(str(d.get("name", "Unknown")))
            device_id = escape(str(d.get("device_id", "unknown")))
            lines.append(f"{i}. <b>{name}</b>\n   id: <code>{device_id}</code>")
    return "\n".join(lines)


def format_network_info(payload: dict) -> str:
    device = payload.get("device", {})
    name = escape(str(device.get("name", "Unknown")))
    device_id = escape(str(device.get("device_id", "unknown")))
    host = escape(str(device.get("host", "unknown")))
    web_port = device.get("web_port", "-")
    sdk_port = device.get("sdk_port", "-")
    username = escape(str(device.get("username", "(hidden)")))
    model = escape(str(device.get("model", "-")))
    serial = escape(str(device.get("serial_number", "-")))
    firmware = escape(str(device.get("firmware_version", "-")))
    return (
        f"<b>NETWORK INFO</b>\n"
        f"Device: <b>{name}</b>\n"
        f"ID: <code>{device_id}</code>\n"
        f"IP/Host: <code>{host}</code>\n"
        f"Web port: <code>{web_port}</code>\n"
        f"SDK port: <code>{sdk_port}</code>\n"
        f"Model: {model}\n"
        f"Serial: <code>{serial}</code>\n"
        f"Firmware: {firmware}\n"
        f"Username: <code>{username}</code>"
    )


def format_credentials(payload: dict) -> str:
    username = escape(str(payload.get("username", "")))
    password = escape(str(payload.get("password", "")))
    return (
        "<b>CREDENTIALS</b>\n"
        f"Username: <code>{username}</code>\n"
        f"Password: <code>{password}</code>"
    )


def format_disks(payload: dict) -> str:
    disks = payload.get("disks", []) or []
    if not disks:
        return "<b>DISKS</b>\nNo disk data."
    lines = ["<b>DISKS</b>"]
    for i, d in enumerate(disks, start=1):
        disk_id = escape(str(d.get("disk_id", str(i))))
        status = escape(str(d.get("status", "unknown")))
        smart = escape(str(d.get("smart_status", "n/a")))
        temp = escape(str(d.get("temperature", "n/a")))
        lines.append(f"{i}. Disk <code>{disk_id}</code>: <b>{status}</b>, SMART={smart}, Temp={temp}")
    return "\n".join(lines)


def format_channels(channels: list[dict], *, page: int, page_size: int) -> str:
    cameras = channels or []
    if not cameras:
        return "<b>CHANNELS</b>\nNo channels found."
    total = len(cameras)
    total_pages = max(1, (total + page_size - 1) // page_size)
    page = max(0, min(page, total_pages - 1))
    start = page * page_size
    end = min(start + page_size, total)

    lines = [f"<b>CHANNELS</b>  (page {page + 1}/{total_pages})"]
    for i, c in enumerate(cameras[start:end], start=start + 1):
        ch_id = escape(str(c.get("channel_id", str(i))))
        name = escape(str(c.get("channel_name", f"Channel {ch_id}")))
        status = escape(str(c.get("status", "unknown")))
        rec = escape(str(c.get("recording", "n/a")))
        ip_addr = escape(str(c.get("ip_address", "n/a")))
        lines.append(f"{i}. <b>{name}</b>\n   id=<code>{ch_id}</code>, status=<b>{status}</b>, rec={rec}, ip={ip_addr}")
    return "\n".join(lines)
