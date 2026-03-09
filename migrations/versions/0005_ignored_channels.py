"""Add ignored_channels column to devices

Revision ID: 0005
Revises: 0004
Create Date: 2026-03-08

"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa

# revision identifiers, used by Alembic.
revision: str = "0005"
down_revision: Union[str, None] = "0004"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.add_column("devices", sa.Column("ignored_channels", sa.JSON(), nullable=True))


def downgrade() -> None:
    op.drop_column("devices", "ignored_channels")
