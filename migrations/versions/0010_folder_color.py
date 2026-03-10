"""Add color column to folders table

Revision ID: 0010
Revises: 0009
Create Date: 2026-03-10
"""

from alembic import op
import sqlalchemy as sa

revision = "0010"
down_revision = "0009"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.add_column("folders", sa.Column("color", sa.String(7), nullable=True))


def downgrade() -> None:
    op.drop_column("folders", "color")
