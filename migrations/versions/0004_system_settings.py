"""Add system_settings table

Revision ID: 0004
Revises: 0003
Create Date: 2026-03-08

"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa

# revision identifiers, used by Alembic.
revision: str = "0004"
down_revision: Union[str, None] = "0003"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.create_table(
        "system_settings",
        sa.Column("key", sa.String(100), primary_key=True),
        sa.Column("value", sa.Text(), nullable=False),
    )
    # Seed default poll interval
    op.execute(
        "INSERT INTO system_settings (key, value) VALUES ('default_poll_interval', '900')"
    )


def downgrade() -> None:
    op.drop_table("system_settings")
