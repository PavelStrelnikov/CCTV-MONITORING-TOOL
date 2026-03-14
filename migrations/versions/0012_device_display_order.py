"""Add display_order column to devices table

Revision ID: 0012
Revises: 0011
Create Date: 2026-03-12
"""

from alembic import op
import sqlalchemy as sa

revision = "0012"
down_revision = "0011"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.add_column("devices", sa.Column("display_order", sa.Integer(), nullable=False, server_default="0"))


def downgrade() -> None:
    op.drop_column("devices", "display_order")
