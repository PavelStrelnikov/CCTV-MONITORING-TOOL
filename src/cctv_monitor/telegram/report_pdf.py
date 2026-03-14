"""PDF report builder for Telegram poll reports via Playwright (RTL-friendly)."""

from __future__ import annotations

import html
import multiprocessing
import os
import tempfile
from datetime import datetime
from typing import Any

from jinja2 import Template


REPORT_TEMPLATE = Template(
    """
<!doctype html>
<html lang="he" dir="rtl">
<head>
  <meta charset="UTF-8" />
  <style>
    * { box-sizing: border-box; }
    body { margin:0; font-family:"Arial","Segoe UI","Noto Sans Hebrew",sans-serif; color:#0f172a; background:#fff; direction:rtl; }
    .wrap { padding:18px 20px; }
    .title { font-size:28px; font-weight:800; margin:0 0 6px 0; }
    .meta { color:#334155; margin:0 0 3px 0; font-size:13px; }
    .summary { margin:10px 0 6px 0; font-size:16px; }
    .summary .ok { color:#166534; font-weight:700; }
    .summary .warn { color:#b45309; font-weight:700; }
    .summary .crit { color:#b91c1c; font-weight:700; }
    .reason { color:#334155; margin:0 0 14px 0; font-size:13px; }
    .section { margin-top:16px; }
    .section h2 { margin:0 0 8px 0; font-size:18px; border-bottom:1px solid #dbe2ea; padding-bottom:4px; }
    table { width:100%; border-collapse:collapse; table-layout:fixed; font-size:14px; }
    th, td { border:1px solid #dbe2ea; padding:8px 10px; text-align:right; vertical-align:top; word-wrap:break-word; }
    th { background:#f1f5f9; width:34%; color:#1e293b; font-weight:700; }
    .head-row th { background:#e6edf5; width:auto; }
    .danger th, .danger td { background:#fff1f2; }
    .info th, .info td { background:#eff6ff; }
    .muted { color:#64748b; }
    .footer { margin-top:18px; font-size:11px; color:#64748b; }
  </style>
</head>
<body>
  <div class="wrap">
    <h1 class="title">דו"ח בדיקת מערכת CCTV</h1>
    <p class="meta">שעת הפקה: {{ generated_at }}</p>
    {% if client_path %}<p class="meta">לקוח / תיקיה: {{ client_path }}</p>{% endif %}
    {% if device_name %}<p class="meta">מכשיר: {{ device_name }}</p>{% endif %}
    <p class="summary">
      סיכום:
      {% if severity == "ok" %}<span class="ok">תקין</span>{% endif %}
      {% if severity == "warn" %}<span class="warn">אזהרה</span>{% endif %}
      {% if severity == "crit" %}<span class="crit">קריטי</span>{% endif %}
    </p>
    <p class="reason">סיבת סיכום: {{ severity_reason }}</p>

    <div class="section">
      <h2>פרטי מכשיר</h2>
      <table>{% for row in device_rows %}<tr><th>{{ row[0] }}</th><td>{{ row[1] }}</td></tr>{% endfor %}</table>
    </div>

    <div class="section">
      <h2>תוצאות בדיקה</h2>
      <table>{% for row in health_rows %}<tr><th>{{ row[0] }}</th><td>{{ row[1] }}</td></tr>{% endfor %}</table>
    </div>

    <div class="section">
      <h2>הקלטה</h2>
      <table>{% for row in recording_rows %}<tr><th>{{ row[0] }}</th><td>{{ row[1] }}</td></tr>{% endfor %}</table>
      {% if recording_problem_rows %}
      <table class="danger" style="margin-top:8px;">
        <tr class="head-row"><th>ערוץ</th><th>שם</th><th>סטטוס הקלטה</th></tr>
        {% for row in recording_problem_rows %}<tr><td>{{ row[0] }}</td><td>{{ row[1] }}</td><td>{{ row[2] }}</td></tr>{% endfor %}
      </table>
      {% endif %}
    </div>

    <div class="section">
      <h2>מצלמות</h2>
      {% if camera_rows %}
      <table class="danger">
        <tr class="head-row"><th>ערוץ</th><th>שם</th><th>סטטוס</th></tr>
        {% for row in camera_rows %}<tr><td>{{ row[0] }}</td><td>{{ row[1] }}</td><td>{{ row[2] }}</td></tr>{% endfor %}
      </table>
      {% else %}
      <p class="muted">כל המצלמות תקינות.</p>
      {% endif %}
    </div>

    <div class="section">
      <h2>דיסקים</h2>
      {% if disk_rows %}
      <table class="info">
        <tr class="head-row"><th>דיסק</th><th>סטטוס</th><th>SMART</th><th>טמפרטורה</th><th>נפח</th><th>שעות עבודה</th></tr>
        {% for row in disk_rows %}<tr><td>{{ row[0] }}</td><td>{{ row[1] }}</td><td>{{ row[2] }}</td><td>{{ row[3] }}</td><td>{{ row[4] }}</td><td>{{ row[5] }}</td></tr>{% endfor %}
      </table>
      {% else %}
      <p class="muted">אין נתוני דיסקים זמינים.</p>
      {% endif %}
    </div>

    <p class="footer">הדו"ח הופק אוטומטית על ידי מערכת הניטור.</p>
  </div>
</body>
</html>
"""
)


