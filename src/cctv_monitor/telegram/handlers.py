"""Telegram command handlers."""

from __future__ import annotations

import re
from html import escape
import asyncio
import httpx
from aiogram import F, Router
from aiogram.exceptions import TelegramBadRequest
from aiogram.filters import Command
from aiogram.types import (
    BufferedInputFile,
    CallbackQuery,
    InlineKeyboardButton,
    InlineKeyboardMarkup,
    KeyboardButton,
    Message,
    ReplyKeyboardMarkup,
)

from cctv_monitor.telegram.api_client import TelegramApiClient
from cctv_monitor.telegram.auth import get_access
from cctv_monitor.telegram.formatters import (
    format_alerts,
    format_devices,
    format_device_detail,
    format_disks,
    format_channels,
    format_credentials,
    format_network_info,
    format_overview,
    format_poll_result,
)
from cctv_monitor.telegram.report_pdf import build_device_poll_report_pdf, build_report_filename


def build_router(api_client: TelegramApiClient) -> Router:
    router = Router(name="telegram_commands")
    device_cache_by_chat: dict[int, list[dict]] = {}
    folder_choices_by_chat: dict[int, list[tuple[int | None, str]]] = {}
    selected_folder_by_chat: dict[int, int | None] = {}
    folder_nodes_by_chat: dict[int, dict[int, dict]] = {}
    folder_children_by_chat: dict[int, dict[int | None, list[int]]] = {}
    folder_items_by_chat: dict[int, list[tuple[str, int]]] = {}
    folder_devices_by_chat: dict[int, list[dict]] = {}
    current_folder_by_chat: dict[int, int | None] = {}
    channels_cache_by_chat: dict[int, dict[int, list[dict]]] = {}
    mode_by_chat: dict[int, str] = {}
    active_device_idx_by_chat: dict[int, int] = {}
    device_page_by_chat: dict[int, int] = {}
    channels_page_by_chat: dict[int, int] = {}
    DEVICES_PAGE_SIZE = 12
    CHANNELS_PAGE_SIZE = 8
    FOLDER_ICON = "📁"
    DEVICE_ICON = "📹"

    def _main_menu() -> ReplyKeyboardMarkup:
        return ReplyKeyboardMarkup(
            keyboard=[
                [KeyboardButton(text="Overview"), KeyboardButton(text="Alerts")],
                [KeyboardButton(text="Devices"), KeyboardButton(text="Help")],
            ],
            resize_keyboard=True,
        )

    def _device_menu() -> ReplyKeyboardMarkup:
        return ReplyKeyboardMarkup(
            keyboard=[
                [KeyboardButton(text="Status"), KeyboardButton(text="Poll")],
                [KeyboardButton(text="Poll + PDF")],
                [KeyboardButton(text="Network"), KeyboardButton(text="Credentials")],
                [KeyboardButton(text="Disks"), KeyboardButton(text="Channels")],
                [KeyboardButton(text="Back"), KeyboardButton(text="Exit")],
            ],
            resize_keyboard=True,
        )

    def _channels_menu(channels: list[dict], page: int) -> ReplyKeyboardMarkup:
        total = len(channels)
        total_pages = max(1, (total + CHANNELS_PAGE_SIZE - 1) // CHANNELS_PAGE_SIZE)
        page = max(0, min(page, total_pages - 1))
        start = page * CHANNELS_PAGE_SIZE
        end = min(start + CHANNELS_PAGE_SIZE, total)

        rows: list[list[KeyboardButton]] = []
        current_row: list[KeyboardButton] = []
        for ch in channels[start:end]:
            ch_id = str(ch.get("channel_id", ""))
            current_row.append(KeyboardButton(text=f"CH {ch_id}"))
            if len(current_row) == 2:
                rows.append(current_row)
                current_row = []
        if current_row:
            rows.append(current_row)

        nav_row: list[KeyboardButton] = []
        if page > 0:
            nav_row.append(KeyboardButton(text="Prev"))
        if page < total_pages - 1:
            nav_row.append(KeyboardButton(text="Next"))
        if nav_row:
            rows.append(nav_row)
        rows.append([KeyboardButton(text="Back"), KeyboardButton(text="Exit")])
        return ReplyKeyboardMarkup(keyboard=rows, resize_keyboard=True)

    def _devices_menu(devices: list[dict], page: int) -> ReplyKeyboardMarkup:
        total = len(devices)
        total_pages = max(1, (total + DEVICES_PAGE_SIZE - 1) // DEVICES_PAGE_SIZE)
        page = max(0, min(page, total_pages - 1))
        start = page * DEVICES_PAGE_SIZE
        end = min(start + DEVICES_PAGE_SIZE, total)

        rows: list[list[KeyboardButton]] = []
        current_row: list[KeyboardButton] = []
        for i, d in enumerate(devices[start:end], start=start + 1):
            name = str(d.get("name", "Unknown"))
            label = f"{i}. {name[:18]}"
            current_row.append(KeyboardButton(text=label))
            if len(current_row) == 2:
                rows.append(current_row)
                current_row = []
        if current_row:
            rows.append(current_row)

        nav_row: list[KeyboardButton] = []
        if page > 0:
            nav_row.append(KeyboardButton(text="Prev"))
        if page < total_pages - 1:
            nav_row.append(KeyboardButton(text="Next"))
        if nav_row:
            rows.append(nav_row)
        rows.append([KeyboardButton(text="Back"), KeyboardButton(text="Exit")])
        return ReplyKeyboardMarkup(keyboard=rows, resize_keyboard=True)

    def _folders_menu(
        items: list[tuple[str, int]],
        folder_nodes: dict[int, dict],
        devices: list[dict],
    ) -> ReplyKeyboardMarkup:
        rows: list[list[KeyboardButton]] = []
        current_row: list[KeyboardButton] = []
        for i, (kind, value) in enumerate(items, start=1):
            if kind == "folder":
                name = str(folder_nodes.get(value, {}).get("name", "Folder"))
                label = f"{i}. {FOLDER_ICON} {name[:16]}"
            else:
                name = str(devices[value].get("name", "Device"))
                label = f"{i}. {DEVICE_ICON} {name[:16]}"
            current_row.append(KeyboardButton(text=label))
            if len(current_row) == 2:
                rows.append(current_row)
                current_row = []
        if current_row:
            rows.append(current_row)
        rows.append([KeyboardButton(text="Back"), KeyboardButton(text="Exit")])
        return ReplyKeyboardMarkup(keyboard=rows, resize_keyboard=True)

    def _format_folder_choices(
        folder_id: int | None,
        items: list[tuple[str, int]],
        folder_nodes: dict[int, dict],
        devices: list[dict],
    ) -> str:
        location = "Root"
        if folder_id is not None and folder_id in folder_nodes:
            location = str(folder_nodes[folder_id].get("name", "Folder"))
        lines = ["<b>DEVICES BROWSER</b>", f"Location: <b>{escape(location)}</b>"]
        folder_lines: list[str] = []
        device_lines: list[str] = []
        for i, (kind, value) in enumerate(items, start=1):
            if kind == "folder":
                name = escape(str(folder_nodes.get(value, {}).get("name", "Folder")))
                folder_lines.append(f"{i}. {FOLDER_ICON} <b>{name}</b>")
            else:
                device = devices[value]
                name = escape(str(device.get("name", "Unknown")))
                device_lines.append(f"{i}. {DEVICE_ICON} <b>{name}</b>")
        if folder_lines:
            lines.append("\n<b>Folders</b>")
            lines.extend(folder_lines)
        if device_lines:
            lines.append("\n<b>Devices</b>")
            lines.extend(device_lines)
        if not folder_lines and not device_lines:
            lines.append("\nEmpty folder.")
        return "\n".join(lines)

    def _build_folder_index(raw: list[dict]) -> tuple[dict[int, dict], dict[int | None, list[int]]]:
        nodes: dict[int, dict] = {}
        children: dict[int | None, list[int]] = {None: []}
        for top in raw:
            top_id = top.get("id")
            if not isinstance(top_id, int):
                continue
            nodes[top_id] = {
                "id": top_id,
                "name": str(top.get("name", "Folder")),
                "parent_id": None,
                "sort_order": int(top.get("sort_order", 0) or 0),
            }
            children.setdefault(None, []).append(top_id)
            for child in top.get("children", []) or []:
                child_id = child.get("id")
                if not isinstance(child_id, int):
                    continue
                nodes[child_id] = {
                    "id": child_id,
                    "name": str(child.get("name", "Subfolder")),
                    "parent_id": top_id,
                    "sort_order": int(child.get("sort_order", 0) or 0),
                }
                children.setdefault(top_id, []).append(child_id)
                children.setdefault(child_id, [])
        def _sort_key(fid: int) -> tuple[int, str]:
            node = nodes.get(fid, {})
            return (int(node.get("sort_order", 0) or 0), str(node.get("name", "")).lower())
        for key in list(children.keys()):
            children[key] = sorted(children.get(key, []), key=_sort_key)
        return nodes, children

    def _devices_keyboard(devices: list[dict], page: int) -> InlineKeyboardMarkup:
        total = len(devices)
        total_pages = max(1, (total + DEVICES_PAGE_SIZE - 1) // DEVICES_PAGE_SIZE)
        page = max(0, min(page, total_pages - 1))
        start = page * DEVICES_PAGE_SIZE
        end = min(start + DEVICES_PAGE_SIZE, total)

        rows: list[list[InlineKeyboardButton]] = []
        current_row: list[InlineKeyboardButton] = []
        for i, d in enumerate(devices[start:end], start=start + 1):
            name = d.get("name", "Unknown")
            current_row.append(
                InlineKeyboardButton(text=f"{i}. {name}", callback_data=f"devsel:{i - 1}")
            )
            if len(current_row) == 2:
                rows.append(current_row)
                current_row = []
        if current_row:
            rows.append(current_row)

        nav: list[InlineKeyboardButton] = []
        if page > 0:
            nav.append(InlineKeyboardButton(text="Prev", callback_data=f"devpage:{page - 1}"))
        if page < total_pages - 1:
            nav.append(InlineKeyboardButton(text="Next", callback_data=f"devpage:{page + 1}"))
        if nav:
            rows.append(nav)
        rows.append([InlineKeyboardButton(text="Exit", callback_data="home")])
        return InlineKeyboardMarkup(inline_keyboard=rows)

    def _device_actions_keyboard(idx: int) -> InlineKeyboardMarkup:
        return InlineKeyboardMarkup(
            inline_keyboard=[
                [
                    InlineKeyboardButton(text="Status", callback_data=f"devstatus:{idx}"),
                    InlineKeyboardButton(text="Poll", callback_data=f"devpoll:{idx}"),
                ],
                [
                    InlineKeyboardButton(text="Poll + PDF", callback_data=f"devpollpdf:{idx}"),
                ],
                [
                    InlineKeyboardButton(text="Network", callback_data=f"devnet:{idx}"),
                    InlineKeyboardButton(text="Credentials", callback_data=f"devcred:{idx}"),
                ],
                [
                    InlineKeyboardButton(text="Disks", callback_data=f"devdisks:{idx}"),
                    InlineKeyboardButton(text="Channels", callback_data=f"devchannels:{idx}"),
                ],
                [
                    InlineKeyboardButton(text="Back", callback_data="devlist"),
                    InlineKeyboardButton(text="Exit", callback_data="home"),
                ],
            ]
        )

    def _channels_keyboard(idx: int, channels: list[dict], page: int) -> InlineKeyboardMarkup:
        total = len(channels)
        total_pages = max(1, (total + CHANNELS_PAGE_SIZE - 1) // CHANNELS_PAGE_SIZE)
        page = max(0, min(page, total_pages - 1))
        start = page * CHANNELS_PAGE_SIZE
        end = min(start + CHANNELS_PAGE_SIZE, total)

        rows: list[list[InlineKeyboardButton]] = []
        current_row: list[InlineKeyboardButton] = []
        for i, ch in enumerate(channels[start:end], start=start + 1):
            ch_id = str(ch.get("channel_id", i))
            ch_name = ch.get("channel_name", f"CH {ch_id}")
            current_row.append(
                InlineKeyboardButton(text=f"{i}. {ch_name}", callback_data=f"chsnap:{idx}:{ch_id}")
            )
            if len(current_row) == 2:
                rows.append(current_row)
                current_row = []
        if current_row:
            rows.append(current_row)
        nav: list[InlineKeyboardButton] = []
        if page > 0:
            nav.append(InlineKeyboardButton(text="Prev", callback_data=f"chpage:{idx}:{page - 1}"))
        if page < total_pages - 1:
            nav.append(InlineKeyboardButton(text="Next", callback_data=f"chpage:{idx}:{page + 1}"))
        if nav:
            rows.append(nav)
        rows.append(
            [
                InlineKeyboardButton(text="Back", callback_data=f"devback:{idx}"),
                InlineKeyboardButton(text="Exit", callback_data="home"),
            ]
        )
        return InlineKeyboardMarkup(inline_keyboard=rows)

    async def _safe_edit_message(message: Message, text: str, reply_markup: InlineKeyboardMarkup | None = None) -> None:
        try:
            await message.edit_text(text, parse_mode="HTML", reply_markup=reply_markup)
        except TelegramBadRequest:
            await message.answer(text, parse_mode="HTML", reply_markup=reply_markup)

    async def _show_devices_page(message: Message, chat_id: int, page: int) -> None:
        devices = device_cache_by_chat.get(chat_id, [])
        if not devices:
            mode_by_chat[chat_id] = "devices"
            device_page_by_chat[chat_id] = 0
            await _safe_edit_message(message, "<b>DEVICES</b>\nNo devices found.", reply_markup=None)
            return
        total_pages = max(1, (len(devices) + DEVICES_PAGE_SIZE - 1) // DEVICES_PAGE_SIZE)
        page = max(0, min(page, total_pages - 1))
        mode_by_chat[chat_id] = "devices"
        device_page_by_chat[chat_id] = page
        await _safe_edit_message(
            message,
            format_devices(devices, page=page, page_size=DEVICES_PAGE_SIZE),
            reply_markup=_devices_keyboard(devices, page),
        )
        await message.answer(
            format_devices(devices, page=page, page_size=DEVICES_PAGE_SIZE),
            parse_mode="HTML",
            reply_markup=_devices_menu(devices, page),
        )

    async def _show_folder_level(message: Message, chat_id: int, folder_id: int | None) -> None:
        folder_nodes = folder_nodes_by_chat.get(chat_id, {})
        folder_children = folder_children_by_chat.get(chat_id, {})
        if folder_id is not None and folder_id not in folder_nodes:
            folder_id = None

        # Root level should list only top-level folders. Devices appear only after entering a folder.
        if folder_id is None:
            devices: list[dict] = []
        else:
            devices = await api_client.list_devices(folder_id=folder_id, limit=200)
            devices = sorted(devices, key=lambda d: str(d.get("name", "")).lower())
        folder_devices_by_chat[chat_id] = devices
        device_cache_by_chat[chat_id] = devices
        current_folder_by_chat[chat_id] = folder_id

        items: list[tuple[str, int]] = []
        for child_id in folder_children.get(folder_id, []):
            items.append(("folder", child_id))
        for idx in range(len(devices)):
            items.append(("device", idx))
        folder_items_by_chat[chat_id] = items
        mode_by_chat[chat_id] = "folders"

        await message.answer(
            _format_folder_choices(folder_id, items, folder_nodes, devices),
            parse_mode="HTML",
            reply_markup=_folders_menu(items, folder_nodes, devices),
        )

    async def _show_folders(message: Message, chat_id: int) -> None:
        try:
            raw = await api_client.list_folders()
            nodes, children = _build_folder_index(raw)
            folder_nodes_by_chat[chat_id] = nodes
            folder_children_by_chat[chat_id] = children
            await _show_folder_level(message, chat_id, folder_id=None)
        except httpx.HTTPError:
            await message.answer("Failed to fetch folders.", reply_markup=_main_menu())
            return

    async def _show_devices_for_selected_folder(message: Message, chat_id: int, search: str | None = None) -> None:
        folder_id = selected_folder_by_chat.get(chat_id)
        payload = await api_client.list_devices(search=search, folder_id=folder_id, limit=30)
        device_cache_by_chat[chat_id] = payload
        mode_by_chat[chat_id] = "devices"
        device_page_by_chat[chat_id] = 0
        if not payload:
            await message.answer(
                "<b>DEVICES</b>\nNo devices found.",
                parse_mode="HTML",
                reply_markup=_devices_menu(payload, page=0),
            )
            return
        await message.answer(
            format_devices(payload, page=0, page_size=DEVICES_PAGE_SIZE),
            parse_mode="HTML",
            reply_markup=_devices_menu(payload, page=0),
        )

    async def _show_channels_page(callback: CallbackQuery, idx: int, page: int) -> None:
        if callback.message is None:
            return
        chat_id = callback.message.chat.id
        channel_map = channels_cache_by_chat.get(chat_id, {})
        channels = channel_map.get(idx, [])
        if not channels:
            await callback.message.answer("Channel list expired. Open Channels again.")
            return
        mode_by_chat[chat_id] = "channels"
        channels_page_by_chat[chat_id] = page
        active_device_idx_by_chat[chat_id] = idx
        await _safe_edit_message(
            callback.message,
            format_channels(channels, page=page, page_size=CHANNELS_PAGE_SIZE),
            reply_markup=_channels_keyboard(idx, channels, page),
        )
        await callback.message.answer(
            format_channels(channels, page=page, page_size=CHANNELS_PAGE_SIZE),
            parse_mode="HTML",
            reply_markup=_channels_menu(channels, page),
        )

    async def _get_best_device_detail_after_poll(device_id: str, baseline: dict | None = None) -> dict:
        """
        Poll can update detail payload asynchronously for some transports.
        We retry briefly and keep richer baseline cameras/disks if fresh payload is sparse.
        """
        payload = await api_client.get_device(device_id)
        for _ in range(8):
            cameras = payload.get("cameras", []) or []
            disks = payload.get("disks", []) or []
            if cameras or disks:
                break
            await asyncio.sleep(1.0)
            payload = await api_client.get_device(device_id)

        if baseline:
            if not (payload.get("cameras", []) or []):
                baseline_cameras = baseline.get("cameras", []) or []
                if baseline_cameras:
                    payload["cameras"] = baseline_cameras
            if not (payload.get("disks", []) or []):
                baseline_disks = baseline.get("disks", []) or []
                if baseline_disks:
                    payload["disks"] = baseline_disks

            # If fresh health is empty, keep baseline health snapshot.
            if not (payload.get("health", {}) or {}) and (baseline.get("health", {}) or {}):
                payload["health"] = baseline.get("health", {})

            # Preserve folder path from baseline when backend detail does not include it.
            if isinstance(payload.get("device"), dict) and isinstance(baseline.get("device"), dict):
                if not payload["device"].get("folder_path"):
                    payload["device"]["folder_path"] = baseline["device"].get("folder_path")
        return payload

    def _merge_poll_with_detail(poll_payload: dict, detail_payload: dict) -> dict:
        poll_health = poll_payload.get("health", {}) or {}
        detail_health = detail_payload.get("health", {}) or {}
        cameras = detail_payload.get("cameras", []) or []
        device = detail_payload.get("device", {}) or {}
        ignored = {str(ch) for ch in (device.get("ignored_channels", []) or [])}
        active_cameras = [c for c in cameras if str(c.get("channel_id", "")) not in ignored]
        active_total = len(active_cameras)
        active_online = sum(1 for c in active_cameras if str(c.get("status", "")).lower() in ("ok", "online"))

        merged = detail_health.copy()
        merged.update({k: v for k, v in poll_health.items() if v is not None})

        # Poll response can transiently return 0/0; prefer fresh detail counters when available.
        if active_total > 0:
            merged["camera_count"] = active_total
            merged["online_cameras"] = active_online
            merged["offline_cameras"] = max(0, active_total - active_online)
        else:
            total = int(merged.get("camera_count", 0) or 0)
            online = int(merged.get("online_cameras", 0) or 0)
            merged["offline_cameras"] = max(0, total - online)

        if float(merged.get("response_time_ms", 0) or 0) <= 0:
            merged["response_time_ms"] = float(detail_health.get("response_time_ms", 0) or 0)

        return {"health": merged}

    async def _log_callback(callback: CallbackQuery, command: str, status: str) -> None:
        if callback.from_user is None:
            return
        try:
            await api_client.write_audit(
                telegram_user_id=callback.from_user.id,
                telegram_chat_id=callback.message.chat.id if callback.message else None,
                command=command,
                status=status,
            )
        except httpx.HTTPError:
            pass

    async def _safe_callback_answer(callback: CallbackQuery, *args, **kwargs) -> None:
        try:
            await callback.answer(*args, **kwargs)
        except TelegramBadRequest:
            # Query can expire if handler spends too long before answering.
            pass

    async def _authorize_and_audit(message: Message, command: str) -> tuple[bool, str | None]:
        user = message.from_user
        if user is None:
            return False, None
        try:
            allowed, role = await get_access(api_client, user.id)
            await api_client.write_audit(
                telegram_user_id=user.id,
                telegram_chat_id=message.chat.id if message.chat else None,
                command=command,
                status="ok" if allowed else "denied",
            )
        except httpx.HTTPError:
            await message.answer("Service temporarily unavailable.")
            return False, None
        if not allowed:
            await message.answer("Access denied. Contact system administrator.")
        return allowed, role

    async def _get_active_device(message: Message) -> tuple[int, dict] | None:
        if message.chat is None:
            return None
        chat_id = message.chat.id
        idx = active_device_idx_by_chat.get(chat_id)
        items = device_cache_by_chat.get(chat_id, [])
        if idx is None or idx < 0 or idx >= len(items):
            await message.answer("No selected device. Open Devices first.")
            return None
        return idx, items[idx]

    async def _run_device_action(message: Message, action: str) -> None:
        info = await _get_active_device(message)
        if info is None:
            return
        idx, device = info
        device_id = device.get("device_id", "")
        if not device_id:
            await message.answer("Invalid device selection.")
            return

        allowed, role = await _authorize_and_audit(message, f"/{action}")
        if not allowed:
            return

        if action in ("poll", "poll_pdf") and role not in ("operator", "admin"):
            await message.answer("Insufficient role for poll.")
            return
        if action == "credentials" and role != "admin":
            await message.answer("Credentials are available for admin only.")
            return

        try:
            if action == "status":
                payload = await api_client.get_device(device_id)
                mode_by_chat[message.chat.id] = "device"
                await message.answer(format_device_detail(payload), parse_mode="HTML", reply_markup=_device_menu())
            elif action == "poll":
                await message.answer("Polling started...", reply_markup=_device_menu())
                baseline_payload: dict | None = None
                try:
                    baseline_payload = await api_client.get_device(device_id)
                except httpx.HTTPError:
                    baseline_payload = None
                payload = await api_client.poll_device(device_id)
                detail_after_poll = await _get_best_device_detail_after_poll(device_id, baseline_payload)
                merged_payload = _merge_poll_with_detail(payload, detail_after_poll)
                await message.answer(format_poll_result(merged_payload), parse_mode="HTML", reply_markup=_device_menu())
            elif action == "poll_pdf":
                await message.answer("Generating PDF report...", reply_markup=_device_menu())
                baseline_payload: dict | None = None
                try:
                    baseline_payload = await api_client.get_device(device_id)
                except httpx.HTTPError:
                    baseline_payload = None
                poll_payload = await api_client.poll_device(device_id)
                payload = await _get_best_device_detail_after_poll(device_id, baseline_payload)
                if isinstance(payload.get("device"), dict):
                    payload["device"]["folder_path"] = device.get("folder_path")
                pdf = build_device_poll_report_pdf(payload, poll_payload)
                device_obj = payload.get("device", {}) or {}
                filename = build_report_filename(device_obj)
                await message.answer_document(
                    document=BufferedInputFile(pdf, filename=filename),
                    caption=f"PDF report: {device_obj.get('name', device_id)}",
                    reply_markup=_device_menu(),
                )
                await message.answer(format_poll_result(poll_payload), parse_mode="HTML", reply_markup=_device_menu())
            elif action == "network":
                payload = await api_client.get_device(device_id)
                await message.answer(format_network_info(payload), parse_mode="HTML", reply_markup=_device_menu())
            elif action == "credentials":
                payload = await api_client.get_credentials(device_id)
                await message.answer(format_credentials(payload), parse_mode="HTML", reply_markup=_device_menu())
            elif action == "disks":
                payload = await api_client.get_device(device_id)
                await message.answer(format_disks(payload), parse_mode="HTML", reply_markup=_device_menu())
            elif action == "channels":
                payload = await api_client.get_device(device_id)
                channels = payload.get("cameras", []) or []
                ignored = set((payload.get("device", {}) or {}).get("ignored_channels", []) or [])
                channels = [c for c in channels if str(c.get("channel_id", "")) not in ignored]
                channels_cache_by_chat.setdefault(message.chat.id, {})[idx] = channels
                mode_by_chat[message.chat.id] = "channels"
                channels_page_by_chat[message.chat.id] = 0
                if not channels:
                    await message.answer(
                        "<b>CHANNELS</b>\nNo channels available (all are ignored or unavailable).",
                        parse_mode="HTML",
                        reply_markup=_device_menu(),
                    )
                else:
                    await message.answer(
                        format_channels(channels, page=0, page_size=CHANNELS_PAGE_SIZE),
                        parse_mode="HTML",
                        reply_markup=_channels_menu(channels, 0),
                    )
        except httpx.HTTPStatusError as exc:
            if exc.response.status_code == 404:
                await message.answer(f"Device '{device_id}' not found.")
            else:
                await message.answer(f"Action '{action}' failed.")
        except httpx.HTTPError:
            await message.answer(f"Action '{action}' failed.")
        except Exception:
            if action == "poll_pdf":
                await message.answer("PDF generation failed. Install Playwright and browser: pip install playwright && playwright install chromium")
            else:
                await message.answer(f"Action '{action}' failed.")

    @router.message(Command("start"))
    async def handle_start(message: Message) -> None:
        allowed, _ = await _authorize_and_audit(message, "/start")
        if not allowed:
            return
        if message.chat:
            mode_by_chat[message.chat.id] = "main"
        await message.answer(
            "CCTV bot connected. Use menu buttons below.",
            reply_markup=_main_menu(),
        )

    @router.message(Command("help"))
    async def handle_help(message: Message) -> None:
        allowed, _ = await _authorize_and_audit(message, "/help")
        if not allowed:
            return
        await message.answer(
            "Commands:\n"
            "/overview - system status summary\n"
            "/devices [search] - list devices and ids\n"
            "/alerts - active alerts\n"
            "/device <id> - device summary\n"
            "/poll <id> - run manual poll (operator/admin)\n"
            "/help - show this help"
        )

    @router.message(F.text == "Overview")
    async def handle_menu_overview(message: Message) -> None:
        await handle_overview(message)

    @router.message(F.text == "Alerts")
    async def handle_menu_alerts(message: Message) -> None:
        await handle_alerts(message)

    @router.message(F.text == "Devices")
    async def handle_menu_devices(message: Message) -> None:
        if message.chat is None:
            return
        await _show_folders(message, message.chat.id)

    @router.message(F.text == "Help")
    async def handle_menu_help(message: Message) -> None:
        await handle_help(message)

    @router.message(F.text == "Exit")
    async def handle_exit_text(message: Message) -> None:
        if message.chat:
            mode_by_chat[message.chat.id] = "main"
        await message.answer("Main menu", reply_markup=_main_menu())

    @router.message(F.text == "Back")
    async def handle_back_text(message: Message) -> None:
        if message.chat is None:
            return
        chat_id = message.chat.id
        mode = mode_by_chat.get(chat_id, "main")
        if mode == "channels":
            info = await _get_active_device(message)
            if info is None:
                return
            idx, device = info
            mode_by_chat[chat_id] = "device"
            await message.answer(
                f"<b>SELECTED DEVICE</b>\n{device.get('name','Unknown')}\nID: <code>{device.get('device_id','unknown')}</code>",
                parse_mode="HTML",
                reply_markup=_device_menu(),
            )
            return

        if mode == "device":
            devices = device_cache_by_chat.get(chat_id, [])
            if not devices:
                await message.answer("No device list in context. Use Devices.")
                return
            page = device_page_by_chat.get(chat_id, 0)
            mode_by_chat[chat_id] = "devices"
            await message.answer(
                format_devices(devices, page=page, page_size=DEVICES_PAGE_SIZE),
                parse_mode="HTML",
                reply_markup=_devices_menu(devices, page),
            )
            return

        if mode == "devices":
            await _show_folders(message, chat_id)
            return

        if mode == "folders":
            current_folder = current_folder_by_chat.get(chat_id)
            if current_folder is None:
                mode_by_chat[chat_id] = "main"
                await message.answer("Main menu", reply_markup=_main_menu())
                return
            parent_id = folder_nodes_by_chat.get(chat_id, {}).get(current_folder, {}).get("parent_id")
            await _show_folder_level(message, chat_id, parent_id if isinstance(parent_id, int) else None)
            return

        await message.answer("Main menu", reply_markup=_main_menu())

    @router.message(F.text == "Prev")
    async def handle_prev_text(message: Message) -> None:
        if message.chat is None:
            return
        chat_id = message.chat.id
        mode = mode_by_chat.get(chat_id, "main")
        if mode == "channels":
            info = await _get_active_device(message)
            if info is None:
                return
            idx, _ = info
            channels = channels_cache_by_chat.get(chat_id, {}).get(idx, [])
            if not channels:
                await message.answer("No channels in context.")
                return
            page = max(0, channels_page_by_chat.get(chat_id, 0) - 1)
            channels_page_by_chat[chat_id] = page
            await message.answer(
                format_channels(channels, page=page, page_size=CHANNELS_PAGE_SIZE),
                parse_mode="HTML",
                reply_markup=_channels_menu(channels, page),
            )
            return

        if mode == "devices":
            devices = device_cache_by_chat.get(chat_id, [])
            if not devices:
                await message.answer("No device list in context.")
                return
            page = max(0, device_page_by_chat.get(chat_id, 0) - 1)
            device_page_by_chat[chat_id] = page
            await message.answer(
                format_devices(devices, page=page, page_size=DEVICES_PAGE_SIZE),
                parse_mode="HTML",
                reply_markup=_devices_menu(devices, page),
            )

    @router.message(F.text == "Next")
    async def handle_next_text(message: Message) -> None:
        if message.chat is None:
            return
        chat_id = message.chat.id
        mode = mode_by_chat.get(chat_id, "main")
        if mode == "channels":
            info = await _get_active_device(message)
            if info is None:
                return
            idx, _ = info
            channels = channels_cache_by_chat.get(chat_id, {}).get(idx, [])
            if not channels:
                await message.answer("No channels in context.")
                return
            max_page = max(0, (len(channels) - 1) // CHANNELS_PAGE_SIZE)
            page = min(max_page, channels_page_by_chat.get(chat_id, 0) + 1)
            channels_page_by_chat[chat_id] = page
            await message.answer(
                format_channels(channels, page=page, page_size=CHANNELS_PAGE_SIZE),
                parse_mode="HTML",
                reply_markup=_channels_menu(channels, page),
            )
            return

        if mode == "devices":
            devices = device_cache_by_chat.get(chat_id, [])
            if not devices:
                await message.answer("No device list in context.")
                return
            max_page = max(0, (len(devices) - 1) // DEVICES_PAGE_SIZE)
            page = min(max_page, device_page_by_chat.get(chat_id, 0) + 1)
            device_page_by_chat[chat_id] = page
            await message.answer(
                format_devices(devices, page=page, page_size=DEVICES_PAGE_SIZE),
                parse_mode="HTML",
                reply_markup=_devices_menu(devices, page),
            )

    @router.message(F.text.regexp(r"^CH\s+"))
    async def handle_channel_snapshot_text(message: Message) -> None:
        if message.chat is None:
            return
        chat_id = message.chat.id
        if mode_by_chat.get(chat_id) != "channels":
            return
        info = await _get_active_device(message)
        if info is None:
            return
        idx, device = info
        channel_id = (message.text or "").split(" ", 1)[1].strip()
        channels = channels_cache_by_chat.get(chat_id, {}).get(idx, [])
        if channels:
            available = {str(c.get("channel_id", "")) for c in channels}
            if channel_id not in available:
                await message.answer("Channel is ignored or unavailable.")
                return
        try:
            await message.answer("Getting snapshot...")
            image = await api_client.get_snapshot(device.get("device_id", ""), channel_id)
            photo = BufferedInputFile(image, filename=f"{device.get('device_id','dev')}_{channel_id}.jpg")
            await message.answer_photo(photo=photo, caption=f"Snapshot: {device.get('device_id','')} / channel {channel_id}")
        except httpx.HTTPError:
            await message.answer("Failed to get snapshot.")

    @router.message(F.text == "Status")
    async def handle_status_text(message: Message) -> None:
        await _run_device_action(message, "status")

    @router.message(F.text == "Poll")
    async def handle_poll_text(message: Message) -> None:
        await _run_device_action(message, "poll")

    @router.message(F.text == "Poll + PDF")
    async def handle_poll_pdf_text(message: Message) -> None:
        await _run_device_action(message, "poll_pdf")

    @router.message(F.text == "Network")
    async def handle_network_text(message: Message) -> None:
        await _run_device_action(message, "network")

    @router.message(F.text == "Credentials")
    async def handle_credentials_text(message: Message) -> None:
        await _run_device_action(message, "credentials")

    @router.message(F.text == "Disks")
    async def handle_disks_text(message: Message) -> None:
        await _run_device_action(message, "disks")

    @router.message(F.text == "Channels")
    async def handle_channels_text(message: Message) -> None:
        await _run_device_action(message, "channels")

    @router.message(Command("overview"))
    async def handle_overview(message: Message) -> None:
        allowed, _ = await _authorize_and_audit(message, "/overview")
        if not allowed:
            return
        try:
            payload = await api_client.get_overview()
            await message.answer(format_overview(payload), parse_mode="HTML")
        except httpx.HTTPError:
            await message.answer("Failed to fetch overview. Try again later.")

    @router.message(Command("alerts"))
    async def handle_alerts(message: Message) -> None:
        allowed, _ = await _authorize_and_audit(message, "/alerts")
        if not allowed:
            return
        try:
            payload = await api_client.get_alerts(status="active", limit=10)
            await message.answer(format_alerts(payload), parse_mode="HTML")
        except httpx.HTTPError:
            await message.answer("Failed to fetch alerts. Try again later.")

    @router.message(Command("device"))
    async def handle_device(message: Message) -> None:
        allowed, _ = await _authorize_and_audit(message, "/device")
        if not allowed:
            return
        parts = (message.text or "").split(maxsplit=1)
        if len(parts) < 2 or not parts[1].strip():
            await message.answer("Usage: /device <device_id>")
            return
        device_id = parts[1].strip()
        try:
            payload = await api_client.get_device(device_id)
            await message.answer(format_device_detail(payload), parse_mode="HTML")
        except httpx.HTTPStatusError as exc:
            if exc.response.status_code == 404:
                await message.answer(f"Device '{device_id}' not found.")
                return
            await message.answer("Failed to fetch device details.")
        except httpx.HTTPError:
            await message.answer("Failed to fetch device details.")

    @router.message(Command("devices"))
    async def handle_devices(message: Message) -> None:
        allowed, _ = await _authorize_and_audit(message, "/devices")
        if not allowed:
            return
        if message.chat is None:
            return
        parts = (message.text or "").split(maxsplit=1)
        search = parts[1].strip() if len(parts) > 1 else None
        try:
            await _show_devices_for_selected_folder(message, message.chat.id, search=search)
        except httpx.HTTPError:
            await message.answer("Failed to fetch devices list.")

    @router.message(F.text.regexp(r"^\d+\.\s"))
    async def handle_numeric_selection(message: Message) -> None:
        if message.chat is None:
            return
        chat_id = message.chat.id
        mode = mode_by_chat.get(chat_id)
        match = re.match(r"^(\d+)\.\s", message.text or "")
        if not match:
            return
        one_based = int(match.group(1))

        if mode == "folders":
            choices = folder_items_by_chat.get(chat_id, [])
            idx = one_based - 1
            if idx < 0 or idx >= len(choices):
                await message.answer("Invalid selection.")
                return
            kind, value = choices[idx]
            if kind == "folder":
                try:
                    await _show_folder_level(message, chat_id, value)
                except httpx.HTTPError:
                    await message.answer("Failed to open folder.")
                return
            devices = folder_devices_by_chat.get(chat_id, [])
            if value < 0 or value >= len(devices):
                await message.answer("Invalid device selection.")
                return
            active_device_idx_by_chat[chat_id] = value
            mode_by_chat[chat_id] = "device"
            device = devices[value]
            await message.answer(
                f"<b>SELECTED DEVICE</b>\n{device.get('name','Unknown')}",
                parse_mode="HTML",
                reply_markup=_device_menu(),
            )
            return

        if mode == "devices":
            idx = one_based - 1
            items = device_cache_by_chat.get(chat_id, [])
            if idx < 0 or idx >= len(items):
                await message.answer("Invalid device selection.")
                return
            active_device_idx_by_chat[chat_id] = idx
            mode_by_chat[chat_id] = "device"
            device = items[idx]
            await message.answer(
                f"<b>SELECTED DEVICE</b>\n{device.get('name','Unknown')}\nID: <code>{device.get('device_id','unknown')}</code>",
                parse_mode="HTML",
                reply_markup=_device_menu(),
            )
            return

    @router.message(Command("poll"))
    async def handle_poll(message: Message) -> None:
        allowed, role = await _authorize_and_audit(message, "/poll")
        if not allowed:
            return
        if role not in ("operator", "admin"):
            await message.answer("Insufficient role for /poll command.")
            return
        parts = (message.text or "").split(maxsplit=1)
        if len(parts) < 2 or not parts[1].strip():
            await message.answer("Usage: /poll <device_id>")
            return
        device_id = parts[1].strip()
        try:
            payload = await api_client.poll_device(device_id)
            await message.answer(format_poll_result(payload), parse_mode="HTML")
        except httpx.HTTPStatusError as exc:
            if exc.response.status_code == 404:
                await message.answer(f"Device '{device_id}' not found.")
                return
            await message.answer("Poll failed.")
        except httpx.HTTPError:
            await message.answer("Poll failed.")

    @router.callback_query(F.data.startswith("devsel:"))
    async def handle_device_select(callback: CallbackQuery) -> None:
        if callback.message is None:
            return
        chat_id = callback.message.chat.id
        items = device_cache_by_chat.get(chat_id, [])
        try:
            idx = int((callback.data or "").split(":", 1)[1])
        except (ValueError, IndexError):
            await _safe_callback_answer(callback, "Invalid selection", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await _safe_callback_answer(callback, "Device list expired. Run /devices again.", show_alert=True)
            return
        device = items[idx]
        name = device.get("name", "Unknown")
        device_id = device.get("device_id", "unknown")
        active_device_idx_by_chat[chat_id] = idx
        mode_by_chat[chat_id] = "device"
        await _safe_edit_message(
            callback.message,
            f"<b>SELECTED DEVICE</b>\n{name}\nID: <code>{device_id}</code>",
            reply_markup=_device_actions_keyboard(idx),
        )
        await callback.message.answer(
            f"<b>SELECTED DEVICE</b>\n{name}\nID: <code>{device_id}</code>",
            parse_mode="HTML",
            reply_markup=_device_menu(),
        )
        await _safe_callback_answer(callback)

    @router.callback_query(F.data.startswith("devpage:"))
    async def handle_devices_page_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        try:
            page = int((callback.data or "").split(":", 1)[1])
        except (ValueError, IndexError):
            await _safe_callback_answer(callback, "Invalid action", show_alert=True)
            return
        allowed, _ = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "devpage", "denied")
            await _safe_callback_answer(callback, "Access denied", show_alert=True)
            return
        await _log_callback(callback, "devpage", "ok")
        await _show_devices_page(callback.message, callback.message.chat.id, page)
        await _safe_callback_answer(callback)

    @router.callback_query(F.data == "devlist")
    async def handle_devices_back_to_list(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        allowed, _ = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "devlist", "denied")
            await _safe_callback_answer(callback, "Access denied", show_alert=True)
            return
        await _log_callback(callback, "devlist", "ok")
        await _show_devices_page(callback.message, callback.message.chat.id, 0)
        await _safe_callback_answer(callback)

    @router.callback_query(F.data == "home")
    async def handle_home_callback(callback: CallbackQuery) -> None:
        if callback.message is None:
            return
        mode_by_chat[callback.message.chat.id] = "main"
        await callback.message.answer("Main menu", reply_markup=_main_menu())
        await _safe_callback_answer(callback)

    @router.callback_query(F.data.startswith("devback:"))
    async def handle_device_back_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        try:
            idx = int((callback.data or "").split(":", 1)[1])
        except (ValueError, IndexError):
            await _safe_callback_answer(callback, "Invalid action", show_alert=True)
            return
        items = device_cache_by_chat.get(callback.message.chat.id, [])
        if idx < 0 or idx >= len(items):
            await _safe_callback_answer(callback, "Device list expired. Run /devices again.", show_alert=True)
            return
        device = items[idx]
        name = device.get("name", "Unknown")
        device_id = device.get("device_id", "unknown")
        mode_by_chat[callback.message.chat.id] = "device"
        active_device_idx_by_chat[callback.message.chat.id] = idx
        await _safe_edit_message(
            callback.message,
            f"<b>SELECTED DEVICE</b>\n{name}\nID: <code>{device_id}</code>",
            reply_markup=_device_actions_keyboard(idx),
        )
        await callback.message.answer(
            f"<b>SELECTED DEVICE</b>\n{name}\nID: <code>{device_id}</code>",
            parse_mode="HTML",
            reply_markup=_device_menu(),
        )
        await _safe_callback_answer(callback)

    @router.callback_query(F.data.startswith("devstatus:"))
    async def handle_device_status_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        chat_id = callback.message.chat.id
        items = device_cache_by_chat.get(chat_id, [])
        try:
            idx = int((callback.data or "").split(":", 1)[1])
        except (ValueError, IndexError):
            await _safe_callback_answer(callback, "Invalid action", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await _safe_callback_answer(callback, "Device list expired. Run /devices again.", show_alert=True)
            return

        allowed, _ = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "devstatus", "denied")
            await _safe_callback_answer(callback, "Access denied", show_alert=True)
            return
        await _log_callback(callback, "devstatus", "ok")

        device_id = items[idx].get("device_id", "")
        if not device_id:
            await _safe_callback_answer(callback, "Invalid device", show_alert=True)
            return

        try:
            payload = await api_client.get_device(device_id)
            await _safe_edit_message(
                callback.message,
                format_device_detail(payload),
                reply_markup=_device_actions_keyboard(idx),
            )
        except httpx.HTTPStatusError as exc:
            if exc.response.status_code == 404:
                await callback.message.answer(f"Device '{device_id}' not found.")
            else:
                await callback.message.answer("Failed to fetch device details.")
        except httpx.HTTPError:
            await callback.message.answer("Failed to fetch device details.")
        await _safe_callback_answer(callback)

    @router.callback_query(F.data.startswith("devpoll:"))
    async def handle_device_poll_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        chat_id = callback.message.chat.id
        items = device_cache_by_chat.get(chat_id, [])
        try:
            idx = int((callback.data or "").split(":", 1)[1])
        except (ValueError, IndexError):
            await _safe_callback_answer(callback, "Invalid action", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await _safe_callback_answer(callback, "Device list expired. Run /devices again.", show_alert=True)
            return

        allowed, role = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "devpoll", "denied")
            await _safe_callback_answer(callback, "Access denied", show_alert=True)
            return
        if role not in ("operator", "admin"):
            await _log_callback(callback, "devpoll", "denied")
            await _safe_callback_answer(callback, "Insufficient role for poll", show_alert=True)
            return
        await _log_callback(callback, "devpoll", "ok")
        await _safe_callback_answer(callback, "Polling started...")

        device_id = items[idx].get("device_id", "")
        if not device_id:
            await _safe_callback_answer(callback, "Invalid device", show_alert=True)
            return

        try:
            baseline_payload: dict | None = None
            try:
                baseline_payload = await api_client.get_device(device_id)
            except httpx.HTTPError:
                baseline_payload = None
            payload = await api_client.poll_device(device_id)
            detail_after_poll = await _get_best_device_detail_after_poll(device_id, baseline_payload)
            merged_payload = _merge_poll_with_detail(payload, detail_after_poll)
            await _safe_edit_message(
                callback.message,
                format_poll_result(merged_payload),
                reply_markup=_device_actions_keyboard(idx),
            )
        except httpx.HTTPStatusError as exc:
            if exc.response.status_code == 404:
                await callback.message.answer(f"Device '{device_id}' not found.")
            else:
                await callback.message.answer("Poll failed.")
        except httpx.HTTPError:
            await callback.message.answer("Poll failed.")
        return

    @router.callback_query(F.data.startswith("devpollpdf:"))
    async def handle_device_poll_pdf_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        chat_id = callback.message.chat.id
        items = device_cache_by_chat.get(chat_id, [])
        try:
            idx = int((callback.data or "").split(":", 1)[1])
        except (ValueError, IndexError):
            await _safe_callback_answer(callback, "Invalid action", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await _safe_callback_answer(callback, "Device list expired. Run /devices again.", show_alert=True)
            return

        allowed, role = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "devpollpdf", "denied")
            await _safe_callback_answer(callback, "Access denied", show_alert=True)
            return
        if role not in ("operator", "admin"):
            await _log_callback(callback, "devpollpdf", "denied")
            await _safe_callback_answer(callback, "Insufficient role for poll", show_alert=True)
            return
        await _log_callback(callback, "devpollpdf", "ok")
        await _safe_callback_answer(callback, "Generating PDF report...")

        device_id = items[idx].get("device_id", "")
        if not device_id:
            await callback.message.answer("Invalid device.")
            return

        try:
            baseline_payload: dict | None = None
            try:
                baseline_payload = await api_client.get_device(device_id)
            except httpx.HTTPError:
                baseline_payload = None
            poll_payload = await api_client.poll_device(device_id)
            payload = await _get_best_device_detail_after_poll(device_id, baseline_payload)
            if isinstance(payload.get("device"), dict):
                payload["device"]["folder_path"] = items[idx].get("folder_path")
            pdf = build_device_poll_report_pdf(payload, poll_payload)
            device_obj = payload.get("device", {}) or {}
            filename = build_report_filename(device_obj)
            await callback.message.answer_document(
                document=BufferedInputFile(pdf, filename=filename),
                caption=f"PDF report: {device_obj.get('name', device_id)}",
            )
            await _safe_edit_message(
                callback.message,
                format_poll_result(poll_payload),
                reply_markup=_device_actions_keyboard(idx),
            )
        except httpx.HTTPStatusError as exc:
            if exc.response.status_code == 404:
                await callback.message.answer(f"Device '{device_id}' not found.")
            else:
                await callback.message.answer("Poll report failed.")
        except httpx.HTTPError:
            await callback.message.answer("Poll report failed.")
        except Exception:
            await callback.message.answer("Failed to generate PDF report.")
        return

    @router.callback_query(F.data.startswith("devnet:"))
    async def handle_device_network_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        chat_id = callback.message.chat.id
        items = device_cache_by_chat.get(chat_id, [])
        try:
            idx = int((callback.data or "").split(":", 1)[1])
        except (ValueError, IndexError):
            await _safe_callback_answer(callback, "Invalid action", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await _safe_callback_answer(callback, "Device list expired. Run /devices again.", show_alert=True)
            return
        allowed, _ = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "devnet", "denied")
            await _safe_callback_answer(callback, "Access denied", show_alert=True)
            return
        await _log_callback(callback, "devnet", "ok")

        device_id = items[idx].get("device_id", "")
        try:
            payload = await api_client.get_device(device_id)
            await _safe_edit_message(
                callback.message,
                format_network_info(payload),
                reply_markup=_device_actions_keyboard(idx),
            )
        except httpx.HTTPError:
            await callback.message.answer("Failed to fetch network details.")
        await _safe_callback_answer(callback)

    @router.callback_query(F.data.startswith("devcred:"))
    async def handle_device_credentials_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        chat_id = callback.message.chat.id
        items = device_cache_by_chat.get(chat_id, [])
        try:
            idx = int((callback.data or "").split(":", 1)[1])
        except (ValueError, IndexError):
            await _safe_callback_answer(callback, "Invalid action", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await _safe_callback_answer(callback, "Device list expired. Run /devices again.", show_alert=True)
            return

        allowed, role = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "devcred", "denied")
            await _safe_callback_answer(callback, "Access denied", show_alert=True)
            return
        if role != "admin":
            await _log_callback(callback, "devcred", "denied")
            await _safe_callback_answer(callback, "Credentials are available for admin only", show_alert=True)
            return
        await _log_callback(callback, "devcred", "ok")

        device_id = items[idx].get("device_id", "")
        try:
            payload = await api_client.get_credentials(device_id)
            await _safe_edit_message(
                callback.message,
                format_credentials(payload),
                reply_markup=_device_actions_keyboard(idx),
            )
        except httpx.HTTPError:
            await callback.message.answer("Failed to fetch credentials.")
        await _safe_callback_answer(callback)

    @router.callback_query(F.data.startswith("devdisks:"))
    async def handle_device_disks_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        chat_id = callback.message.chat.id
        items = device_cache_by_chat.get(chat_id, [])
        try:
            idx = int((callback.data or "").split(":", 1)[1])
        except (ValueError, IndexError):
            await _safe_callback_answer(callback, "Invalid action", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await _safe_callback_answer(callback, "Device list expired. Run /devices again.", show_alert=True)
            return
        allowed, _ = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "devdisks", "denied")
            await _safe_callback_answer(callback, "Access denied", show_alert=True)
            return
        await _log_callback(callback, "devdisks", "ok")

        device_id = items[idx].get("device_id", "")
        try:
            payload = await api_client.get_device(device_id)
            await _safe_edit_message(
                callback.message,
                format_disks(payload),
                reply_markup=_device_actions_keyboard(idx),
            )
        except httpx.HTTPError:
            await callback.message.answer("Failed to fetch disk status.")
        await _safe_callback_answer(callback)

    @router.callback_query(F.data.startswith("devchannels:"))
    async def handle_device_channels_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        chat_id = callback.message.chat.id
        items = device_cache_by_chat.get(chat_id, [])
        try:
            idx = int((callback.data or "").split(":", 1)[1])
        except (ValueError, IndexError):
            await _safe_callback_answer(callback, "Invalid action", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await _safe_callback_answer(callback, "Device list expired. Run /devices again.", show_alert=True)
            return
        allowed, _ = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "devchannels", "denied")
            await _safe_callback_answer(callback, "Access denied", show_alert=True)
            return
        await _log_callback(callback, "devchannels", "ok")

        device_id = items[idx].get("device_id", "")
        try:
            payload = await api_client.get_device(device_id)
            channels = payload.get("cameras", []) or []
            ignored = set((payload.get("device", {}) or {}).get("ignored_channels", []) or [])
            channels = [
                c for c in channels
                if str(c.get("channel_id", "")) not in ignored
            ]
            channels_cache_by_chat.setdefault(chat_id, {})[idx] = channels
            if channels:
                await _show_channels_page(callback, idx, page=0)
            else:
                await _safe_edit_message(
                    callback.message,
                    "<b>CHANNELS</b>\nNo channels available (all are ignored or unavailable).",
                    reply_markup=_device_actions_keyboard(idx),
                )
        except httpx.HTTPError:
            await callback.message.answer("Failed to fetch channels.")
        await _safe_callback_answer(callback)

    @router.callback_query(F.data.startswith("chpage:"))
    async def handle_channels_page_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        try:
            _, idx_raw, page_raw = (callback.data or "").split(":", 2)
            idx = int(idx_raw)
            page = int(page_raw)
        except (ValueError, IndexError):
            await _safe_callback_answer(callback, "Invalid action", show_alert=True)
            return

        allowed, _ = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "chpage", "denied")
            await _safe_callback_answer(callback, "Access denied", show_alert=True)
            return
        await _log_callback(callback, "chpage", "ok")

        await _show_channels_page(callback, idx, page)
        await _safe_callback_answer(callback)

    @router.callback_query(F.data.startswith("chsnap:"))
    async def handle_channel_snapshot_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        chat_id = callback.message.chat.id
        items = device_cache_by_chat.get(chat_id, [])
        try:
            _, idx_raw, channel_id = (callback.data or "").split(":", 2)
            idx = int(idx_raw)
        except (ValueError, IndexError):
            await _safe_callback_answer(callback, "Invalid action", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await _safe_callback_answer(callback, "Device list expired. Run /devices again.", show_alert=True)
            return
        allowed, _ = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "chsnap", "denied")
            await _safe_callback_answer(callback, "Access denied", show_alert=True)
            return
        await _log_callback(callback, "chsnap", "ok")
        await _safe_callback_answer(callback, "Getting snapshot...")

        device_id = items[idx].get("device_id", "")
        channels = channels_cache_by_chat.get(chat_id, {}).get(idx, [])
        if channels:
            available = {str(c.get("channel_id", "")) for c in channels}
            if str(channel_id) not in available:
                await _safe_callback_answer(callback, "Channel is ignored or unavailable.", show_alert=True)
                return
        try:
            image = await api_client.get_snapshot(device_id, channel_id)
            photo = BufferedInputFile(image, filename=f"{device_id}_{channel_id}.jpg")
            await callback.message.answer_photo(photo=photo, caption=f"Snapshot: {device_id} / channel {channel_id}")
        except httpx.HTTPStatusError as exc:
            if exc.response.status_code == 404:
                await callback.message.answer("Snapshot source not found.")
            else:
                await callback.message.answer("Failed to get snapshot.")
        except httpx.HTTPError:
            await callback.message.answer("Failed to get snapshot.")
        return

    return router


