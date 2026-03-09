"""v2 schema: device extras, tags, health log

Revision ID: 0002
Revises: 0001
Create Date: 2026-03-08

"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa

# revision identifiers, used by Alembic.
revision: str = "0002"
down_revision: Union[str, None] = "0001"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    # --- Add new columns to devices table ---
    op.add_column("devices", sa.Column("model", sa.String(255), nullable=True))
    op.add_column("devices", sa.Column("serial_number", sa.String(255), nullable=True))
    op.add_column("devices", sa.Column("firmware_version", sa.String(255), nullable=True))
    op.add_column("devices", sa.Column("last_poll_at", sa.DateTime(timezone=True), nullable=True))
    op.add_column("devices", sa.Column("last_health_json", sa.JSON(), nullable=True))

    # --- Create device_tags table ---
    op.create_table(
        "device_tags",
        sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
        sa.Column(
            "device_id",
            sa.String(100),
            sa.ForeignKey("devices.device_id", ondelete="CASCADE"),
            nullable=False,
        ),
        sa.Column("tag", sa.String(100), nullable=False),
        sa.UniqueConstraint("device_id", "tag", name="uq_device_tag"),
    )
    op.create_index("ix_device_tags_tag", "device_tags", ["tag"])

    # --- Create device_health_log table ---
    op.create_table(
        "device_health_log",
        sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
        sa.Column(
            "device_id",
            sa.String(100),
            sa.ForeignKey("devices.device_id", ondelete="CASCADE"),
            nullable=False,
        ),
        sa.Column("reachable", sa.Boolean(), nullable=False),
        sa.Column("camera_count", sa.Integer(), nullable=False, server_default="0"),
        sa.Column("online_cameras", sa.Integer(), nullable=False, server_default="0"),
        sa.Column("offline_cameras", sa.Integer(), nullable=False, server_default="0"),
        sa.Column("disk_ok", sa.Boolean(), nullable=False, server_default="true"),
        sa.Column("response_time_ms", sa.Float(), nullable=False, server_default="0"),
        sa.Column("checked_at", sa.DateTime(timezone=True), nullable=False),
    )
    op.create_index(
        "ix_device_health_log_device_checked",
        "device_health_log",
        ["device_id", "checked_at"],
    )


def downgrade() -> None:
    op.drop_index("ix_device_health_log_device_checked", table_name="device_health_log")
    op.drop_table("device_health_log")
    op.drop_index("ix_device_tags_tag", table_name="device_tags")
    op.drop_table("device_tags")

    op.drop_column("devices", "last_health_json")
    op.drop_column("devices", "last_poll_at")
    op.drop_column("devices", "firmware_version")
    op.drop_column("devices", "serial_number")
    op.drop_column("devices", "model")
