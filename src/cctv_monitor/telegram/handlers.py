"""Telegram command handlers."""

from __future__ import annotations

import httpx
from aiogram import F, Router
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


def build_router(api_client: TelegramApiClient) -> Router:
    router = Router(name="telegram_commands")
    device_cache_by_chat: dict[int, list[dict]] = {}
    channels_cache_by_chat: dict[int, dict[int, list[dict]]] = {}
    CHANNELS_PAGE_SIZE = 8

    def _main_menu() -> ReplyKeyboardMarkup:
        return ReplyKeyboardMarkup(
            keyboard=[
                [KeyboardButton(text="Overview"), KeyboardButton(text="Alerts")],
                [KeyboardButton(text="Devices"), KeyboardButton(text="Help")],
            ],
            resize_keyboard=True,
        )

    def _devices_keyboard(devices: list[dict]) -> InlineKeyboardMarkup:
        rows: list[list[InlineKeyboardButton]] = []
        for i, d in enumerate(devices[:20], start=1):
            name = d.get("name", "Unknown")
            rows.append(
                [InlineKeyboardButton(text=f"{i}. {name}", callback_data=f"devsel:{i - 1}")]
            )
        return InlineKeyboardMarkup(inline_keyboard=rows)

    def _device_actions_keyboard(idx: int) -> InlineKeyboardMarkup:
        return InlineKeyboardMarkup(
            inline_keyboard=[
                [
                    InlineKeyboardButton(text="Status", callback_data=f"devstatus:{idx}"),
                    InlineKeyboardButton(text="Poll", callback_data=f"devpoll:{idx}"),
                ],
                [
                    InlineKeyboardButton(text="Network", callback_data=f"devnet:{idx}"),
                    InlineKeyboardButton(text="Credentials", callback_data=f"devcred:{idx}"),
                ],
                [
                    InlineKeyboardButton(text="Disks", callback_data=f"devdisks:{idx}"),
                    InlineKeyboardButton(text="Channels", callback_data=f"devchannels:{idx}"),
                ]
            ]
        )

    def _channels_keyboard(idx: int, channels: list[dict], page: int) -> InlineKeyboardMarkup:
        total = len(channels)
        total_pages = max(1, (total + CHANNELS_PAGE_SIZE - 1) // CHANNELS_PAGE_SIZE)
        page = max(0, min(page, total_pages - 1))
        start = page * CHANNELS_PAGE_SIZE
        end = min(start + CHANNELS_PAGE_SIZE, total)

        rows: list[list[InlineKeyboardButton]] = []
        for i, ch in enumerate(channels[start:end], start=start + 1):
            ch_id = str(ch.get("channel_id", i))
            ch_name = ch.get("channel_name", f"CH {ch_id}")
            rows.append(
                [InlineKeyboardButton(text=f"{i}. {ch_name}", callback_data=f"chsnap:{idx}:{ch_id}")]
            )
        nav: list[InlineKeyboardButton] = []
        if page > 0:
            nav.append(InlineKeyboardButton(text="Prev", callback_data=f"chpage:{idx}:{page - 1}"))
        if page < total_pages - 1:
            nav.append(InlineKeyboardButton(text="Next", callback_data=f"chpage:{idx}:{page + 1}"))
        if nav:
            rows.append(nav)
        return InlineKeyboardMarkup(inline_keyboard=rows)

    async def _show_channels_page(callback: CallbackQuery, idx: int, page: int) -> None:
        if callback.message is None:
            return
        chat_id = callback.message.chat.id
        channel_map = channels_cache_by_chat.get(chat_id, {})
        channels = channel_map.get(idx, [])
        if not channels:
            await callback.message.answer("Channel list expired. Open Channels again.")
            return
        await callback.message.answer(
            format_channels(channels, page=page, page_size=CHANNELS_PAGE_SIZE),
            parse_mode="HTML",
        )
        await callback.message.answer(
            "Select channel for snapshot:",
            reply_markup=_channels_keyboard(idx, channels, page),
        )

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

    @router.message(Command("start"))
    async def handle_start(message: Message) -> None:
        allowed, _ = await _authorize_and_audit(message, "/start")
        if not allowed:
            return
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
        await handle_devices(message)

    @router.message(F.text == "Help")
    async def handle_menu_help(message: Message) -> None:
        await handle_help(message)

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
        parts = (message.text or "").split(maxsplit=1)
        search = parts[1].strip() if len(parts) > 1 else None
        try:
            payload = await api_client.list_devices(search=search, limit=30)
            if message.chat:
                device_cache_by_chat[message.chat.id] = payload
            await message.answer(format_devices(payload), parse_mode="HTML")
            if payload:
                await message.answer(
                    "Select a device:",
                    reply_markup=_devices_keyboard(payload),
                )
        except httpx.HTTPError:
            await message.answer("Failed to fetch devices list.")

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
            await callback.answer("Invalid selection", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await callback.answer("Device list expired. Run /devices again.", show_alert=True)
            return
        device = items[idx]
        name = device.get("name", "Unknown")
        device_id = device.get("device_id", "unknown")
        await callback.message.answer(
            f"Selected: {name} ({device_id})",
            reply_markup=_device_actions_keyboard(idx),
        )
        await callback.answer()

    @router.callback_query(F.data.startswith("devstatus:"))
    async def handle_device_status_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        chat_id = callback.message.chat.id
        items = device_cache_by_chat.get(chat_id, [])
        try:
            idx = int((callback.data or "").split(":", 1)[1])
        except (ValueError, IndexError):
            await callback.answer("Invalid action", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await callback.answer("Device list expired. Run /devices again.", show_alert=True)
            return

        allowed, _ = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "devstatus", "denied")
            await callback.answer("Access denied", show_alert=True)
            return
        await _log_callback(callback, "devstatus", "ok")

        device_id = items[idx].get("device_id", "")
        if not device_id:
            await callback.answer("Invalid device", show_alert=True)
            return

        try:
            payload = await api_client.get_device(device_id)
            await callback.message.answer(format_device_detail(payload), parse_mode="HTML")
        except httpx.HTTPStatusError as exc:
            if exc.response.status_code == 404:
                await callback.message.answer(f"Device '{device_id}' not found.")
            else:
                await callback.message.answer("Failed to fetch device details.")
        except httpx.HTTPError:
            await callback.message.answer("Failed to fetch device details.")
        await callback.answer()

    @router.callback_query(F.data.startswith("devpoll:"))
    async def handle_device_poll_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        chat_id = callback.message.chat.id
        items = device_cache_by_chat.get(chat_id, [])
        try:
            idx = int((callback.data or "").split(":", 1)[1])
        except (ValueError, IndexError):
            await callback.answer("Invalid action", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await callback.answer("Device list expired. Run /devices again.", show_alert=True)
            return

        allowed, role = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "devpoll", "denied")
            await callback.answer("Access denied", show_alert=True)
            return
        if role not in ("operator", "admin"):
            await _log_callback(callback, "devpoll", "denied")
            await callback.answer("Insufficient role for poll", show_alert=True)
            return
        await _log_callback(callback, "devpoll", "ok")

        device_id = items[idx].get("device_id", "")
        if not device_id:
            await callback.answer("Invalid device", show_alert=True)
            return

        try:
            payload = await api_client.poll_device(device_id)
            await callback.message.answer(format_poll_result(payload), parse_mode="HTML")
        except httpx.HTTPStatusError as exc:
            if exc.response.status_code == 404:
                await callback.message.answer(f"Device '{device_id}' not found.")
            else:
                await callback.message.answer("Poll failed.")
        except httpx.HTTPError:
            await callback.message.answer("Poll failed.")
        await callback.answer()

    @router.callback_query(F.data.startswith("devnet:"))
    async def handle_device_network_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        chat_id = callback.message.chat.id
        items = device_cache_by_chat.get(chat_id, [])
        try:
            idx = int((callback.data or "").split(":", 1)[1])
        except (ValueError, IndexError):
            await callback.answer("Invalid action", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await callback.answer("Device list expired. Run /devices again.", show_alert=True)
            return
        allowed, _ = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "devnet", "denied")
            await callback.answer("Access denied", show_alert=True)
            return
        await _log_callback(callback, "devnet", "ok")

        device_id = items[idx].get("device_id", "")
        try:
            payload = await api_client.get_device(device_id)
            await callback.message.answer(format_network_info(payload), parse_mode="HTML")
        except httpx.HTTPError:
            await callback.message.answer("Failed to fetch network details.")
        await callback.answer()

    @router.callback_query(F.data.startswith("devcred:"))
    async def handle_device_credentials_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        chat_id = callback.message.chat.id
        items = device_cache_by_chat.get(chat_id, [])
        try:
            idx = int((callback.data or "").split(":", 1)[1])
        except (ValueError, IndexError):
            await callback.answer("Invalid action", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await callback.answer("Device list expired. Run /devices again.", show_alert=True)
            return

        allowed, role = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "devcred", "denied")
            await callback.answer("Access denied", show_alert=True)
            return
        if role != "admin":
            await _log_callback(callback, "devcred", "denied")
            await callback.answer("Credentials are available for admin only", show_alert=True)
            return
        await _log_callback(callback, "devcred", "ok")

        device_id = items[idx].get("device_id", "")
        try:
            payload = await api_client.get_credentials(device_id)
            await callback.message.answer(format_credentials(payload), parse_mode="HTML")
        except httpx.HTTPError:
            await callback.message.answer("Failed to fetch credentials.")
        await callback.answer()

    @router.callback_query(F.data.startswith("devdisks:"))
    async def handle_device_disks_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        chat_id = callback.message.chat.id
        items = device_cache_by_chat.get(chat_id, [])
        try:
            idx = int((callback.data or "").split(":", 1)[1])
        except (ValueError, IndexError):
            await callback.answer("Invalid action", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await callback.answer("Device list expired. Run /devices again.", show_alert=True)
            return
        allowed, _ = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "devdisks", "denied")
            await callback.answer("Access denied", show_alert=True)
            return
        await _log_callback(callback, "devdisks", "ok")

        device_id = items[idx].get("device_id", "")
        try:
            payload = await api_client.get_device(device_id)
            await callback.message.answer(format_disks(payload), parse_mode="HTML")
        except httpx.HTTPError:
            await callback.message.answer("Failed to fetch disk status.")
        await callback.answer()

    @router.callback_query(F.data.startswith("devchannels:"))
    async def handle_device_channels_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        chat_id = callback.message.chat.id
        items = device_cache_by_chat.get(chat_id, [])
        try:
            idx = int((callback.data or "").split(":", 1)[1])
        except (ValueError, IndexError):
            await callback.answer("Invalid action", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await callback.answer("Device list expired. Run /devices again.", show_alert=True)
            return
        allowed, _ = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "devchannels", "denied")
            await callback.answer("Access denied", show_alert=True)
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
                await callback.message.answer("No channels available (all are ignored or unavailable).")
        except httpx.HTTPError:
            await callback.message.answer("Failed to fetch channels.")
        await callback.answer()

    @router.callback_query(F.data.startswith("chpage:"))
    async def handle_channels_page_callback(callback: CallbackQuery) -> None:
        if callback.message is None or callback.from_user is None:
            return
        try:
            _, idx_raw, page_raw = (callback.data or "").split(":", 2)
            idx = int(idx_raw)
            page = int(page_raw)
        except (ValueError, IndexError):
            await callback.answer("Invalid action", show_alert=True)
            return

        allowed, _ = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "chpage", "denied")
            await callback.answer("Access denied", show_alert=True)
            return
        await _log_callback(callback, "chpage", "ok")

        await _show_channels_page(callback, idx, page)
        await callback.answer()

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
            await callback.answer("Invalid action", show_alert=True)
            return
        if idx < 0 or idx >= len(items):
            await callback.answer("Device list expired. Run /devices again.", show_alert=True)
            return
        allowed, _ = await get_access(api_client, callback.from_user.id)
        if not allowed:
            await _log_callback(callback, "chsnap", "denied")
            await callback.answer("Access denied", show_alert=True)
            return
        await _log_callback(callback, "chsnap", "ok")

        device_id = items[idx].get("device_id", "")
        channels = channels_cache_by_chat.get(chat_id, {}).get(idx, [])
        if channels:
            available = {str(c.get("channel_id", "")) for c in channels}
            if str(channel_id) not in available:
                await callback.answer("Channel is ignored or unavailable.", show_alert=True)
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
        await callback.answer()

    return router
