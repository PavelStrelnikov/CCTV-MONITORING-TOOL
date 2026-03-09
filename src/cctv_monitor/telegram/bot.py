"""Bot runtime wiring."""

import structlog

from cctv_monitor.core.config import Settings

logger = structlog.get_logger()


class BotRuntime:
    """Runtime container for Telegram polling mode."""

    def __init__(self, settings: Settings) -> None:
        self._settings = settings

    async def start(self) -> None:
        if not self._settings.TELEGRAM_BOT_TOKEN:
            logger.warning("telegram.bot.disabled", reason="missing_token")
            return
        logger.info("telegram.bot.starting")
        # TODO: initialize aiogram Bot + Dispatcher and start polling.
        logger.info("telegram.bot.started")


def create_bot_runtime() -> BotRuntime:
    settings = Settings()  # type: ignore[call-arg]
    return BotRuntime(settings)
