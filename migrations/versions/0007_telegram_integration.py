"""Add Telegram integration tables

Revision ID: 0007
Revises: 0006
Create Date: 2026-03-09

"""
from typing import Sequence, Union

from alembic import op
import sqlalchemy as sa
from sqlalchemy.dialects import postgresql

# revision identifiers, used by Alembic.
revision: str = "0007"
down_revision: Union[str, None] = "0006"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.create_table(
        "telegram_users",
        sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
        sa.Column("telegram_user_id", sa.BigInteger(), nullable=False, unique=True),
        sa.Column("username", sa.String(length=255), nullable=True),
        sa.Column("display_name", sa.String(length=255), nullable=True),
        sa.Column("role", sa.String(length=20), nullable=False, server_default="viewer"),
        sa.Column("is_active", sa.Boolean(), nullable=False, server_default=sa.true()),
        sa.Column("created_at", sa.DateTime(timezone=True), nullable=False, server_default=sa.text("now()")),
        sa.Column("updated_at", sa.DateTime(timezone=True), nullable=False, server_default=sa.text("now()")),
    )

    op.create_table(
        "telegram_chats",
        sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
        sa.Column("telegram_chat_id", sa.BigInteger(), nullable=False, unique=True),
        sa.Column("chat_type", sa.String(length=20), nullable=False),
        sa.Column("title", sa.String(length=255), nullable=True),
        sa.Column("is_active", sa.Boolean(), nullable=False, server_default=sa.true()),
        sa.Column("created_at", sa.DateTime(timezone=True), nullable=False, server_default=sa.text("now()")),
        sa.Column("updated_at", sa.DateTime(timezone=True), nullable=False, server_default=sa.text("now()")),
    )

    op.create_table(
        "telegram_subscriptions",
        sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
        sa.Column("telegram_user_id", sa.BigInteger(), nullable=False),
        sa.Column("subscription_type", sa.String(length=50), nullable=False),
        sa.Column("is_enabled", sa.Boolean(), nullable=False, server_default=sa.true()),
        sa.Column("schedule_cron", sa.String(length=100), nullable=True),
        sa.Column("timezone", sa.String(length=100), nullable=False, server_default="Asia/Jerusalem"),
        sa.Column("created_at", sa.DateTime(timezone=True), nullable=False, server_default=sa.text("now()")),
        sa.Column("updated_at", sa.DateTime(timezone=True), nullable=False, server_default=sa.text("now()")),
        sa.ForeignKeyConstraint(
            ["telegram_user_id"],
            ["telegram_users.telegram_user_id"],
            ondelete="CASCADE",
        ),
        sa.UniqueConstraint(
            "telegram_user_id",
            "subscription_type",
            name="uq_telegram_subscription_user_type",
        ),
    )

    op.create_table(
        "telegram_audit_log",
        sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
        sa.Column("telegram_user_id", sa.BigInteger(), nullable=False),
        sa.Column("telegram_chat_id", sa.BigInteger(), nullable=True),
        sa.Column("command", sa.String(length=100), nullable=False),
        sa.Column("args_json", postgresql.JSONB(astext_type=sa.Text()), nullable=True),
        sa.Column("status", sa.String(length=20), nullable=False),
        sa.Column("error_message", sa.Text(), nullable=True),
        sa.Column("created_at", sa.DateTime(timezone=True), nullable=False, server_default=sa.text("now()")),
    )
    op.create_index("ix_telegram_audit_created_at", "telegram_audit_log", ["created_at"])

    op.create_table(
        "telegram_delivery_log",
        sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
        sa.Column("alert_id", sa.Integer(), nullable=True),
        sa.Column("telegram_user_id", sa.BigInteger(), nullable=False),
        sa.Column("message_type", sa.String(length=50), nullable=False),
        sa.Column("dedup_key", sa.String(length=255), nullable=False),
        sa.Column("status", sa.String(length=20), nullable=False),
        sa.Column("error_message", sa.Text(), nullable=True),
        sa.Column("created_at", sa.DateTime(timezone=True), nullable=False, server_default=sa.text("now()")),
        sa.ForeignKeyConstraint(["alert_id"], ["alerts.id"], ondelete="SET NULL"),
    )
    op.create_index(
        "ix_telegram_delivery_dedup_key_created_at",
        "telegram_delivery_log",
        ["dedup_key", "created_at"],
    )


def downgrade() -> None:
    op.drop_index("ix_telegram_delivery_dedup_key_created_at", table_name="telegram_delivery_log")
    op.drop_table("telegram_delivery_log")
    op.drop_index("ix_telegram_audit_created_at", table_name="telegram_audit_log")
    op.drop_table("telegram_audit_log")
    op.drop_table("telegram_subscriptions")
    op.drop_table("telegram_chats")
    op.drop_table("telegram_users")
