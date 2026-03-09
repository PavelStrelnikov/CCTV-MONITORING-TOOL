"""Telegram command handlers (skeleton)."""


async def handle_start() -> str:
    return "CCTV bot is connected. Use /help to view commands."


async def handle_help() -> str:
    return (
        "Commands:\n"
        "/overview - system status summary\n"
        "/alerts - active alerts\n"
        "/device <id> - device details\n"
        "/poll <id> - run device poll"
    )
