from datetime import datetime, timezone
from sqlalchemy import (
    Boolean, DateTime, Float, Integer, String, Text, ForeignKey, Index,
)
from sqlalchemy.dialects.postgresql import JSONB
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column


class Base(DeclarativeBase):
    pass


class PollingPolicyTable(Base):
    __tablename__ = "polling_policies"

    name: Mapped[str] = mapped_column(String(50), primary_key=True)
    device_info_interval: Mapped[int] = mapped_column(Integer, default=300)
    camera_status_interval: Mapped[int] = mapped_column(Integer, default=120)
    disk_status_interval: Mapped[int] = mapped_column(Integer, default=600)
    snapshot_interval: Mapped[int] = mapped_column(Integer, default=900)


class DeviceTable(Base):
    __tablename__ = "devices"

    device_id: Mapped[str] = mapped_column(String(100), primary_key=True)
    name: Mapped[str] = mapped_column(String(255))
    vendor: Mapped[str] = mapped_column(String(50))
    host: Mapped[str] = mapped_column(String(255))
    port: Mapped[int] = mapped_column(Integer, default=80)
    username: Mapped[str] = mapped_column(String(255))
    password_encrypted: Mapped[str] = mapped_column(Text)
    transport_mode: Mapped[str] = mapped_column(String(20), default="isapi")
    polling_policy_id: Mapped[str] = mapped_column(
        String(50), ForeignKey("polling_policies.name"), default="standard"
    )
    is_active: Mapped[bool] = mapped_column(Boolean, default=True)


class DeviceCapabilityTable(Base):
    __tablename__ = "device_capabilities"

    device_id: Mapped[str] = mapped_column(
        String(100), ForeignKey("devices.device_id"), primary_key=True
    )
    model: Mapped[str] = mapped_column(String(255), default="")
    firmware_version: Mapped[str] = mapped_column(String(255), default="")
    supports_isapi: Mapped[bool] = mapped_column(Boolean, default=False)
    supports_sdk: Mapped[bool] = mapped_column(Boolean, default=False)
    supports_snapshot: Mapped[bool] = mapped_column(Boolean, default=False)
    supports_recording_status: Mapped[bool] = mapped_column(Boolean, default=False)
    supports_disk_status: Mapped[bool] = mapped_column(Boolean, default=False)
    detected_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True),
        default=lambda: datetime.now(timezone.utc),
    )


class CheckResultTable(Base):
    __tablename__ = "check_results"
    __table_args__ = (
        Index("ix_check_results_device_type_time", "device_id", "check_type", "checked_at"),
    )

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    device_id: Mapped[str] = mapped_column(String(100), ForeignKey("devices.device_id"))
    check_type: Mapped[str] = mapped_column(String(50))
    success: Mapped[bool] = mapped_column(Boolean)
    error_type: Mapped[str | None] = mapped_column(String(100), nullable=True)
    duration_ms: Mapped[float] = mapped_column(Float)
    payload_json: Mapped[dict | None] = mapped_column(JSONB, nullable=True)
    checked_at: Mapped[datetime] = mapped_column(DateTime(timezone=True))


class SnapshotTable(Base):
    __tablename__ = "snapshots"

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    device_id: Mapped[str] = mapped_column(String(100), ForeignKey("devices.device_id"))
    channel_id: Mapped[str] = mapped_column(String(50))
    file_path: Mapped[str] = mapped_column(Text)
    file_size_bytes: Mapped[int] = mapped_column(Integer)
    checked_at: Mapped[datetime] = mapped_column(DateTime(timezone=True))


class AlertTable(Base):
    __tablename__ = "alerts"
    __table_args__ = (
        Index("ix_alerts_device_status", "device_id", "status"),
    )

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    device_id: Mapped[str] = mapped_column(String(100), ForeignKey("devices.device_id"))
    channel_id: Mapped[str | None] = mapped_column(String(50), nullable=True)
    alert_type: Mapped[str] = mapped_column(String(50))
    severity: Mapped[str] = mapped_column(String(20))
    message: Mapped[str] = mapped_column(Text)
    source: Mapped[str] = mapped_column(String(50))
    status: Mapped[str] = mapped_column(String(20), default="active")
    created_at: Mapped[datetime] = mapped_column(DateTime(timezone=True))
    resolved_at: Mapped[datetime | None] = mapped_column(DateTime(timezone=True), nullable=True)
