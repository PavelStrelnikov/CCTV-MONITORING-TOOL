"""Telegram bot service entrypoint."""

import asyncio

from cctv_monitor.telegram.bot import create_bot_runtime


async def main() -> None:
    runtime = create_bot_runtime()
    await runtime.start()


if __name__ == "__main__":
    asyncio.run(main())
