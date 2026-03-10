from datetime import datetime, timezone, timedelta
from sqlalchemy import select, update, delete, func
from sqlalchemy.ext.asyncio import AsyncSession
from cctv_monitor.storage.tables import (
    DeviceTable, CheckResultTable, AlertTable, SnapshotTable,
    DeviceTagTable, TagDefinitionTable, DeviceHealthLogTable, SystemSettingTable,
    FolderTable,
    TelegramUserTable, TelegramChatTable, TelegramSubscriptionTable,
    TelegramAuditLogTable, TelegramDeliveryLogTable,
)


class DeviceRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def get_active_devices(self) -> list[DeviceTable]:
        result = await self._session.execute(
            select(DeviceTable).where(DeviceTable.is_active.is_(True))
        )
        return list(result.scalars().all())

    async def get_by_id(self, device_id: str) -> DeviceTable | None:
        result = await self._session.execute(
            select(DeviceTable).where(DeviceTable.device_id == device_id)
        )
        return result.scalar_one_or_none()

    async def list_all(self) -> list[DeviceTable]:
        result = await self._session.execute(select(DeviceTable))
        return list(result.scalars().all())

    async def create(self, device: DeviceTable) -> None:
        self._session.add(device)
        await self._session.flush()

    async def update(self, device_id: str, **fields) -> DeviceTable | None:
        device = await self.get_by_id(device_id)
        if device is None:
            return None
        for key, value in fields.items():
            setattr(device, key, value)
        await self._session.flush()
        return device

    async def delete(self, device_id: str) -> bool:
        device = await self.get_by_id(device_id)
        if device is None:
            return False
        await self._session.delete(device)
        await self._session.flush()
        return True


class FolderRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def list_all(self) -> list[FolderTable]:
        result = await self._session.execute(
            select(FolderTable).order_by(FolderTable.sort_order, FolderTable.name)
        )
        return list(result.scalars().all())

    async def get_by_id(self, folder_id: int) -> FolderTable | None:
        return await self._session.get(FolderTable, folder_id)

    async def create(self, name: str, parent_id: int | None = None, color: str | None = None, icon: str | None = None) -> FolderTable:
        if parent_id is not None:
            parent = await self.get_by_id(parent_id)
            if parent is None:
                raise ValueError("Parent folder not found")
            if parent.parent_id is not None:
                raise ValueError("Maximum folder depth is 2 levels")
        folder = FolderTable(name=name, parent_id=parent_id, color=color, icon=icon)
        self._session.add(folder)
        await self._session.flush()
        return folder

    async def get_children(self, folder_id: int) -> list[FolderTable]:
        result = await self._session.execute(
            select(FolderTable).where(FolderTable.parent_id == folder_id)
        )
        return list(result.scalars().all())

    async def update(self, folder_id: int, **fields) -> FolderTable | None:
        folder = await self.get_by_id(folder_id)
        if folder is None:
            return None
        for key, value in fields.items():
            setattr(folder, key, value)
        await self._session.flush()
        return folder

    async def delete(self, folder_id: int) -> bool:
        folder = await self.get_by_id(folder_id)
        if folder is None:
            return False
        # Move devices in this folder (and subfolders) to root before delete
        # Subfolders cascade-delete, but their devices need SET NULL too
        subfolder_ids = [
            r.id for r in (await self._session.execute(
                select(FolderTable.id).where(FolderTable.parent_id == folder_id)
            )).scalars().all()
        ]
        all_folder_ids = [folder_id] + subfolder_ids
        await self._session.execute(
            update(DeviceTable)
            .where(DeviceTable.folder_id.in_(all_folder_ids))
            .values(folder_id=None)
        )
        await self._session.delete(folder)
        await self._session.flush()
        return True

    async def get_tree(self) -> list[dict]:
        folders = await self.list_all()
        # Count devices per folder
        count_result = await self._session.execute(
            select(DeviceTable.folder_id, func.count())
            .where(DeviceTable.folder_id.is_not(None))
            .group_by(DeviceTable.folder_id)
        )
        device_counts = dict(count_result.all())

        # Build tree: top-level folders with children
        top_level = [f for f in folders if f.parent_id is None]
        children_map: dict[int, list[FolderTable]] = {}
        for f in folders:
            if f.parent_id is not None:
                children_map.setdefault(f.parent_id, []).append(f)

        tree = []
        for folder in top_level:
            kids = children_map.get(folder.id, [])
            child_device_count = sum(device_counts.get(k.id, 0) for k in kids)
            tree.append({
                "id": folder.id,
                "name": folder.name,
                "sort_order": folder.sort_order,
                "color": folder.color,
                "icon": folder.icon,
                "children": [
                    {
                        "id": k.id,
                        "name": k.name,
                        "parent_id": k.parent_id,
                        "sort_order": k.sort_order,
                        "color": k.color,
                        "icon": k.icon,
                        "device_count": device_counts.get(k.id, 0),
                    }
                    for k in kids
                ],
                "device_count": device_counts.get(folder.id, 0) + child_device_count,
            })
        return tree


class CheckResultRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def save(self, result: CheckResultTable) -> None:
        self._session.add(result)
        await self._session.flush()

    async def get_latest(self, device_id: str, check_type: str) -> CheckResultTable | None:
        result = await self._session.execute(
            select(CheckResultTable)
            .where(
                CheckResultTable.device_id == device_id,
                CheckResultTable.check_type == check_type,
            )
            .order_by(CheckResultTable.checked_at.desc())
            .limit(1)
        )
        return result.scalar_one_or_none()


class AlertRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def get_active_alerts(self, device_id: str) -> list[AlertTable]:
        result = await self._session.execute(
            select(AlertTable).where(
                AlertTable.device_id == device_id,
                AlertTable.status == "active",
            )
        )
        return list(result.scalars().all())

    async def create_alert(self, alert: AlertTable) -> None:
        self._session.add(alert)
        await self._session.flush()

    async def resolve_alert(self, alert_id: int) -> None:
        await self._session.execute(
            update(AlertTable)
            .where(AlertTable.id == alert_id)
            .values(
                status="resolved",
                resolved_at=datetime.now(timezone.utc),
            )
        )

    async def get_all_alerts(self, status: str | None = None, device_id: str | None = None) -> list:
        stmt = select(AlertTable).order_by(AlertTable.created_at.desc())
        if status:
            stmt = stmt.where(AlertTable.status == status)
        if device_id:
            stmt = stmt.where(AlertTable.device_id == device_id)
        result = await self._session.execute(stmt)
        return list(result.scalars().all())


class SnapshotRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def save(self, record: SnapshotTable) -> None:
        self._session.add(record)
        await self._session.flush()


class DeviceTagRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def add_tag(self, device_id: str, tag: str) -> None:
        # Ensure tag definition exists
        existing = await self._session.get(TagDefinitionTable, tag)
        if not existing:
            self._session.add(TagDefinitionTable(name=tag))
        self._session.add(DeviceTagTable(device_id=device_id, tag=tag))

    async def remove_tag(self, device_id: str, tag: str) -> bool:
        result = await self._session.execute(
            delete(DeviceTagTable).where(
                DeviceTagTable.device_id == device_id,
                DeviceTagTable.tag == tag,
            )
        )
        return result.rowcount > 0

    async def get_tags(self, device_id: str) -> list[str]:
        result = await self._session.execute(
            select(DeviceTagTable.tag).where(DeviceTagTable.device_id == device_id)
        )
        return list(result.scalars().all())

    async def get_tags_with_colors(self, device_id: str) -> list[dict]:
        result = await self._session.execute(
            select(DeviceTagTable.tag, TagDefinitionTable.color)
            .outerjoin(TagDefinitionTable, DeviceTagTable.tag == TagDefinitionTable.name)
            .where(DeviceTagTable.device_id == device_id)
        )
        return [{"name": row.tag, "color": row.color or "#6366F1"} for row in result.all()]

    async def get_all_unique_tags(self) -> list[str]:
        result = await self._session.execute(select(DeviceTagTable.tag).distinct())
        return list(result.scalars().all())

    async def get_all_tag_definitions(self) -> list[TagDefinitionTable]:
        result = await self._session.execute(
            select(TagDefinitionTable).order_by(TagDefinitionTable.name)
        )
        return list(result.scalars().all())

    async def update_tag_definition(self, name: str, new_name: str | None = None, color: str | None = None) -> TagDefinitionTable | None:
        tag_def = await self._session.get(TagDefinitionTable, name)
        if not tag_def:
            return None
        if color is not None:
            tag_def.color = color
        if new_name is not None and new_name != name:
            # Rename: update all device_tags references, then recreate definition
            await self._session.execute(
                update(DeviceTagTable).where(DeviceTagTable.tag == name).values(tag=new_name)
            )
            await self._session.delete(tag_def)
            await self._session.flush()
            new_def = TagDefinitionTable(name=new_name, color=tag_def.color if color is None else color)
            self._session.add(new_def)
            return new_def
        return tag_def

    async def delete_tag_definition(self, name: str) -> bool:
        """Delete tag definition and remove from all devices."""
        await self._session.execute(
            delete(DeviceTagTable).where(DeviceTagTable.tag == name)
        )
        result = await self._session.execute(
            delete(TagDefinitionTable).where(TagDefinitionTable.name == name)
        )
        return result.rowcount > 0


class DeviceHealthLogRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def insert(self, *, device_id: str, reachable: bool,
                     camera_count: int, online_cameras: int,
                     offline_cameras: int, disk_ok: bool,
                     response_time_ms: float) -> None:
        self._session.add(DeviceHealthLogTable(
            device_id=device_id, reachable=reachable,
            camera_count=camera_count, online_cameras=online_cameras,
            offline_cameras=offline_cameras, disk_ok=disk_ok,
            response_time_ms=response_time_ms,
            checked_at=datetime.now(timezone.utc),
        ))

    async def get_history(self, device_id: str, hours: int = 24) -> list:
        cutoff = datetime.now(timezone.utc) - timedelta(hours=hours)
        result = await self._session.execute(
            select(DeviceHealthLogTable)
            .where(DeviceHealthLogTable.device_id == device_id,
                   DeviceHealthLogTable.checked_at >= cutoff)
            .order_by(DeviceHealthLogTable.checked_at)
        )
        return list(result.scalars().all())

    async def get_all_history(self, hours: int = 24, limit: int = 500) -> list:
        cutoff = datetime.now(timezone.utc) - timedelta(hours=hours)
        result = await self._session.execute(
            select(DeviceHealthLogTable)
            .where(DeviceHealthLogTable.checked_at >= cutoff)
            .order_by(DeviceHealthLogTable.checked_at.desc())
            .limit(limit)
        )
        return list(result.scalars().all())

    async def cleanup_old(self, days: int = 30) -> int:
        cutoff = datetime.now(timezone.utc) - timedelta(days=days)
        result = await self._session.execute(
            delete(DeviceHealthLogTable).where(DeviceHealthLogTable.checked_at < cutoff)
        )
        return result.rowcount


class SystemSettingsRepository:
    DEFAULTS: dict[str, str] = {
        "default_poll_interval": "900",
    }

    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def get(self, key: str) -> str:
        result = await self._session.execute(
            select(SystemSettingTable).where(SystemSettingTable.key == key)
        )
        row = result.scalar_one_or_none()
        if row is not None:
            return row.value
        return self.DEFAULTS.get(key, "")

    async def get_int(self, key: str) -> int:
        val = await self.get(key)
        try:
            return int(val)
        except (ValueError, TypeError):
            return int(self.DEFAULTS.get(key, "0"))

    async def set(self, key: str, value: str) -> None:
        result = await self._session.execute(
            select(SystemSettingTable).where(SystemSettingTable.key == key)
        )
        row = result.scalar_one_or_none()
        if row is not None:
            row.value = value
        else:
            self._session.add(SystemSettingTable(key=key, value=value))

    async def get_all(self) -> dict[str, str]:
        result = await self._session.execute(select(SystemSettingTable))
        stored = {r.key: r.value for r in result.scalars().all()}
        merged = dict(self.DEFAULTS)
        merged.update(stored)
        return merged


class TelegramUserRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def get_by_telegram_user_id(self, telegram_user_id: int) -> TelegramUserTable | None:
        result = await self._session.execute(
            select(TelegramUserTable).where(TelegramUserTable.telegram_user_id == telegram_user_id)
        )
        return result.scalar_one_or_none()

    async def upsert_user(
        self,
        *,
        telegram_user_id: int,
        username: str | None,
        display_name: str | None,
        role: str = "viewer",
        is_active: bool = True,
    ) -> TelegramUserTable:
        user = await self.get_by_telegram_user_id(telegram_user_id)
        if user is None:
            user = TelegramUserTable(
                telegram_user_id=telegram_user_id,
                username=username,
                display_name=display_name,
                role=role,
                is_active=is_active,
            )
            self._session.add(user)
        else:
            user.username = username
            user.display_name = display_name
            user.role = role
            user.is_active = is_active
            user.updated_at = datetime.now(timezone.utc)
        await self._session.flush()
        return user

    async def list_active_users(self) -> list[TelegramUserTable]:
        result = await self._session.execute(
            select(TelegramUserTable).where(TelegramUserTable.is_active.is_(True))
        )
        return list(result.scalars().all())


class TelegramChatRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def upsert_chat(
        self,
        *,
        telegram_chat_id: int,
        chat_type: str,
        title: str | None,
        is_active: bool = True,
    ) -> TelegramChatTable:
        result = await self._session.execute(
            select(TelegramChatTable).where(TelegramChatTable.telegram_chat_id == telegram_chat_id)
        )
        chat = result.scalar_one_or_none()
        if chat is None:
            chat = TelegramChatTable(
                telegram_chat_id=telegram_chat_id,
                chat_type=chat_type,
                title=title,
                is_active=is_active,
            )
            self._session.add(chat)
        else:
            chat.chat_type = chat_type
            chat.title = title
            chat.is_active = is_active
            chat.updated_at = datetime.now(timezone.utc)
        await self._session.flush()
        return chat


class TelegramSubscriptionRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def get_subscription(
        self,
        *,
        telegram_user_id: int,
        subscription_type: str,
    ) -> TelegramSubscriptionTable | None:
        result = await self._session.execute(
            select(TelegramSubscriptionTable).where(
                TelegramSubscriptionTable.telegram_user_id == telegram_user_id,
                TelegramSubscriptionTable.subscription_type == subscription_type,
            )
        )
        return result.scalar_one_or_none()

    async def set_subscription(
        self,
        *,
        telegram_user_id: int,
        subscription_type: str,
        enabled: bool,
        timezone_name: str = "Asia/Jerusalem",
        schedule_cron: str | None = None,
    ) -> TelegramSubscriptionTable:
        row = await self.get_subscription(
            telegram_user_id=telegram_user_id,
            subscription_type=subscription_type,
        )
        if row is None:
            row = TelegramSubscriptionTable(
                telegram_user_id=telegram_user_id,
                subscription_type=subscription_type,
                is_enabled=enabled,
                timezone=timezone_name,
                schedule_cron=schedule_cron,
            )
            self._session.add(row)
        else:
            row.is_enabled = enabled
            row.timezone = timezone_name
            row.schedule_cron = schedule_cron
            row.updated_at = datetime.now(timezone.utc)
        await self._session.flush()
        return row

    async def list_enabled_users(self, subscription_type: str) -> list[int]:
        result = await self._session.execute(
            select(TelegramSubscriptionTable.telegram_user_id).where(
                TelegramSubscriptionTable.subscription_type == subscription_type,
                TelegramSubscriptionTable.is_enabled.is_(True),
            )
        )
        return list(result.scalars().all())


class TelegramAuditRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def log_command(
        self,
        *,
        telegram_user_id: int,
        telegram_chat_id: int | None,
        command: str,
        status: str,
        args_json: dict | None = None,
        error_message: str | None = None,
    ) -> TelegramAuditLogTable:
        row = TelegramAuditLogTable(
            telegram_user_id=telegram_user_id,
            telegram_chat_id=telegram_chat_id,
            command=command,
            args_json=args_json,
            status=status,
            error_message=error_message,
            created_at=datetime.now(timezone.utc),
        )
        self._session.add(row)
        await self._session.flush()
        return row


class TelegramDeliveryRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def record_delivery(
        self,
        *,
        telegram_user_id: int,
        message_type: str,
        dedup_key: str,
        status: str,
        alert_id: int | None = None,
        error_message: str | None = None,
    ) -> TelegramDeliveryLogTable:
        row = TelegramDeliveryLogTable(
            alert_id=alert_id,
            telegram_user_id=telegram_user_id,
            message_type=message_type,
            dedup_key=dedup_key,
            status=status,
            error_message=error_message,
            created_at=datetime.now(timezone.utc),
        )
        self._session.add(row)
        await self._session.flush()
        return row

    async def was_sent_recently(self, *, dedup_key: str, cooldown_minutes: int) -> bool:
        cutoff = datetime.now(timezone.utc) - timedelta(minutes=cooldown_minutes)
        result = await self._session.execute(
            select(TelegramDeliveryLogTable.id).where(
                TelegramDeliveryLogTable.dedup_key == dedup_key,
                TelegramDeliveryLogTable.status == "sent",
                TelegramDeliveryLogTable.created_at >= cutoff,
            )
        )
        return result.scalar_one_or_none() is not None
