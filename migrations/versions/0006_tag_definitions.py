"""Add tag_definitions table for tag name + color

Revision ID: 0006
Revises: 0005
Create Date: 2026-03-08

"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa

# revision identifiers, used by Alembic.
revision: str = "0006"
down_revision: Union[str, None] = "0005"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.create_table(
        "tag_definitions",
        sa.Column("name", sa.String(100), primary_key=True),
        sa.Column("color", sa.String(7), nullable=False, server_default="#6366F1"),
    )
    # Populate tag_definitions from existing device_tags
    op.execute(
        "INSERT INTO tag_definitions (name) "
        "SELECT DISTINCT tag FROM device_tags "
        "ON CONFLICT (name) DO NOTHING"
    )


def downgrade() -> None:
    op.drop_table("tag_definitions")
