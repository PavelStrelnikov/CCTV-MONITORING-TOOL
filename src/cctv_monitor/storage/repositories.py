from datetime import datetime, timezone
from sqlalchemy import select, update
from sqlalchemy.ext.asyncio import AsyncSession
from cctv_monitor.storage.tables import (
    DeviceTable, CheckResultTable, AlertTable, SnapshotTable,
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


class SnapshotRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def save(self, record: SnapshotTable) -> None:
        self._session.add(record)
        await self._session.flush()
