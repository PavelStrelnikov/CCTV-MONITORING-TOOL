from datetime import datetime, timezone, timedelta
from sqlalchemy import select, update, delete
from sqlalchemy.ext.asyncio import AsyncSession
from cctv_monitor.storage.tables import (
    DeviceTable, CheckResultTable, AlertTable, SnapshotTable,
    DeviceTagTable, TagDefinitionTable, DeviceHealthLogTable, SystemSettingTable,
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
