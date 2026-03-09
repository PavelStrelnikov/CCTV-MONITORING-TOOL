"""Try to get SMART/HDD data from SDK-only device at 89.237.85.40:8008.

Approach 1: Try ISAPI on the same host (different ports)
Approach 2: Try SDK calls for disk info
"""

import asyncio
import sys
import os

sys.path.insert(0, os.path.join(os.path.dirname(__file__), "..", "src"))

from cctv_monitor.core.config import Settings
from cctv_monitor.core.crypto import decrypt_value
from cctv_monitor.storage.database import create_engine, create_session_factory
from cctv_monitor.storage.tables import DeviceTable
from sqlalchemy import select
import httpx


async def try_isapi(host, username, password):
    """Try ISAPI on common ports to see if HTTP is available."""
    ports = [80, 443, 8080, 8443, 8008]
    auth = httpx.DigestAuth(username, password)

    async with httpx.AsyncClient(verify=False, timeout=10.0) as client:
        for port in ports:
            for scheme in ["http", "https"]:
                url = f"{scheme}://{host}:{port}/ISAPI/System/deviceInfo"
                try:
                    resp = await client.get(url, auth=auth)
                    if resp.status_code == 200 and "DeviceInfo" in resp.text:
                        print(f"  ISAPI available at {scheme}://{host}:{port}")
                        # Try SMART
                        smart_url = f"{scheme}://{host}:{port}/ISAPI/ContentMgmt/Storage/hdd/1/SMARTTest/status"
                        smart_resp = await client.get(smart_url, auth=auth)
                        print(f"  SMART status: {smart_resp.status_code}")
                        if smart_resp.status_code == 200:
                            text = smart_resp.text
                            print(text[:2000] if len(text) > 2000 else text)
                        return True
                except Exception as e:
                    pass  # Port not available
    return False


async def try_sdk(host, port, username, password):
    """Try SDK calls for disk info."""
    settings = Settings()
    lib_path = settings.HCNETSDK_LIB_PATH
    if not lib_path:
        print("  SDK not configured (no HCNETSDK_LIB_PATH)")
        return

    from cctv_monitor.drivers.hikvision.transports.sdk_bindings import HCNetSDKBinding
    import ctypes
    from ctypes import c_uint

    binding = HCNetSDKBinding(lib_path=lib_path)
    binding.init()

    try:
        user_id, login_info = binding.login(host, port, username, password)
        print(f"  SDK Login OK, user_id={user_id}")
        print(f"  Login info: {login_info}")

        # Get HDD config (already implemented)
        print("\n  --- HDD Config (NET_DVR_GET_HDCFG = 1054) ---")
        disks = binding.get_hdd_config(user_id)
        for d in disks:
            print(f"  Disk {d['disk_id']}: {d['capacity_mb']}MB, "
                  f"free={d['free_space_mb']}MB, status={d['status_name']}, "
                  f"type={d['hd_type']}, attr={d['hd_attr']}")

        # Try various SDK commands that might have SMART data
        # NET_DVR_GET_HDCFG_V40 might have more info
        smart_commands = [
            (1054, "NET_DVR_GET_HDCFG"),
            (1056, "NET_DVR_GET_HDGROUP"),
            (6701, "NET_DVR_GET_TEMP_HUMI_INFO"),
        ]

        for cmd, name in smart_commands:
            if cmd == 1054:
                continue  # Already tried
            print(f"\n  --- {name} (cmd={cmd}) ---")
            # Generic attempt with a large buffer
            buf = ctypes.create_string_buffer(4096)
            bytes_ret = c_uint(0)
            ok = binding._lib.NET_DVR_GetDVRConfig(
                user_id, cmd, 0,
                ctypes.byref(buf), 4096,
                ctypes.byref(bytes_ret)
            )
            if ok:
                print(f"  OK! bytes_returned={bytes_ret.value}")
                # Print raw bytes as hex for analysis
                raw = buf.raw[:bytes_ret.value]
                print(f"  Raw (first 200 bytes): {raw[:200].hex()}")
            else:
                err = binding.get_last_error()
                print(f"  Failed, error_code={err}")

        binding.logout(user_id)
    except Exception as e:
        print(f"  SDK error: {e}")


async def main():
    settings = Settings()
    engine = create_engine(settings.database_url)
    session_factory = create_session_factory(engine)

    async with session_factory() as session:
        result = await session.execute(
            select(DeviceTable).where(
                DeviceTable.host == "89.237.85.40"
            )
        )
        device = result.scalar_one_or_none()

    if not device:
        print("Device 89.237.85.40 not found in database")
        return

    password = decrypt_value(device.password_encrypted, settings.ENCRYPTION_KEY)
    print(f"Device: {device.name} ({device.host}:{device.port}, transport={device.transport_mode})")
    print("=" * 80)

    print("\n1. Trying ISAPI on common ports...")
    isapi_found = await try_isapi(device.host, device.username, password)
    if not isapi_found:
        print("  No ISAPI access found on any port")

    print("\n2. Trying SDK for disk info...")
    await try_sdk(device.host, device.port, device.username, password)

    await engine.dispose()


if __name__ == "__main__":
    asyncio.run(main())
