"""Add icon column to folders table

Revision ID: 0011
Revises: 0010
Create Date: 2026-03-10
"""

from alembic import op
import sqlalchemy as sa

revision = "0011"
down_revision = "0010"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.add_column("folders", sa.Column("icon", sa.String(50), nullable=True))


def downgrade() -> None:
    op.drop_column("folders", "icon")