def _safe_filename(raw: str) -> str:
    allowed = set("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_")
    value = "".join(ch if ch in allowed else "_" for ch in raw)
    value = value.strip("_")
    return value or "device"


def _value(raw: Any, default: str = "-") -> str:
    if raw is None:
        return default
    text = str(raw).strip()
    return text if text else default


def _bool_he(raw: Any) -> str:
    return "כן" if bool(raw) else "לא"


def _status_he(raw: Any) -> str:
    status = str(raw or "").strip().lower()
    mapping = {
        "ok": "תקין",
        "online": "מקוון",
        "offline": "לא זמין",
        "warning": "אזהרה",
        "error": "שגיאה",
        "unknown": "לא ידוע",
    }
    return mapping.get(status, _value(raw))


def _recording_ok(raw: Any) -> bool:
    return str(raw or "").strip().lower() in ("on", "ok", "recording", "true", "yes", "1")


def _recording_he(raw: Any) -> str:
    return "מקליט" if _recording_ok(raw) else "לא מקליט"


def _disk_row_problem(disk: dict) -> bool:
    status = str(disk.get("status", "")).strip().lower()
    smart = str(disk.get("smart_status", "")).strip().lower()
    status_bad = status not in ("", "ok", "online", "normal", "healthy")
    smart_bad = smart not in ("", "-", "ok", "normal", "healthy", "passed")
    return status_bad or smart_bad


def _short_device_name(raw_name: Any) -> str:
    name = _value(raw_name)
    if "/" in name:
        return name.split("/")[-1].strip() or name
    return name


def _bytes_to_human(raw: Any) -> str:
    try:
        size = float(raw)
    except (TypeError, ValueError):
        return "-"
    units = ["B", "KB", "MB", "GB", "TB", "PB"]
    idx = 0
    while size >= 1024 and idx < len(units) - 1:
        size /= 1024
        idx += 1
    return f"{size:.1f} {units[idx]}" if idx > 0 else f"{int(size)} {units[idx]}"


def _format_power_on_hours(raw: Any) -> str:
    try:
        total = int(float(raw))
    except (TypeError, ValueError):
        return "-"
    years = total // (24 * 365)
    rem = total % (24 * 365)
    months = rem // (24 * 30)
    rem %= (24 * 30)
    days = rem // 24
    parts: list[str] = []
    if years:
        parts.append(f"{years} שנים")
    if months:
        parts.append(f"{months} חודשים")
    if days and len(parts) < 2:
        parts.append(f"{days} ימים")
    return " ".join(parts) if parts else "פחות מיום"


def _port_value(port: Any, is_open: Any) -> str | None:
    text = _value(port, "")
    if not text:
        return None
    if is_open is False:
        return None
    return f"🟢 {text}" if is_open is True else text


def build_report_filename(device: dict) -> str:
    device_id = _safe_filename(_value(device.get("device_id"), "device"))
    stamp = datetime.now().strftime("%Y%m%d_%H%M")
    return f"poll_report_{device_id}_{stamp}.pdf"


def _render_pdf_in_subprocess(html_path: str, pdf_path: str) -> None:
    from playwright.sync_api import sync_playwright

    pw = sync_playwright().start()
    browser = pw.chromium.launch()
    page = browser.new_page()
    try:
        with open(html_path, "r", encoding="utf-8") as f:
            content = f.read()
        page.set_content(content, wait_until="networkidle")
        page.pdf(path=pdf_path, format="A4", margin={"top": "12mm", "right": "12mm", "bottom": "12mm", "left": "12mm"}, print_background=True)
    finally:
        page.close()
        browser.close()
        pw.stop()


def _run_process(proc: multiprocessing.Process) -> None:
    proc.start()
    proc.join(timeout=60)
    if proc.exitcode != 0:
        raise RuntimeError(f"PDF generation failed (exit code {proc.exitcode})")


