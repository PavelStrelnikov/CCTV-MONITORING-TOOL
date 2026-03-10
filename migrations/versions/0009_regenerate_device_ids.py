"""Regenerate device_id values to dvr-XXXXXXXX format

Revision ID: 0009
Revises: 0008
Create Date: 2026-03-10
"""

from alembic import op
import sqlalchemy as sa
import uuid

revision = "0009"
down_revision = "0008"
branch_labels = None
depends_on = None

FIND_FK_SQL = sa.text("""
    SELECT tc.constraint_name, tc.table_name,
           rc.delete_rule
    FROM information_schema.table_constraints tc
    JOIN information_schema.key_column_usage kcu
         ON tc.constraint_name = kcu.constraint_name
         AND tc.table_schema = kcu.table_schema
    JOIN information_schema.constraint_column_usage ccu
         ON ccu.constraint_name = tc.constraint_name
         AND ccu.table_schema = tc.table_schema
    JOIN information_schema.referential_constraints rc
         ON rc.constraint_name = tc.constraint_name
         AND rc.constraint_schema = tc.table_schema
    WHERE tc.constraint_type = 'FOREIGN KEY'
      AND ccu.table_name = 'devices'
      AND ccu.column_name = 'device_id'
""")


def upgrade() -> None:
    conn = op.get_bind()

    # Step 1: Find all FK constraints referencing devices.device_id
    fk_rows = conn.execute(FIND_FK_SQL).fetchall()
    fk_info = []
    for constraint_name, table_name, delete_rule in fk_rows:
        on_delete = None
        if delete_rule == "CASCADE":
            on_delete = "CASCADE"
        elif delete_rule == "SET NULL":
            on_delete = "SET NULL"
        fk_info.append((constraint_name, table_name, on_delete))

    # Step 2: Drop all FK constraints
    for fk_name, table, _on_del in fk_info:
        op.drop_constraint(fk_name, table, type_="foreignkey")

    # Step 3: Recreate them with ON UPDATE CASCADE
    for fk_name, table, on_delete in fk_info:
        op.create_foreign_key(
            fk_name, table, "devices",
            ["device_id"], ["device_id"],
            onupdate="CASCADE",
            ondelete=on_delete,
        )

    # Step 4: Fetch all current device_ids and update them
    devices = conn.execute(sa.text("SELECT device_id FROM devices")).fetchall()
    for (old_id,) in devices:
        new_id = f"dvr-{uuid.uuid4().hex[:8]}"
        conn.execute(
            sa.text("UPDATE devices SET device_id = :new WHERE device_id = :old"),
            {"new": new_id, "old": old_id},
        )

    # Step 5: Drop CASCADE FK constraints and recreate with original settings
    for fk_name, table, _on_del in fk_info:
        op.drop_constraint(fk_name, table, type_="foreignkey")

    for fk_name, table, on_delete in fk_info:
        op.create_foreign_key(
            fk_name, table, "devices",
            ["device_id"], ["device_id"],
            ondelete=on_delete,
        )


def downgrade() -> None:
    # Cannot reverse UUID generation - device_ids are already changed
    pass
