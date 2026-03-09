"""Quick script to check what ISAPI SMART/HDD endpoints return from real NVR devices."""

import asyncio
import sys
import os

# Add project root to path
sys.path.insert(0, os.path.join(os.path.dirname(__file__), "..", "src"))

from cctv_monitor.core.config import Settings
from cctv_monitor.core.crypto import decrypt_value
from cctv_monitor.storage.database import create_engine, create_session_factory
from cctv_monitor.storage.tables import DeviceTable
from sqlalchemy import select
import httpx


ENDPOINTS_TO_TRY = [
    "/ISAPI/ContentMgmt/Storage/hdd",
    "/ISAPI/ContentMgmt/Storage/hdd/1/smartTest",
    "/ISAPI/ContentMgmt/Storage/hdd/1/SMARTTest/status",
    "/ISAPI/ContentMgmt/Storage/hdd/smart",
    "/ISAPI/ContentMgmt/Storage/hdd/1/smart",
    "/ISAPI/ContentMgmt/Storage/hdd/capabilities",
    "/ISAPI/System/deviceInfo",
]


async def main():
    settings = Settings()
    engine = create_engine(settings.database_url)
    session_factory = create_session_factory(engine)

    async with session_factory() as session:
        result = await session.execute(
            select(DeviceTable).where(DeviceTable.transport_mode == "isapi", DeviceTable.is_active == True)
        )
        devices = result.scalars().all()

    if not devices:
        print("No ISAPI devices found in database")
        return

    # Print all devices and pick the one with most disks (likely the big NVR)
    for i, d in enumerate(devices):
        print(f"  [{i}] {d.name} - {d.host}:{d.port}")

    # Try to find the big NVR (16-disk one), otherwise use first
    device = devices[0]
    for d in devices:
        if "8443" in str(d.port) and "213" in d.host:
            device = d
            break
        if "9664" in (d.name or ""):
            device = d
            break
    password = decrypt_value(device.password_encrypted, settings.ENCRYPTION_KEY)
    scheme = "https" if device.port % 1000 == 443 else "http"
    base_url = f"{scheme}://{device.host}:{device.port}"

    print(f"Testing device: {device.name} ({base_url})")
    print(f"=" * 80)

    auth = httpx.DigestAuth(device.username, password)

    async with httpx.AsyncClient(verify=False, timeout=15.0) as client:
        for endpoint in ENDPOINTS_TO_TRY:
            url = f"{base_url}{endpoint}"
            print(f"\n--- {endpoint} ---")
            try:
                resp = await client.get(url, auth=auth)
                print(f"Status: {resp.status_code}")
                if resp.status_code == 200:
                    text = resp.text
                    # Print first 3000 chars to see structure
                    if len(text) > 3000:
                        print(text[:3000] + "\n... (truncated)")
                    else:
                        print(text)
                else:
                    print(resp.text[:500] if resp.text else "(empty)")
            except Exception as e:
                print(f"Error: {e}")

    await engine.dispose()


if __name__ == "__main__":
    asyncio.run(main())
