from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    POSTGRES_USER: str = "cctv_admin"
    POSTGRES_PASSWORD: str
    POSTGRES_HOST: str = "localhost"
    POSTGRES_PORT: int = 5432
    POSTGRES_DB: str = "cctv_monitoring"
    ENCRYPTION_KEY: str
    SNAPSHOT_BASE_DIR: str = "./data/snapshots"
    LOG_LEVEL: str = "INFO"
    HCNETSDK_LIB_PATH: str | None = None
    TELEGRAM_BOT_TOKEN: str | None = None
    INTERNAL_API_BASE_URL: str = "http://localhost:8001"
    INTERNAL_API_TOKEN: str | None = None
    TELEGRAM_DEFAULT_TIMEZONE: str = "Asia/Jerusalem"
    ADMIN_USERNAME: str = "admin"
    ADMIN_PASSWORD_HASH: str | None = None
    JWT_SECRET_KEY: str | None = None
    JWT_ALGORITHM: str = "HS256"
    JWT_EXPIRY_HOURS: int = 24
    CORS_ORIGINS: str = "http://localhost:5173"

    model_config = {"env_file": ".env", "env_file_encoding": "utf-8"}

    @property
    def database_url(self) -> str:
        return (
            f"postgresql+asyncpg://{self.POSTGRES_USER}:{self.POSTGRES_PASSWORD}"
            f"@{self.POSTGRES_HOST}:{self.POSTGRES_PORT}/{self.POSTGRES_DB}"
        )

    @property
    def database_url_sync(self) -> str:
        return (
            f"postgresql://{self.POSTGRES_USER}:{self.POSTGRES_PASSWORD}"
            f"@{self.POSTGRES_HOST}:{self.POSTGRES_PORT}/{self.POSTGRES_DB}"
        )
