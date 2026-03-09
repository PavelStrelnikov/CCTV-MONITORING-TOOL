from datetime import datetime, timezone
from sqlalchemy import (
    BigInteger, Boolean, DateTime, Float, Integer, String, Text, ForeignKey, Index,
    JSON, UniqueConstraint,
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
    web_port: Mapped[int | None] = mapped_column(Integer, nullable=True)
    sdk_port: Mapped[int | None] = mapped_column(Integer, nullable=True)
    username: Mapped[str] = mapped_column(String(255))
    password_encrypted: Mapped[str] = mapped_column(Text)
    transport_mode: Mapped[str] = mapped_column(String(20), default="isapi")
    polling_policy_id: Mapped[str] = mapped_column(
        String(50), ForeignKey("polling_policies.name"), default="standard"
    )
    is_active: Mapped[bool] = mapped_column(Boolean, default=True)
    model: Mapped[str | None] = mapped_column(String(255), nullable=True)
    serial_number: Mapped[str | None] = mapped_column(String(255), nullable=True)
    firmware_version: Mapped[str | None] = mapped_column(String(255), nullable=True)
    poll_interval_seconds: Mapped[int | None] = mapped_column(Integer, nullable=True, default=None)
    last_poll_at: Mapped[datetime | None] = mapped_column(DateTime(timezone=True), nullable=True)
    last_health_json: Mapped[dict | None] = mapped_column(JSON, nullable=True)
    ignored_channels: Mapped[list | None] = mapped_column(JSON, nullable=True, default=None)


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


class TagDefinitionTable(Base):
    __tablename__ = "tag_definitions"

    name: Mapped[str] = mapped_column(String(100), primary_key=True)
    color: Mapped[str] = mapped_column(String(7), default="#6366F1")  # hex color


class DeviceTagTable(Base):
    __tablename__ = "device_tags"
    __table_args__ = (
        UniqueConstraint("device_id", "tag", name="uq_device_tag"),
    )

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    device_id: Mapped[str] = mapped_column(
        String(100), ForeignKey("devices.device_id", ondelete="CASCADE"), nullable=False
    )
    tag: Mapped[str] = mapped_column(String(100), nullable=False)


class SystemSettingTable(Base):
    __tablename__ = "system_settings"

    key: Mapped[str] = mapped_column(String(100), primary_key=True)
    value: Mapped[str] = mapped_column(Text, nullable=False)


class DeviceHealthLogTable(Base):
    __tablename__ = "device_health_log"
    __table_args__ = (
        Index("ix_device_health_log_device_checked", "device_id", "checked_at"),
    )

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    device_id: Mapped[str] = mapped_column(
        String(100), ForeignKey("devices.device_id", ondelete="CASCADE"), nullable=False
    )
    reachable: Mapped[bool] = mapped_column(Boolean, nullable=False)
    camera_count: Mapped[int] = mapped_column(Integer, default=0)
    online_cameras: Mapped[int] = mapped_column(Integer, default=0)
    offline_cameras: Mapped[int] = mapped_column(Integer, default=0)
    disk_ok: Mapped[bool] = mapped_column(Boolean, default=True)
    response_time_ms: Mapped[float] = mapped_column(Float, default=0)
    checked_at: Mapped[datetime] = mapped_column(DateTime(timezone=True), nullable=False)


class TelegramUserTable(Base):
    __tablename__ = "telegram_users"

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    telegram_user_id: Mapped[int] = mapped_column(BigInteger, unique=True, nullable=False)
    username: Mapped[str | None] = mapped_column(String(255), nullable=True)
    display_name: Mapped[str | None] = mapped_column(String(255), nullable=True)
    role: Mapped[str] = mapped_column(String(20), default="viewer", nullable=False)
    is_active: Mapped[bool] = mapped_column(Boolean, default=True, nullable=False)
    created_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True),
        default=lambda: datetime.now(timezone.utc),
        nullable=False,
    )
    updated_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True),
        default=lambda: datetime.now(timezone.utc),
        onupdate=lambda: datetime.now(timezone.utc),
        nullable=False,
    )


class TelegramChatTable(Base):
    __tablename__ = "telegram_chats"

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    telegram_chat_id: Mapped[int] = mapped_column(BigInteger, unique=True, nullable=False)
    chat_type: Mapped[str] = mapped_column(String(20), nullable=False)
    title: Mapped[str | None] = mapped_column(String(255), nullable=True)
    is_active: Mapped[bool] = mapped_column(Boolean, default=True, nullable=False)
    created_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True),
        default=lambda: datetime.now(timezone.utc),
        nullable=False,
    )
    updated_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True),
        default=lambda: datetime.now(timezone.utc),
        onupdate=lambda: datetime.now(timezone.utc),
        nullable=False,
    )


class TelegramSubscriptionTable(Base):
    __tablename__ = "telegram_subscriptions"
    __table_args__ = (
        UniqueConstraint(
            "telegram_user_id",
            "subscription_type",
            name="uq_telegram_subscription_user_type",
        ),
    )

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    telegram_user_id: Mapped[int] = mapped_column(
        BigInteger,
        ForeignKey("telegram_users.telegram_user_id", ondelete="CASCADE"),
        nullable=False,
    )
    subscription_type: Mapped[str] = mapped_column(String(50), nullable=False)
    is_enabled: Mapped[bool] = mapped_column(Boolean, default=True, nullable=False)
    schedule_cron: Mapped[str | None] = mapped_column(String(100), nullable=True)
    timezone: Mapped[str] = mapped_column(String(100), default="Asia/Jerusalem", nullable=False)
    created_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True),
        default=lambda: datetime.now(timezone.utc),
        nullable=False,
    )
    updated_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True),
        default=lambda: datetime.now(timezone.utc),
        onupdate=lambda: datetime.now(timezone.utc),
        nullable=False,
    )


class TelegramAuditLogTable(Base):
    __tablename__ = "telegram_audit_log"
    __table_args__ = (
        Index("ix_telegram_audit_created_at", "created_at"),
    )

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    telegram_user_id: Mapped[int] = mapped_column(BigInteger, nullable=False)
    telegram_chat_id: Mapped[int | None] = mapped_column(BigInteger, nullable=True)
    command: Mapped[str] = mapped_column(String(100), nullable=False)
    args_json: Mapped[dict | None] = mapped_column(JSONB, nullable=True)
    status: Mapped[str] = mapped_column(String(20), nullable=False)
    error_message: Mapped[str | None] = mapped_column(Text, nullable=True)
    created_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True),
        default=lambda: datetime.now(timezone.utc),
        nullable=False,
    )


class TelegramDeliveryLogTable(Base):
    __tablename__ = "telegram_delivery_log"
    __table_args__ = (
        Index("ix_telegram_delivery_dedup_key_created_at", "dedup_key", "created_at"),
    )

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    alert_id: Mapped[int | None] = mapped_column(
        Integer,
        ForeignKey("alerts.id", ondelete="SET NULL"),
        nullable=True,
    )
    telegram_user_id: Mapped[int] = mapped_column(BigInteger, nullable=False)
    message_type: Mapped[str] = mapped_column(String(50), nullable=False)
    dedup_key: Mapped[str] = mapped_column(String(255), nullable=False)
    status: Mapped[str] = mapped_column(String(20), nullable=False)
    error_message: Mapped[str | None] = mapped_column(Text, nullable=True)
    created_at: Mapped[datetime] = mapped_column(
        DateTime(timezone=True),
        default=lambda: datetime.now(timezone.utc),
        nullable=False,
    )
