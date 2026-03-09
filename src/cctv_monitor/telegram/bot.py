"""Bot runtime wiring."""

from aiogram import Bot, Dispatcher
from aiogram.exceptions import TelegramBadRequest
import structlog

from cctv_monitor.core.config import Settings
from cctv_monitor.telegram.api_client import TelegramApiClient
from cctv_monitor.telegram.handlers import build_router

logger = structlog.get_logger()


class BotRuntime:
    """Runtime container for Telegram polling mode."""

    def __init__(self, settings: Settings) -> None:
        self._settings = settings

    async def start(self) -> None:
        if not self._settings.TELEGRAM_BOT_TOKEN:
            logger.warning("telegram.bot.disabled", reason="missing_token")
            return

        api_client = TelegramApiClient(
            base_url=self._settings.INTERNAL_API_BASE_URL,
            internal_token=self._settings.INTERNAL_API_TOKEN,
        )
        bot = Bot(token=self._settings.TELEGRAM_BOT_TOKEN)
        dispatcher = Dispatcher()
        dispatcher.include_router(build_router(api_client))

        logger.info("telegram.bot.starting")
        try:
            logger.info("telegram.bot.started")
            await dispatcher.start_polling(bot)
        except TelegramBadRequest as exc:
            logger.error("telegram.bot.polling_error", error=str(exc))
            raise
        finally:
            await bot.session.close()
            logger.info("telegram.bot.stopped")


def create_bot_runtime() -> BotRuntime:
    settings = Settings()  # type: ignore[call-arg]
    return BotRuntime(settings)