def build_device_poll_report_pdf(device_payload: dict, poll_payload: dict) -> bytes:
    device = device_payload.get("device", {}) or {}
    cameras = device_payload.get("cameras", []) or []
    disks = device_payload.get("disks", []) or []
    poll_health = poll_payload.get("health", {}) or {}
    detail_health = device_payload.get("health", {}) or {}
    health = detail_health.copy()
    health.update({k: v for k, v in poll_health.items() if v not in (None, "")})

    ignored = {str(ch) for ch in (device.get("ignored_channels", []) or [])}
    active_cameras = [c for c in cameras if str(c.get("channel_id", "")) not in ignored]
    total_cameras = len(active_cameras) or int(health.get("camera_count", 0) or 0)
    online_cameras = sum(1 for c in active_cameras if str(c.get("status", "")).lower() in ("ok", "online"))
    if online_cameras == 0 and total_cameras > 0:
        online_cameras = int(health.get("online_cameras", 0) or 0)
    offline_cameras = max(0, total_cameras - online_cameras)

    reachable = bool(health.get("reachable", False))
    disk_ok_effective = (len([d for d in disks if _disk_row_problem(d)]) == 0) if disks else bool(health.get("disk_ok", False))
    non_recording = [c for c in active_cameras if not _recording_ok(c.get("recording"))]

    severity = "ok"
    reasons: list[str] = []
    if reachable is False and total_cameras == 0:
        severity = "crit"
        reasons.append("המכשיר לא זמין")
    if offline_cameras > 0:
        if severity != "crit":
            severity = "warn"
        reasons.append(f"{offline_cameras} מצלמות לא זמינות")
    if not disk_ok_effective:
        if severity != "crit":
            severity = "warn"
        reasons.append("זוהתה בעיית דיסק/SMART")
    if non_recording:
        if severity != "crit":
            severity = "warn"
        reasons.append(f"{len(non_recording)} ערוצים ללא הקלטה")
    severity_reason = ", ".join(reasons) if reasons else "אין חריגות"

    device_rows = [
        ("שם מכשיר", _short_device_name(device.get("name"))),
        ("מספר סידורי", _value(device.get("serial_number"))),
        ("ספק", _value(device.get("vendor"))),
        ("כתובת / IP", _value(device.get("host"))),
    ]
    web_port = _port_value(device.get("web_port"), health.get("web_port_open"))
    sdk_port = _port_value(device.get("sdk_port"), health.get("sdk_port_open"))
    if web_port is not None:
        device_rows.append(("פורט WEB", web_port))
    if sdk_port is not None:
        device_rows.append(("פורט SDK", sdk_port))

    health_rows = [
        ("מצלמות (אונליין/סה\"כ)", f"{online_cameras}/{total_cameras}"),
        ("תקינות דיסק", _bool_he(disk_ok_effective)),
    ]
    recording_rows = [("ערוצים מקליטים (תקין/סה\"כ)", f"{total_cameras - len(non_recording)}/{total_cameras}" if total_cameras > 0 else "-")]
    recording_rows.append(("מצב", "כל הערוצים מקליטים" if not non_recording else "נמצאו ערוצים ללא הקלטה"))
    recording_problem_rows = [(_value(c.get("channel_id")), _value(c.get("channel_name")), _recording_he(c.get("recording"))) for c in non_recording][:14]
    camera_rows = [(_value(c.get("channel_id")), _value(c.get("channel_name")), _status_he(c.get("status"))) for c in active_cameras if str(c.get("status", "")).lower() not in ("ok", "online")][:14]
    disk_rows = [(_value(d.get("disk_id")), _status_he(d.get("status")), _value(d.get("smart_status")), _value(d.get("temperature")), _bytes_to_human(d.get("capacity_bytes")), _format_power_on_hours(d.get("power_on_hours"))) for d in disks][:14]

    html_content = REPORT_TEMPLATE.render(
        generated_at=html.escape(datetime.now().strftime("%d/%m/%Y %H:%M")),
        client_path=html.escape(_value(device.get("folder_path"), "")),
        device_name=html.escape(_short_device_name(device.get("name"))),
        severity=severity,
        severity_reason=html.escape(severity_reason),
        device_rows=[(html.escape(k), html.escape(v)) for k, v in device_rows],
        health_rows=[(html.escape(k), html.escape(v)) for k, v in health_rows],
        recording_rows=[(html.escape(k), html.escape(v)) for k, v in recording_rows],
        recording_problem_rows=[tuple(html.escape(x) for x in row) for row in recording_problem_rows],
        camera_rows=[tuple(html.escape(x) for x in row) for row in camera_rows],
        disk_rows=[tuple(html.escape(x) for x in row) for row in disk_rows],
    )

    html_fd, html_path = tempfile.mkstemp(suffix=".html")
    pdf_fd, pdf_path = tempfile.mkstemp(suffix=".pdf")
    os.close(html_fd)
    os.close(pdf_fd)
    try:
        with open(html_path, "w", encoding="utf-8") as f:
            f.write(html_content)
        proc = multiprocessing.Process(target=_render_pdf_in_subprocess, args=(html_path, pdf_path))
        _run_process(proc)
        with open(pdf_path, "rb") as f:
            return f.read()
    except ModuleNotFoundError as exc:
        raise RuntimeError("PDF engine dependency is missing: install 'playwright'.") from exc
    finally:
        for path in (html_path, pdf_path):
            try:
                os.unlink(path)
            except OSError:
                pass

