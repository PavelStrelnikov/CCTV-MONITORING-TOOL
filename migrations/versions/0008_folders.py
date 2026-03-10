"""Add folders for hierarchical device grouping

Revision ID: 0008
Revises: 0007
Create Date: 2026-03-10
"""

from alembic import op
import sqlalchemy as sa

revision = "0008"
down_revision = "0007"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.create_table(
        "folders",
        sa.Column("id", sa.Integer(), autoincrement=True, nullable=False),
        sa.Column("name", sa.String(255), nullable=False),
        sa.Column("parent_id", sa.Integer(), sa.ForeignKey("folders.id", ondelete="CASCADE"), nullable=True),
        sa.Column("sort_order", sa.Integer(), server_default="0", nullable=False),
        sa.PrimaryKeyConstraint("id"),
        sa.UniqueConstraint("parent_id", "name", name="uq_folder_parent_name"),
    )
    op.create_index("ix_folders_parent_id", "folders", ["parent_id"])

    op.add_column(
        "devices",
        sa.Column("folder_id", sa.Integer(), sa.ForeignKey("folders.id", ondelete="SET NULL"), nullable=True),
    )
    op.create_index("ix_devices_folder_id", "devices", ["folder_id"])


def downgrade() -> None:
    op.drop_index("ix_devices_folder_id", table_name="devices")
    op.drop_column("devices", "folder_id")
    op.drop_index("ix_folders_parent_id", table_name="folders")
    op.drop_table("folders")
