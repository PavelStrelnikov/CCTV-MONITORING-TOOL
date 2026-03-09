"""Tests for v2 schema additions: new DeviceTable columns, DeviceTagTable, DeviceHealthLogTable."""

import pytest
from sqlalchemy import inspect

from cctv_monitor.storage.tables import (
    DeviceTable,
    DeviceTagTable,
    DeviceHealthLogTable,
)


class TestDeviceTableV2Columns:
    """DeviceTable should have the five new v2 columns."""

    @pytest.mark.parametrize(
        "column_name",
        ["model", "serial_number", "firmware_version", "last_poll_at", "last_health_json"],
    )
    def test_device_table_has_v2_column(self, column_name: str) -> None:
        columns = {c.name for c in DeviceTable.__table__.columns}
        assert column_name in columns, f"DeviceTable missing column: {column_name}"

    def test_v2_columns_are_nullable(self) -> None:
        table = DeviceTable.__table__
        for col_name in ("model", "serial_number", "firmware_version", "last_poll_at", "last_health_json"):
            col = table.c[col_name]
            assert col.nullable is True, f"{col_name} should be nullable"


class TestDeviceTagTable:
    """DeviceTagTable should exist with the expected columns and constraint."""

    def test_tablename(self) -> None:
        assert DeviceTagTable.__tablename__ == "device_tags"

    def test_has_expected_columns(self) -> None:
        columns = {c.name for c in DeviceTagTable.__table__.columns}
        assert columns == {"id", "device_id", "tag"}

    def test_device_id_fk(self) -> None:
        col = DeviceTagTable.__table__.c["device_id"]
        fk_targets = [fk.target_fullname for fk in col.foreign_keys]
        assert "devices.device_id" in fk_targets

    def test_unique_constraint_exists(self) -> None:
        constraints = DeviceTagTable.__table__.constraints
        unique_names = {c.name for c in constraints if hasattr(c, "name")}
        assert "uq_device_tag" in unique_names


class TestDeviceHealthLogTable:
    """DeviceHealthLogTable should exist with the expected columns."""

    def test_tablename(self) -> None:
        assert DeviceHealthLogTable.__tablename__ == "device_health_log"

    def test_has_expected_columns(self) -> None:
        columns = {c.name for c in DeviceHealthLogTable.__table__.columns}
        expected = {
            "id", "device_id", "reachable", "camera_count",
            "online_cameras", "offline_cameras", "disk_ok",
            "response_time_ms", "checked_at",
        }
        assert columns == expected

    def test_device_id_fk(self) -> None:
        col = DeviceHealthLogTable.__table__.c["device_id"]
        fk_targets = [fk.target_fullname for fk in col.foreign_keys]
        assert "devices.device_id" in fk_targets

    def test_checked_at_not_nullable(self) -> None:
        col = DeviceHealthLogTable.__table__.c["checked_at"]
        assert col.nullable is False

    def test_composite_index_exists(self) -> None:
        indexes = DeviceHealthLogTable.__table__.indexes
        index_names = {idx.name for idx in indexes}
        assert "ix_device_health_log_device_checked" in index_names
