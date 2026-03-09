# Hikvision SDK Transport Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implement SDK-based transport for Hikvision NVR devices using ctypes, so devices without HTTP/HTTPS access can be monitored via the Device Network SDK.

**Architecture:** A ctypes binding module (`sdk_bindings.py`) wraps the native HCNetSDK library. `SdkTransport` implements the existing `HikvisionTransport` ABC, delegating to bindings via `asyncio.to_thread()`. The poll endpoint selects transport based on `device.transport_mode`. Frontend gets a transport_mode selector.

**Tech Stack:** Python ctypes, asyncio, FastAPI, React/TypeScript

---

### Task 1: SDK Bindings Module — ctypes structures and library loading

**Files:**
- Create: `src/cctv_monitor/drivers/hikvision/transports/sdk_bindings.py`
- Test: `tests/unit/drivers/hikvision/transports/test_sdk_bindings.py`

**Context:**
- The SDK header is at `docs/vendors/hikvision/device-network-sdk/linux-64/incEn/HCNetSDK.h`
- Windows DLL: `HCNetSDK.dll`, Linux SO: `libhcnetsdk.so`
- Key constants: `NET_DVR_DEV_ADDRESS_MAX_LEN=129`, `NET_DVR_LOGIN_USERNAME_MAX_LEN=64`, `NET_DVR_LOGIN_PASSWD_MAX_LEN=64`, `SERIALNO_LEN=48`, `MAX_DISKNUM_V30=33`, `NAME_LEN=32`, `DEV_TYPE_NAME_LEN=64`
- Command codes: `NET_DVR_GET_DEVICECFG_V40=1100`, `NET_DVR_GET_HDCFG=1054`
- Error codes: 0=no error, 1=password error, 3=not initialized, 7=connect failed

**Step 1: Write the failing test**

```python
# tests/unit/drivers/hikvision/transports/test_sdk_bindings.py
"""Tests for SDK bindings — uses a mock ctypes library."""
import ctypes
from unittest.mock import MagicMock, patch
import pytest

from cctv_monitor.drivers.hikvision.transports.sdk_bindings import (
    NET_DVR_DEVICEINFO_V30,
    NET_DVR_DEVICEINFO_V40,
    NET_DVR_USER_LOGIN_INFO,
    HCNetSDKBinding,
)


def test_deviceinfo_v30_struct_size():
    """V30 struct must have expected fields."""
    info = NET_DVR_DEVICEINFO_V30()
    assert hasattr(info, "sSerialNumber")
    assert hasattr(info, "byDiskNum")
    assert hasattr(info, "byChanNum")
    assert hasattr(info, "byIPChanNum")
    assert hasattr(info, "byStartChan")


def test_deviceinfo_v40_struct_contains_v30():
    """V40 wraps V30."""
    info = NET_DVR_DEVICEINFO_V40()
    assert hasattr(info, "struDeviceV30")
    assert isinstance(info.struDeviceV30, NET_DVR_DEVICEINFO_V30)


def test_login_info_struct_fields():
    """Login info struct must have address, port, user, password."""
    login = NET_DVR_USER_LOGIN_INFO()
    assert hasattr(login, "sDeviceAddress")
    assert hasattr(login, "wPort")
    assert hasattr(login, "sUserName")
    assert hasattr(login, "sPassword")
    assert hasattr(login, "bUseAsynLogin")


def test_binding_init_calls_sdk_init():
    """HCNetSDKBinding.init() must call NET_DVR_Init."""
    mock_lib = MagicMock()
    mock_lib.NET_DVR_Init.return_value = True
    binding = HCNetSDKBinding(lib=mock_lib)
    binding.init()
    mock_lib.NET_DVR_Init.assert_called_once()


def test_binding_cleanup_calls_sdk_cleanup():
    """HCNetSDKBinding.cleanup() must call NET_DVR_Cleanup."""
    mock_lib = MagicMock()
    mock_lib.NET_DVR_Cleanup.return_value = True
    binding = HCNetSDKBinding(lib=mock_lib)
    binding.cleanup()
    mock_lib.NET_DVR_Cleanup.assert_called_once()


def test_binding_login_success():
    """Login returns user_id and device info dict on success."""
    mock_lib = MagicMock()
    mock_lib.NET_DVR_Login_V40.return_value = 1  # user_id=1

    binding = HCNetSDKBinding(lib=mock_lib)
    user_id, info = binding.login("192.168.1.100", 8000, "admin", "password123")

    assert user_id == 1
    assert mock_lib.NET_DVR_Login_V40.called


def test_binding_login_failure_raises():
    """Login raises SdkError when SDK returns -1."""
    mock_lib = MagicMock()
    mock_lib.NET_DVR_Login_V40.return_value = -1
    mock_lib.NET_DVR_GetLastError.return_value = 7  # connect failed

    binding = HCNetSDKBinding(lib=mock_lib)

    from cctv_monitor.drivers.hikvision.errors import SdkError
    with pytest.raises(SdkError, match="SDK error 7"):
        binding.login("192.168.1.100", 8000, "admin", "password123")


def test_binding_logout():
    """Logout calls NET_DVR_Logout."""
    mock_lib = MagicMock()
    mock_lib.NET_DVR_Logout.return_value = True
    binding = HCNetSDKBinding(lib=mock_lib)
    binding.logout(1)
    mock_lib.NET_DVR_Logout.assert_called_once_with(1)
```

**Step 2: Run test to verify it fails**

