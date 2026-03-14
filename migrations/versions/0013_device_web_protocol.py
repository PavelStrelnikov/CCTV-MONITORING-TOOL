"""Add web_protocol column to devices table

Revision ID: 0013
Revises: 0012
Create Date: 2026-03-13
"""

from alembic import op
import sqlalchemy as sa

revision = "0013"
down_revision = "0012"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.add_column("devices", sa.Column("web_protocol", sa.String(5), nullable=False, server_default="http"))


def downgrade() -> None:
    op.drop_column("devices", "web_protocol")
