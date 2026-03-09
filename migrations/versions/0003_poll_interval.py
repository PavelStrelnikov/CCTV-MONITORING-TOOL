"""Add poll_interval_seconds to devices

Revision ID: 0003
Revises: 0002
Create Date: 2026-03-08

"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa

# revision identifiers, used by Alembic.
revision: str = "0003"
down_revision: Union[str, None] = "0002"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.add_column("devices", sa.Column("poll_interval_seconds", sa.Integer(), nullable=True))


def downgrade() -> None:
    op.drop_column("devices", "poll_interval_seconds")