Run: `python -m pytest tests/unit/drivers/hikvision/transports/test_sdk_bindings.py -v`
Expected: FAIL with ImportError (module doesn't exist yet)

**Step 3: Write minimal implementation**

```python
# src/cctv_monitor/drivers/hikvision/transports/sdk_bindings.py
"""Low-level ctypes wrapper around Hikvision HCNetSDK native library."""

from __future__ import annotations

import ctypes
import platform
import sys
from ctypes import (
    POINTER,
    Structure,
    c_bool,
    c_byte,
    c_char,
    c_int,
    c_long,
    c_uint,
    c_ushort,
)
from pathlib import Path
from typing import Any

from cctv_monitor.drivers.hikvision.errors import SdkError

# ----- Constants from HCNetSDK.h -----
NET_DVR_DEV_ADDRESS_MAX_LEN = 129
NET_DVR_LOGIN_USERNAME_MAX_LEN = 64
NET_DVR_LOGIN_PASSWD_MAX_LEN = 64
SERIALNO_LEN = 48
NAME_LEN = 32
DEV_TYPE_NAME_LEN = 64
MAX_DISKNUM_V30 = 33

# Command codes for NET_DVR_GetDVRConfig
NET_DVR_GET_DEVICECFG_V40 = 1100
NET_DVR_GET_HDCFG = 1054

# Disk status codes
DISK_STATUS_NAMES = {
    0: "normal",
    1: "raw",
    2: "error",
    3: "smart_failed",
    4: "unmatched",
    5: "sleep",
    6: "net_disk_offline",
    10: "repairing",
    11: "formatting",
    17: "warning",
    18: "bad_disk",
}


# ----- ctypes Structures -----

class NET_DVR_DEVICEINFO_V30(Structure):
    _fields_ = [
        ("sSerialNumber", c_byte * SERIALNO_LEN),
        ("byAlarmInPortNum", c_byte),
        ("byAlarmOutPortNum", c_byte),
        ("byDiskNum", c_byte),
        ("byDVRType", c_byte),
        ("byChanNum", c_byte),
        ("byStartChan", c_byte),
        ("byAudioChanNum", c_byte),
        ("byIPChanNum", c_byte),
        ("byZeroChanNum", c_byte),
        ("byMainProto", c_byte),
        ("bySubProto", c_byte),
        ("bySupport", c_byte),
        ("bySupport1", c_byte),
        ("bySupport2", c_byte),
        ("wDevType", c_ushort),
        ("bySupport3", c_byte),
        ("byMultiStreamProto", c_byte),
        ("byStartDChan", c_byte),
        ("byStartDTalkChan", c_byte),
        ("byHighDChanNum", c_byte),
        ("bySupport4", c_byte),
        ("byLanguageType", c_byte),
        ("byVoiceInChanNum", c_byte),
        ("byStartVoiceInChanNo", c_byte),
        ("bySupport5", c_byte),
        ("bySupport6", c_byte),
        ("byMirrorChanNum", c_byte),
        ("wStartMirrorChanNo", c_ushort),
        ("bySupport7", c_byte),
        ("byRes2", c_byte),
    ]


class NET_DVR_DEVICEINFO_V40(Structure):
    _fields_ = [
        ("struDeviceV30", NET_DVR_DEVICEINFO_V30),
        ("bySupportLock", c_byte),
        ("byRetryLoginTime", c_byte),
        ("byPasswordLevel", c_byte),
        ("byProxyType", c_byte),
        ("dwSurplusLockTime", c_uint),
        ("byCharEncodeType", c_byte),
        ("bySupportDev5", c_byte),
        ("bySupport", c_byte),
        ("byLoginMode", c_byte),
        ("dwOEMCode", c_uint),
        ("iResidualValidity", c_int),
        ("byResidualValidity", c_byte),
        ("bySingleStartDTalkChan", c_byte),
        ("bySingleDTalkChanNums", c_byte),
        ("byPassWordResetLevel", c_byte),
        ("bySupportStreamEncrypt", c_byte),
        ("byMarketType", c_byte),
        ("byTLSCap", c_byte),
        ("byRes2", c_byte * 237),
    ]


class NET_DVR_USER_LOGIN_INFO(Structure):
    _fields_ = [
        ("sDeviceAddress", c_char * NET_DVR_DEV_ADDRESS_MAX_LEN),
        ("byUseTransport", c_byte),
        ("wPort", c_ushort),
        ("sUserName", c_char * NET_DVR_LOGIN_USERNAME_MAX_LEN),
        ("sPassword", c_char * NET_DVR_LOGIN_PASSWD_MAX_LEN),
        ("cbLoginResult", ctypes.c_void_p),
        ("pUser", ctypes.c_void_p),
        ("bUseAsynLogin", c_int),
        ("byProxyType", c_byte),
        ("byUseUTCTime", c_byte),
        ("byLoginMode", c_byte),
        ("byHttps", c_byte),
        ("iProxyID", c_long),
        ("byVerifyMode", c_byte),
        ("byRes3", c_byte * 119),
    ]


class NET_DVR_SINGLE_HD(Structure):
    _fields_ = [
        ("dwHDNo", c_uint),
        ("dwCapacity", c_uint),
        ("dwFreeSpace", c_uint),
        ("dwHdStatus", c_uint),
        ("byHDAttr", c_byte),
        ("byHDType", c_byte),
        ("byDiskDriver", c_byte),
        ("byRes1", c_byte),
        ("dwHdGroup", c_uint),
        ("byRecycling", c_byte),
        ("bySupportFormatType", c_byte),
        ("byFormatType", c_byte),
        ("byRes2", c_byte),
        ("dwStorageType", c_uint),
        ("dwPictureCapacity", c_uint),
        ("dwFreePictureSpace", c_uint),
        ("byRes3", c_byte * 104),
    ]


class NET_DVR_HDCFG(Structure):
    _fields_ = [
        ("dwSize", c_uint),
        ("dwHDCount", c_uint),
        ("struHDInfo", NET_DVR_SINGLE_HD * MAX_DISKNUM_V30),
    ]


class NET_DVR_DEVICECFG_V40(Structure):
    _fields_ = [
        ("dwSize", c_uint),
        ("sDVRName", c_byte * NAME_LEN),
        ("dwDVRID", c_uint),
        ("dwRecycleRecord", c_uint),
        ("sSerialNumber", c_byte * SERIALNO_LEN),
        ("dwSoftwareVersion", c_uint),
        ("dwSoftwareBuildDate", c_uint),
        ("dwDSPSoftwareVersion", c_uint),
        ("dwDSPSoftwareBuildDate", c_uint),
        ("dwPanelVersion", c_uint),
        ("dwHardwareVersion", c_uint),
        ("byAlarmInPortNum", c_byte),
        ("byAlarmOutPortNum", c_byte),
        ("byRS232Num", c_byte),
        ("byRS485Num", c_byte),
        ("byNetworkPortNum", c_byte),
        ("byDiskCtrlNum", c_byte),
        ("byDiskNum", c_byte),
        ("byDVRType", c_byte),
        ("byChanNum", c_byte),
        ("byStartChan", c_byte),
        ("byDecordChans", c_byte),
        ("byVGANum", c_byte),
        ("byUSBNum", c_byte),
        ("byAuxoutNum", c_byte),
        ("byAudioNum", c_byte),
        ("byIPChanNum", c_byte),
        ("byZeroChanNum", c_byte),
        ("bySupport", c_byte),
        ("byEsataUseage", c_byte),
        ("byIPCPlug", c_byte),
        ("byStorageMode", c_byte),
        ("bySupport1", c_byte),
        ("wDevType", c_ushort),
        ("byDevTypeName", c_byte * DEV_TYPE_NAME_LEN),
        ("bySupport2", c_byte),
        ("byAnalogAlarmInPortNum", c_byte),
        ("byStartAlarmInNo", c_byte),
        ("byStartAlarmOutNo", c_byte),
        ("byStartIPAlarmInNo", c_byte),
        ("byStartIPAlarmOutNo", c_byte),
        ("byHighIPChanNum", c_byte),
        ("byEnableRemotePowerOn", c_byte),
        ("wDevClass", c_ushort),
        ("byRes2", c_byte * 6),
    ]


# ----- SDK Binding Class -----

def _load_sdk_library(lib_path: str | None = None) -> ctypes.CDLL:
    """Load the HCNetSDK native library."""
    if lib_path:
        return ctypes.cdll.LoadLibrary(lib_path)

    system = platform.system()
    if system == "Windows":
        return ctypes.cdll.LoadLibrary("HCNetSDK.dll")
    elif system == "Linux":
        return ctypes.cdll.LoadLibrary("libhcnetsdk.so")
    else:
        raise OSError(f"Unsupported platform: {system}")


def _bytes_to_str(byte_array: Any) -> str:
    """Convert ctypes byte array to Python string."""
    return bytes(byte_array).split(b"\x00", 1)[0].decode("utf-8", errors="replace")


class HCNetSDKBinding:
    """Thin wrapper around HCNetSDK C functions.

    All methods are synchronous (blocking). Use asyncio.to_thread() to
    call from async code.
    """

    def __init__(self, lib: Any | None = None, lib_path: str | None = None) -> None:
        """Initialize binding. Pass `lib` for testing (mock), or `lib_path` for production."""
        self._lib = lib if lib is not None else _load_sdk_library(lib_path)

    def init(self) -> None:
        """Call NET_DVR_Init(). Must be called once before any other SDK calls."""
        result = self._lib.NET_DVR_Init()
        if not result:
            raise SdkError(device_id="system", error_code=self.get_last_error(), message="NET_DVR_Init failed")

    def cleanup(self) -> None:
        """Call NET_DVR_Cleanup(). Must be called once when done with SDK."""
        self._lib.NET_DVR_Cleanup()

    def get_last_error(self) -> int:
        """Return the last SDK error code."""
        return self._lib.NET_DVR_GetLastError()

    def login(self, host: str, port: int, username: str, password: str) -> tuple[int, dict]:
        """Login to device. Returns (user_id, device_info_dict).

        Raises SdkError on failure.
        """
        login_info = NET_DVR_USER_LOGIN_INFO()
        device_info = NET_DVR_DEVICEINFO_V40()

        login_info.sDeviceAddress = host.encode("utf-8")
        login_info.wPort = port
        login_info.sUserName = username.encode("utf-8")
        login_info.sPassword = password.encode("utf-8")
        login_info.bUseAsynLogin = 0  # synchronous login

        user_id = self._lib.NET_DVR_Login_V40(
            ctypes.byref(login_info),
            ctypes.byref(device_info),
        )

        if user_id < 0:
            error_code = self.get_last_error()
            raise SdkError(
                device_id=f"{host}:{port}",
                error_code=error_code,
                message=f"Login failed to {host}:{port}",
            )

        v30 = device_info.struDeviceV30
        info_dict = {
            "serial_number": _bytes_to_str(v30.sSerialNumber),
            "disk_num": int(v30.byDiskNum),
            "chan_num": int(v30.byChanNum),
            "start_chan": int(v30.byStartChan),
            "ip_chan_num": int(v30.byIPChanNum),
            "dvr_type": int(v30.byDVRType),
            "char_encode_type": int(device_info.byCharEncodeType),
        }

        return user_id, info_dict

    def logout(self, user_id: int) -> None:
        """Logout from device."""
        self._lib.NET_DVR_Logout(user_id)

    def get_device_config(self, user_id: int) -> dict:
        """Get extended device configuration (NET_DVR_DEVICECFG_V40)."""
        cfg = NET_DVR_DEVICECFG_V40()
        cfg.dwSize = ctypes.sizeof(cfg)
        bytes_returned = c_uint(0)

        result = self._lib.NET_DVR_GetDVRConfig(
            user_id,
            NET_DVR_GET_DEVICECFG_V40,
            0,  # channel 0 = device-level
            ctypes.byref(cfg),
            ctypes.sizeof(cfg),
            ctypes.byref(bytes_returned),
        )

        if not result:
            error_code = self.get_last_error()
            raise SdkError(
                device_id=f"user:{user_id}",
                error_code=error_code,
                message="GetDVRConfig(DEVICECFG_V40) failed",
            )

        # Parse software version: high 16 bits = major, low 16 bits = minor
        sw = cfg.dwSoftwareVersion
        sw_major = (sw >> 24) & 0xFF
        sw_minor = (sw >> 16) & 0xFF
        sw_build = sw & 0xFFFF

        return {
            "device_name": _bytes_to_str(cfg.sDVRName),
            "serial_number": _bytes_to_str(cfg.sSerialNumber),
            "firmware_version": f"V{sw_major}.{sw_minor}.{sw_build}",
            "device_type": int(cfg.byDVRType),
            "device_type_name": _bytes_to_str(cfg.byDevTypeName),
            "chan_num": int(cfg.byChanNum),
            "ip_chan_num": int(cfg.byIPChanNum),
            "disk_num": int(cfg.byDiskNum),
            "start_chan": int(cfg.byStartChan),
        }

    def get_hdd_config(self, user_id: int) -> list[dict]:
        """Get HDD configuration (NET_DVR_HDCFG)."""
        hdd_cfg = NET_DVR_HDCFG()
        hdd_cfg.dwSize = ctypes.sizeof(hdd_cfg)
        bytes_returned = c_uint(0)

        result = self._lib.NET_DVR_GetDVRConfig(
            user_id,
            NET_DVR_GET_HDCFG,
            0,
            ctypes.byref(hdd_cfg),
            ctypes.sizeof(hdd_cfg),
            ctypes.byref(bytes_returned),
        )

        if not result:
            error_code = self.get_last_error()
            raise SdkError(
                device_id=f"user:{user_id}",
                error_code=error_code,
                message="GetDVRConfig(HDCFG) failed",
            )

        disks = []
        for i in range(hdd_cfg.dwHDCount):
            hd = hdd_cfg.struHDInfo[i]
            status_code = int(hd.dwHdStatus)
            disks.append({
                "disk_id": str(hd.dwHDNo),
                "capacity_mb": int(hd.dwCapacity),
                "free_space_mb": int(hd.dwFreeSpace),
                "status_code": status_code,
                "status_name": DISK_STATUS_NAMES.get(status_code, f"unknown_{status_code}"),
                "hd_type": int(hd.byHDType),
                "recycling": bool(hd.byRecycling),
            })
        return disks
```

**Step 4: Run test to verify it passes**

Run: `python -m pytest tests/unit/drivers/hikvision/transports/test_sdk_bindings.py -v`
Expected: All 7 tests PASS

**Step 5: Commit**

```bash
git add src/cctv_monitor/drivers/hikvision/transports/sdk_bindings.py tests/unit/drivers/hikvision/transports/test_sdk_bindings.py
git commit -m "feat: add SDK bindings module with ctypes structures and HCNetSDKBinding class"
```

---

### Task 2: Implement SdkTransport (rewrite stub)

**Files:**
- Modify: `src/cctv_monitor/drivers/hikvision/transports/sdk.py`
- Test: `tests/unit/drivers/hikvision/transports/test_sdk_transport.py`

**Context:**
- `SdkTransport` must implement `HikvisionTransport` ABC from `base.py`
- Methods: `connect`, `disconnect`, `get_device_info`, `get_channels_status`, `get_video_inputs`, `get_disk_status`, `get_recording_status`, `get_snapshot`
- All calls go through `asyncio.to_thread()` because SDK is blocking
- Returns data as dicts, matching format expected by `HikvisionDriver`
- The driver currently calls `HikvisionMapper.parse_device_info(raw["raw_xml"], ...)` for ISAPI. For SDK we need a different key so the driver knows to use a different parsing path.

**Step 1: Write the failing test**

```python
# tests/unit/drivers/hikvision/transports/test_sdk_transport.py
"""Tests for SdkTransport — async wrapper over SDK bindings."""
import pytest
from unittest.mock import MagicMock, AsyncMock, patch

from cctv_monitor.drivers.hikvision.transports.sdk import SdkTransport


@pytest.fixture
def mock_binding():
    binding = MagicMock()
    binding.login.return_value = (
        1,
        {
            "serial_number": "DS-7608NI-K2",
            "disk_num": 2,
            "chan_num": 0,
            "start_chan": 1,
            "ip_chan_num": 8,
            "dvr_type": 76,
            "char_encode_type": 6,
        },
    )
    binding.get_device_config.return_value = {
        "device_name": "NVR Building 1",
        "serial_number": "DS-7608NI-K2123456",
        "firmware_version": "V4.30.085",
        "device_type": 76,
        "device_type_name": "DS-7608NI-K2",
        "chan_num": 0,
        "ip_chan_num": 8,
        "disk_num": 2,
        "start_chan": 1,
    }
    binding.get_hdd_config.return_value = [
        {
            "disk_id": "1",
            "capacity_mb": 1907729,
            "free_space_mb": 500000,
            "status_code": 0,
            "status_name": "normal",
            "hd_type": 0,
            "recycling": True,
        }
    ]
    return binding


@pytest.mark.asyncio
async def test_connect_calls_login(mock_binding):
    transport = SdkTransport(binding=mock_binding)
    await transport.connect("192.168.1.100", 8000, "admin", "pass123")
    mock_binding.login.assert_called_once_with("192.168.1.100", 8000, "admin", "pass123")


@pytest.mark.asyncio
async def test_disconnect_calls_logout(mock_binding):
    transport = SdkTransport(binding=mock_binding)
    await transport.connect("192.168.1.100", 8000, "admin", "pass123")
    await transport.disconnect()
    mock_binding.logout.assert_called_once_with(1)


@pytest.mark.asyncio
async def test_get_device_info_returns_sdk_data(mock_binding):
    transport = SdkTransport(binding=mock_binding)
    await transport.connect("192.168.1.100", 8000, "admin", "pass123")
    result = await transport.get_device_info()
    assert "sdk_data" in result
    assert result["sdk_data"]["device_name"] == "NVR Building 1"
    assert result["sdk_data"]["firmware_version"] == "V4.30.085"


@pytest.mark.asyncio
async def test_get_disk_status_returns_list(mock_binding):
    transport = SdkTransport(binding=mock_binding)
    await transport.connect("192.168.1.100", 8000, "admin", "pass123")
    result = await transport.get_disk_status()
    assert len(result) == 1
    assert "sdk_data" in result[0]
    assert result[0]["sdk_data"]["capacity_mb"] == 1907729


@pytest.mark.asyncio
async def test_get_channels_status_from_login_info(mock_binding):
    transport = SdkTransport(binding=mock_binding)
    await transport.connect("192.168.1.100", 8000, "admin", "pass123")
    result = await transport.get_channels_status()
    assert isinstance(result, list)
    # ip_chan_num=8 from login, so 8 channels
    assert len(result) == 8


@pytest.mark.asyncio
async def test_get_snapshot_raises_not_implemented(mock_binding):
    transport = SdkTransport(binding=mock_binding)
    await transport.connect("192.168.1.100", 8000, "admin", "pass123")
    with pytest.raises(NotImplementedError):
        await transport.get_snapshot("1")
```

**Step 2: Run test to verify it fails**

Run: `python -m pytest tests/unit/drivers/hikvision/transports/test_sdk_transport.py -v`
Expected: FAIL (SdkTransport still has stub implementation)

**Step 3: Write minimal implementation**

```python
# src/cctv_monitor/drivers/hikvision/transports/sdk.py
"""SDK transport for Hikvision devices — uses ctypes bindings."""

from __future__ import annotations

import asyncio
from typing import Any

from cctv_monitor.drivers.hikvision.transports.base import HikvisionTransport
from cctv_monitor.drivers.hikvision.transports.sdk_bindings import HCNetSDKBinding


class SdkTransport(HikvisionTransport):
    """Hikvision transport using Device Network SDK via ctypes.

    All SDK calls are blocking, so they run in a thread via asyncio.to_thread().
    """

    def __init__(self, binding: HCNetSDKBinding | None = None) -> None:
        self._binding = binding or HCNetSDKBinding()
        self._user_id: int = -1
        self._device_id: str = ""
        self._login_info: dict[str, Any] = {}

    async def connect(self, host: str, port: int, username: str, password: str) -> None:
        self._device_id = f"{host}:{port}"
        self._user_id, self._login_info = await asyncio.to_thread(
            self._binding.login, host, port, username, password
        )

    async def disconnect(self) -> None:
        if self._user_id >= 0:
            await asyncio.to_thread(self._binding.logout, self._user_id)
            self._user_id = -1

    async def get_device_info(self) -> dict:
        cfg = await asyncio.to_thread(self._binding.get_device_config, self._user_id)
        return {"sdk_data": cfg}

    async def get_channels_status(self) -> list[dict]:
        """Return channel info from login device info.

        SDK doesn't have a single "channel status" API like ISAPI.
        We derive basic channel list from login info (count + start channel).
        """
        ip_chan = self._login_info.get("ip_chan_num", 0)
        analog_chan = self._login_info.get("chan_num", 0)
        start = self._login_info.get("start_chan", 1)

        channels = []
        for i in range(analog_chan + ip_chan):
            chan_id = str(start + i)
            channels.append({
                "sdk_data": {
                    "channel_id": chan_id,
                    "channel_name": f"Channel {chan_id}",
                    "source": "sdk_login_info",
                }
            })
        return channels

    async def get_video_inputs(self) -> dict:
        raise NotImplementedError("SDK get_video_inputs not implemented")

    async def get_disk_status(self) -> list[dict]:
        disks = await asyncio.to_thread(self._binding.get_hdd_config, self._user_id)
        return [{"sdk_data": d} for d in disks]

    async def get_recording_status(self) -> list[dict]:
        raise NotImplementedError("SDK get_recording_status not implemented")

    async def get_snapshot(self, channel_id: str) -> bytes:
        raise NotImplementedError("SDK get_snapshot not implemented")
```

**Step 4: Run test to verify it passes**

Run: `python -m pytest tests/unit/drivers/hikvision/transports/test_sdk_transport.py -v`
Expected: All 6 tests PASS

**Step 5: Commit**

```bash
git add src/cctv_monitor/drivers/hikvision/transports/sdk.py tests/unit/drivers/hikvision/transports/test_sdk_transport.py
git commit -m "feat: implement SdkTransport with async wrapping over SDK bindings"
```

---

### Task 3: Extend HikvisionDriver mapper for SDK data

**Files:**
- Modify: `src/cctv_monitor/drivers/hikvision/mappers.py`
- Modify: `src/cctv_monitor/drivers/hikvision/driver.py`
- Test: `tests/unit/drivers/hikvision/test_sdk_mapper.py`

**Context:**
- `HikvisionDriver.get_device_info()` currently does `HikvisionMapper.parse_device_info(raw["raw_xml"], ...)` — only works for ISAPI XML
- `SdkTransport` returns `{"sdk_data": {...}}` instead of `{"raw_xml": "..."}`
- The driver needs to detect the format and use the right parser
- Same for `get_camera_statuses()` and `get_disk_statuses()`

**Step 1: Write the failing test**

```python
# tests/unit/drivers/hikvision/test_sdk_mapper.py
"""Tests for SDK data mapping in HikvisionMapper."""
from datetime import datetime, timezone
import pytest

from cctv_monitor.drivers.hikvision.mappers import HikvisionMapper
from cctv_monitor.core.types import CameraStatus, DiskStatus


def test_parse_device_info_from_sdk():
    sdk_data = {
        "device_name": "NVR Building 1",
        "serial_number": "DS-7608NI-K2123456",
        "firmware_version": "V4.30.085",
        "device_type": 76,
        "device_type_name": "DS-7608NI-K2",
        "chan_num": 0,
        "ip_chan_num": 8,
        "disk_num": 2,
        "start_chan": 1,
    }
    info = HikvisionMapper.parse_device_info_sdk(sdk_data, "nvr-1")
    assert info.device_id == "nvr-1"
    assert info.model == "DS-7608NI-K2"
    assert info.serial_number == "DS-7608NI-K2123456"
    assert info.firmware_version == "V4.30.085"
    assert info.channels_count == 8


def test_parse_channels_from_sdk():
    sdk_channels = [
        {"channel_id": "1", "channel_name": "Channel 1", "source": "sdk_login_info"},
        {"channel_id": "2", "channel_name": "Channel 2", "source": "sdk_login_info"},
    ]
    now = datetime.now(timezone.utc)
    statuses = HikvisionMapper.parse_channels_sdk(sdk_channels, "nvr-1", now)
    assert len(statuses) == 2
    # SDK doesn't report online/offline per channel from login info, default to UNKNOWN
    assert statuses[0].channel_id == "1"


def test_parse_disk_status_from_sdk():
    sdk_disk = {
        "disk_id": "1",
        "capacity_mb": 1907729,
        "free_space_mb": 500000,
        "status_code": 0,
        "status_name": "normal",
        "hd_type": 0,
        "recycling": True,
    }
    now = datetime.now(timezone.utc)
    disks = HikvisionMapper.parse_disk_status_sdk([sdk_disk], "nvr-1", now)
    assert len(disks) == 1
    assert disks[0].status == DiskStatus.OK
    assert disks[0].capacity_bytes == 1907729 * 1024 * 1024
    assert disks[0].free_bytes == 500000 * 1024 * 1024


def test_parse_disk_error_status():
    sdk_disk = {
        "disk_id": "2",
        "capacity_mb": 0,
        "free_space_mb": 0,
        "status_code": 2,
        "status_name": "error",
        "hd_type": 0,
        "recycling": False,
    }
    now = datetime.now(timezone.utc)
    disks = HikvisionMapper.parse_disk_status_sdk([sdk_disk], "nvr-1", now)
    assert disks[0].status == DiskStatus.ERROR
```

**Step 2: Run test to verify it fails**

Run: `python -m pytest tests/unit/drivers/hikvision/test_sdk_mapper.py -v`
Expected: FAIL with AttributeError (methods don't exist yet)

**Step 3: Add SDK parsing methods to HikvisionMapper**

Add the following methods to `HikvisionMapper` class in `src/cctv_monitor/drivers/hikvision/mappers.py`:

```python
    # Add these to the HikvisionMapper class after the existing methods:

    @staticmethod
    def parse_device_info_sdk(sdk_data: dict, device_id: DeviceId) -> DeviceInfo:
        """Parse SDK device config into DeviceInfo."""
        return DeviceInfo(
            device_id=device_id,
            model=sdk_data.get("device_type_name", ""),
            serial_number=sdk_data.get("serial_number", ""),
            firmware_version=sdk_data.get("firmware_version", ""),
            device_type=str(sdk_data.get("device_type", "")),
            mac_address=None,
            channels_count=sdk_data.get("ip_chan_num", 0) + sdk_data.get("chan_num", 0),
        )

    @staticmethod
    def parse_channels_sdk(
        sdk_channels: list[dict], device_id: DeviceId, checked_at: datetime
    ) -> list[CameraChannelStatus]:
        """Parse SDK channel list into CameraChannelStatus list."""
        from cctv_monitor.core.types import CameraStatus

        return [
            CameraChannelStatus(
                device_id=device_id,
                channel_id=ch.get("channel_id", ""),
                channel_name=ch.get("channel_name", ""),
                status=CameraStatus.UNKNOWN,
                ip_address=None,
                protocol=None,
                checked_at=checked_at,
            )
            for ch in sdk_channels
        ]

    @staticmethod
    def parse_disk_status_sdk(
        sdk_disks: list[dict], device_id: DeviceId, checked_at: datetime
    ) -> list[DiskHealthStatus]:
        """Parse SDK HDD config into DiskHealthStatus list."""
        _SDK_STATUS_MAP = {
            "normal": DiskStatus.OK,
            "error": DiskStatus.ERROR,
            "smart_failed": DiskStatus.ERROR,
            "bad_disk": DiskStatus.ERROR,
            "warning": DiskStatus.WARNING,
            "raw": DiskStatus.WARNING,
            "unmatched": DiskStatus.WARNING,
            "sleep": DiskStatus.OK,
        }

        return [
            DiskHealthStatus(
                device_id=device_id,
                disk_id=d.get("disk_id", ""),
                status=_SDK_STATUS_MAP.get(d.get("status_name", ""), DiskStatus.UNKNOWN),
                capacity_bytes=d.get("capacity_mb", 0) * 1024 * 1024,
                free_bytes=d.get("free_space_mb", 0) * 1024 * 1024,
                health_status=d.get("status_name", "unknown"),
                checked_at=checked_at,
            )
            for d in sdk_disks
        ]
```

Then update `HikvisionDriver` methods in `src/cctv_monitor/drivers/hikvision/driver.py` to detect SDK vs ISAPI data:

Line 42: Change `get_device_info()`:
```python
    async def get_device_info(self) -> DeviceInfo:
        raw = await self._transport.get_device_info()
        if "sdk_data" in raw:
            return HikvisionMapper.parse_device_info_sdk(raw["sdk_data"], self.device_id)
        return HikvisionMapper.parse_device_info(raw["raw_xml"], self.device_id)
```

Lines 44-65: Change `get_camera_statuses()`:
```python
    async def get_camera_statuses(self) -> list[CameraChannelStatus]:
        now = datetime.now(timezone.utc)

        raw_list = await self._transport.get_channels_status()

        # SDK path
        if raw_list and "sdk_data" in raw_list[0]:
            return HikvisionMapper.parse_channels_sdk(
                [r["sdk_data"] for r in raw_list], self.device_id, now
            )

        # ISAPI path (existing)
        all_statuses: list[CameraChannelStatus] = []
        for raw in raw_list:
            all_statuses.extend(
                HikvisionMapper.parse_channels_status(raw["raw_xml"], self.device_id, now)
            )

        if not all_statuses:
            try:
                raw = await self._transport.get_video_inputs()
                all_statuses = HikvisionMapper.parse_video_inputs(
                    raw["raw_xml"], self.device_id, now
                )
            except Exception:
                pass

        return all_statuses
```

Lines 67-75: Change `get_disk_statuses()`:
```python
    async def get_disk_statuses(self) -> list[DiskHealthStatus]:
        raw_list = await self._transport.get_disk_status()
        now = datetime.now(timezone.utc)

        # SDK path
        if raw_list and "sdk_data" in raw_list[0]:
            return HikvisionMapper.parse_disk_status_sdk(
                [r["sdk_data"] for r in raw_list], self.device_id, now
            )

        # ISAPI path (existing)
        all_disks: list[DiskHealthStatus] = []
        for raw in raw_list:
            all_disks.extend(
                HikvisionMapper.parse_disk_status(raw["raw_xml"], self.device_id, now)
            )
        return all_disks
```

**Step 4: Run test to verify it passes**

Run: `python -m pytest tests/unit/drivers/hikvision/test_sdk_mapper.py -v`
Expected: All 4 tests PASS

Run: `python -m pytest tests/ -v`
Expected: All existing tests still pass

**Step 5: Commit**

```bash
git add src/cctv_monitor/drivers/hikvision/mappers.py src/cctv_monitor/drivers/hikvision/driver.py tests/unit/drivers/hikvision/test_sdk_mapper.py
git commit -m "feat: add SDK data parsing to HikvisionMapper and driver"
```

---

### Task 4: Add HCNETSDK_LIB_PATH to Settings and SDK lifecycle to main.py

**Files:**
- Modify: `src/cctv_monitor/core/config.py:11` (add setting)
- Modify: `src/cctv_monitor/main.py` (SDK init/cleanup)

**Step 1: Add setting**

In `src/cctv_monitor/core/config.py`, add after line 11:

```python
    HCNETSDK_LIB_PATH: str | None = None
```

**Step 2: Add SDK lifecycle to main.py**

In `src/cctv_monitor/main.py`, add SDK init after driver registry setup (after line 55) and cleanup in finally block:

After imports, add:
```python
from cctv_monitor.drivers.hikvision.transports.sdk_bindings import HCNetSDKBinding
```

After line 55 (`registry.register(...)`), add:
```python
    # SDK lifecycle
    sdk_binding: HCNetSDKBinding | None = None
    if settings.HCNETSDK_LIB_PATH:
        try:
            sdk_binding = HCNetSDKBinding(lib_path=settings.HCNETSDK_LIB_PATH)
            sdk_binding.init()
            logger.info("sdk.initialized", path=settings.HCNETSDK_LIB_PATH)
        except Exception as exc:
            logger.warning("sdk.init_failed", error=str(exc))
            sdk_binding = None
```

After line 69 (`app.state.scheduler = scheduler`), add:
```python
    app.state.sdk_binding = sdk_binding
```

In the finally block, before `scheduler.shutdown()`, add:
```python
        if sdk_binding:
            sdk_binding.cleanup()
            logger.info("sdk.cleaned_up")
```

**Step 3: Commit**

```bash
git add src/cctv_monitor/core/config.py src/cctv_monitor/main.py
git commit -m "feat: add HCNETSDK_LIB_PATH setting and SDK lifecycle management"
```

---

### Task 5: Transport selection in poll endpoint

**Files:**
- Modify: `src/cctv_monitor/api/routes/devices.py:120-163` (poll endpoint)
- Modify: `src/cctv_monitor/api/deps.py` (add get_sdk_binding dependency)
- Test: `tests/unit/api/test_device_routes.py` (add transport selection test)

**Context:**
- Currently line 135: `transport = IsapiTransport(http_client)` — hardcoded
- Need to check `device.transport_mode` and select accordingly
- `sdk_binding` is on `app.state.sdk_binding`

**Step 1: Add dependency for sdk_binding**

In `src/cctv_monitor/api/deps.py`, add:
```python
from cctv_monitor.drivers.hikvision.transports.sdk_bindings import HCNetSDKBinding

def get_sdk_binding(request: Request) -> HCNetSDKBinding | None:
    return getattr(request.app.state, "sdk_binding", None)
```

**Step 2: Update poll endpoint**

In `src/cctv_monitor/api/routes/devices.py`, update the poll endpoint:

Add import:
```python
from cctv_monitor.api.deps import get_sdk_binding
from cctv_monitor.drivers.hikvision.transports.sdk import SdkTransport
```

Update function signature to add `sdk_binding` dependency:
```python
@router.post("/devices/{device_id}/poll", response_model=PollResultOut)
async def poll_device(
    device_id: str,
    session: AsyncSession = Depends(get_session),
    settings: Settings = Depends(get_settings),
    registry: DriverRegistry = Depends(get_driver_registry),
    http_client: HttpClientManager = Depends(get_http_client),
    sdk_binding: HCNetSDKBinding | None = Depends(get_sdk_binding),
):
```

Replace transport creation (line 135) with:
```python
    transport_mode = device.transport_mode or "isapi"
    if transport_mode == "sdk":
        if sdk_binding is None:
            raise HTTPException(status_code=400, detail="SDK not configured (set HCNETSDK_LIB_PATH)")
        transport = SdkTransport(binding=sdk_binding)
    else:
        transport = IsapiTransport(http_client)
```

**Step 3: Commit**

```bash
git add src/cctv_monitor/api/routes/devices.py src/cctv_monitor/api/deps.py
git commit -m "feat: add transport selection in poll endpoint based on device transport_mode"
```

---

### Task 6: Add transport_mode to API schemas and create endpoint

**Files:**
- Modify: `src/cctv_monitor/api/schemas.py` (add transport_mode field)
- Modify: `src/cctv_monitor/api/routes/devices.py` (pass transport_mode in create/update/detail)

**Step 1: Update schemas**

In `src/cctv_monitor/api/schemas.py`:

Add to `DeviceCreate`:
```python
    transport_mode: str = "isapi"
```

Add to `DeviceUpdate`:
```python
    transport_mode: str | None = None
```

Add to `DeviceOut`:
```python
    transport_mode: str
```

**Step 2: Update routes to pass transport_mode**

In `src/cctv_monitor/api/routes/devices.py`:

Every place that constructs `DeviceOut(...)`, add `transport_mode=device.transport_mode` (or `d.transport_mode` in list).

In `create_device`: add `transport_mode=body.transport_mode` to the `DeviceTable(...)` constructor.

**Step 3: Commit**

```bash
git add src/cctv_monitor/api/schemas.py src/cctv_monitor/api/routes/devices.py
git commit -m "feat: add transport_mode to API schemas and device CRUD"
```

---

### Task 7: Frontend — transport_mode selector

**Files:**
- Modify: `frontend/src/types.ts` (add transport_mode to types)
- Modify: `frontend/src/pages/AddDevice.tsx` (add selector)
- Modify: `frontend/src/pages/EditDevice.tsx` (add selector)

**Step 1: Update TypeScript types**

In `frontend/src/types.ts`:

Add `transport_mode: string;` to `Device` interface (after `port`).
Add `transport_mode: string;` to `DeviceCreate` interface (after `password`).
Add `transport_mode?: string;` to `DeviceUpdate` interface (after `is_active`).

**Step 2: Add selector to AddDevice.tsx**

In `frontend/src/pages/AddDevice.tsx`, add `transport_mode: 'isapi'` to the initial form state.

After the Port field, add:
```tsx
          <div className="form-group">
            <label>Transport</label>
            <select name="transport_mode" value={form.transport_mode} onChange={handleChange}>
              <option value="isapi">ISAPI (HTTP/HTTPS)</option>
              <option value="sdk">SDK (Port 8000)</option>
              <option value="auto">Auto</option>
            </select>
          </div>
```

**Step 3: Add selector to EditDevice.tsx**

In `frontend/src/pages/EditDevice.tsx`:

Add `transport_mode: 'isapi'` to the form state.

In the `useEffect` that loads the device, set `transport_mode: detail.device.transport_mode || 'isapi'`.

Add the same `<select>` after the Port field.

In `handleSubmit`, add `transport_mode` to the update payload:
```typescript
      const update: DeviceUpdate = {
        name: form.name,
        host: form.host,
        port: form.port,
        transport_mode: form.transport_mode,
      };
```

**Step 4: Verify frontend builds**

Run: `cd frontend && npm run build`
Expected: Build succeeds

**Step 5: Commit**

```bash
git add frontend/src/types.ts frontend/src/pages/AddDevice.tsx frontend/src/pages/EditDevice.tsx
git commit -m "feat: add transport_mode selector to Add/Edit device forms"
```

---

### Task 8: Run all tests and verify

**Step 1: Run backend tests**

Run: `python -m pytest tests/ -v`
Expected: All tests pass

**Step 2: Run frontend build**

Run: `cd frontend && npm run build`
Expected: Build succeeds

**Step 3: Final commit (if any fixes needed)**

```bash
git add -A
git commit -m "fix: address test/build issues from SDK transport integration"
```
