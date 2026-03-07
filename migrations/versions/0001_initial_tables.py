"""initial tables

Revision ID: 0001
Revises:
Create Date: 2026-03-07

"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa
from sqlalchemy.dialects import postgresql

# revision identifiers, used by Alembic.
revision: str = "0001"
down_revision: Union[str, None] = None
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.create_table(
        "polling_policies",
        sa.Column("name", sa.String(50), primary_key=True),
        sa.Column("device_info_interval", sa.Integer(), nullable=False, server_default="300"),
        sa.Column("camera_status_interval", sa.Integer(), nullable=False, server_default="120"),
        sa.Column("disk_status_interval", sa.Integer(), nullable=False, server_default="600"),
        sa.Column("snapshot_interval", sa.Integer(), nullable=False, server_default="900"),
    )

    op.create_table(
        "devices",
        sa.Column("device_id", sa.String(100), primary_key=True),
        sa.Column("name", sa.String(255), nullable=False),
        sa.Column("vendor", sa.String(50), nullable=False),
        sa.Column("host", sa.String(255), nullable=False),
        sa.Column("port", sa.Integer(), nullable=False, server_default="80"),
        sa.Column("username", sa.String(255), nullable=False),
        sa.Column("password_encrypted", sa.Text(), nullable=False),
        sa.Column("transport_mode", sa.String(20), nullable=False, server_default="'isapi'"),
        sa.Column(
            "polling_policy_id",
            sa.String(50),
            sa.ForeignKey("polling_policies.name"),
            nullable=False,
            server_default="'standard'",
        ),
        sa.Column("is_active", sa.Boolean(), nullable=False, server_default="true"),
    )

    op.create_table(
        "device_capabilities",
        sa.Column(
            "device_id",
            sa.String(100),
            sa.ForeignKey("devices.device_id"),
            primary_key=True,
        ),
        sa.Column("model", sa.String(255), nullable=False, server_default="''"),
        sa.Column("firmware_version", sa.String(255), nullable=False, server_default="''"),
        sa.Column("supports_isapi", sa.Boolean(), nullable=False, server_default="false"),
        sa.Column("supports_sdk", sa.Boolean(), nullable=False, server_default="false"),
        sa.Column("supports_snapshot", sa.Boolean(), nullable=False, server_default="false"),
        sa.Column("supports_recording_status", sa.Boolean(), nullable=False, server_default="false"),
        sa.Column("supports_disk_status", sa.Boolean(), nullable=False, server_default="false"),
        sa.Column("detected_at", sa.DateTime(timezone=True), nullable=False),
    )

    op.create_table(
        "check_results",
        sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
        sa.Column(
            "device_id",
            sa.String(100),
            sa.ForeignKey("devices.device_id"),
            nullable=False,
        ),
        sa.Column("check_type", sa.String(50), nullable=False),
        sa.Column("success", sa.Boolean(), nullable=False),
        sa.Column("error_type", sa.String(100), nullable=True),
        sa.Column("duration_ms", sa.Float(), nullable=False),
        sa.Column("payload_json", postgresql.JSONB(), nullable=True),
        sa.Column("checked_at", sa.DateTime(timezone=True), nullable=False),
    )
    op.create_index(
        "ix_check_results_device_type_time",
        "check_results",
        ["device_id", "check_type", "checked_at"],
    )

    op.create_table(
        "snapshots",
        sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
        sa.Column(
            "device_id",
            sa.String(100),
            sa.ForeignKey("devices.device_id"),
            nullable=False,
        ),
        sa.Column("channel_id", sa.String(50), nullable=False),
        sa.Column("file_path", sa.Text(), nullable=False),
        sa.Column("file_size_bytes", sa.Integer(), nullable=False),
        sa.Column("checked_at", sa.DateTime(timezone=True), nullable=False),
    )

    op.create_table(
        "alerts",
        sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
        sa.Column(
            "device_id",
            sa.String(100),
            sa.ForeignKey("devices.device_id"),
            nullable=False,
        ),
        sa.Column("channel_id", sa.String(50), nullable=True),
        sa.Column("alert_type", sa.String(50), nullable=False),
        sa.Column("severity", sa.String(20), nullable=False),
        sa.Column("message", sa.Text(), nullable=False),
        sa.Column("source", sa.String(50), nullable=False),
        sa.Column("status", sa.String(20), nullable=False, server_default="'active'"),
        sa.Column("created_at", sa.DateTime(timezone=True), nullable=False),
        sa.Column("resolved_at", sa.DateTime(timezone=True), nullable=True),
    )
    op.create_index(
        "ix_alerts_device_status",
        "alerts",
        ["device_id", "status"],
    )


def downgrade() -> None:
    op.drop_index("ix_alerts_device_status", table_name="alerts")
    op.drop_table("alerts")
    op.drop_table("snapshots")
    op.drop_index("ix_check_results_device_type_time", table_name="check_results")
    op.drop_table("check_results")
    op.drop_table("device_capabilities")
    op.drop_table("devices")
    op.drop_table("polling_policies")
