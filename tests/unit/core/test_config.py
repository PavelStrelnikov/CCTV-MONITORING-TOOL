import os
from cctv_monitor.core.config import Settings


def test_settings_loads_defaults():
    settings = Settings(
        POSTGRES_PASSWORD="test",
        ENCRYPTION_KEY="dGVzdGtleQ==",
    )
    assert settings.POSTGRES_USER == "cctv_admin"
    assert settings.POSTGRES_HOST == "localhost"
    assert settings.POSTGRES_PORT == 5432
    assert settings.POSTGRES_DB == "cctv_monitoring"
    assert settings.LOG_LEVEL == "INFO"


def test_settings_database_url():
    settings = Settings(
        POSTGRES_PASSWORD="testpass",
        POSTGRES_USER="user",
        POSTGRES_HOST="db",
        POSTGRES_PORT=5433,
        POSTGRES_DB="mydb",
        ENCRYPTION_KEY="dGVzdGtleQ==",
    )
    assert "user:testpass@db:5433/mydb" in settings.database_url


def test_settings_snapshot_base_dir_default():
    settings = Settings(
        POSTGRES_PASSWORD="test",
        ENCRYPTION_KEY="dGVzdGtleQ==",
    )
    assert settings.SNAPSHOT_BASE_DIR == "./data/snapshots"
