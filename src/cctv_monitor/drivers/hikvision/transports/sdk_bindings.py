"""Ctypes bindings for Hikvision HCNetSDK (NET_DVR_*)."""

from __future__ import annotations

import ctypes
import sys
from ctypes import (
    Structure,
    c_byte,
    c_char,
    c_int,
    c_long,
    c_uint,
    c_uint16,
    c_ushort,
    c_void_p,
    cast,
    create_string_buffer,
)
from typing import Any

from cctv_monitor.drivers.hikvision.errors import SdkError

# ---------------------------------------------------------------------------
# Constants
# ---------------------------------------------------------------------------

NET_DVR_DEV_ADDRESS_MAX_LEN = 129
NET_DVR_LOGIN_USERNAME_MAX_LEN = 64
NET_DVR_LOGIN_PASSWD_MAX_LEN = 64
SERIALNO_LEN = 48
NAME_LEN = 32
DEV_TYPE_NAME_LEN = 64
MAX_DISKNUM_V30 = 33

NET_DVR_GET_DEVICECFG_V40 = 1100
NET_DVR_GET_HDCFG = 1054
NET_DVR_GET_HDD_SMART_INFO = 3262
NET_DVR_GET_DIGITAL_CHANNEL_STATE = 6126
NET_SDK_INIT_CFG_SDK_PATH = 2
NET_SDK_MAX_FILE_PATH = 256

# FindFile result codes
NET_DVR_FILE_SUCCESS = 1000
NET_DVR_ISFINDING = 1002
NET_DVR_NOMOREFILE = 1003
NET_DVR_FILE_EXCEPTION = 1004

MAX_CHANNUM_V30 = 64
MAX_SMART_ATTR_NUM = 30

DISK_STATUS_NAMES: dict[int, str] = {
    0: "normal",
    1: "unformatted",
    2: "error",
    3: "smart_failed",
    4: "not_match",
    5: "sleeping",
    6: "not_exist",
}

# ---------------------------------------------------------------------------
# ctypes Structures
# ---------------------------------------------------------------------------


class NET_DVR_LOCAL_SDK_PATH(Structure):
    """SDK component path — set via NET_DVR_SetSDKInitCfg BEFORE Init."""

    _fields_ = [
        ("sPath", c_char * NET_SDK_MAX_FILE_PATH),
        ("byRes", c_byte * 128),
    ]


class NET_DVR_DEVICEINFO_V30(Structure):
    """Device info returned by login (V30 layout)."""

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
        ("wDevType", c_uint16),
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
        ("wStartMirrorChanNo", c_uint16),
        ("bySupport7", c_byte),
        ("byRes2", c_byte),
    ]


class NET_DVR_DEVICEINFO_V40(Structure):
    """Extended device info (V40 login)."""

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
        ("bySingleDTalkChanNum", c_byte),
        ("byRes2", c_byte * 13),
    ]


class NET_DVR_USER_LOGIN_INFO(Structure):
    """Login parameters for NET_DVR_Login_V40."""

    _fields_ = [
        ("sDeviceAddress", c_char * NET_DVR_DEV_ADDRESS_MAX_LEN),
        ("byUseTransport", c_byte),
        ("wPort", c_uint16),
        ("sUserName", c_char * NET_DVR_LOGIN_USERNAME_MAX_LEN),
        ("sPassword", c_char * NET_DVR_LOGIN_PASSWD_MAX_LEN),
        ("cbLoginResult", c_void_p),
        ("pUser", c_void_p),
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
    """Single hard-disk info."""

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
        ("byRes2", c_byte * 3),
        ("dwStorageType", c_uint),
        ("dwPictureCapacity", c_uint),
        ("dwFreePictureSpace", c_uint),
        ("byRes3", c_byte * 104),
    ]


class NET_DVR_DIGITAL_CHANNEL_STATE(Structure):
    """Digital channel online/offline state."""

    _fields_ = [
        ("byDigitalAudioChanState", c_byte * MAX_CHANNUM_V30),
        ("byDigitalChanState", c_byte * MAX_CHANNUM_V30),
    ]


class NET_DVR_XML_CONFIG_INPUT(Structure):
    """ISAPI XML request input parameters for NET_DVR_STDXMLConfig."""

    _fields_ = [
        ("dwSize", c_uint),
        ("lpRequestUrl", c_void_p),
        ("dwRequestUrlLen", c_uint),
        ("lpInBuffer", c_void_p),
        ("dwInBufferSize", c_uint),
        ("dwRecvTimeOut", c_uint),
        ("byForceEncrpt", c_byte),
        ("byNumOfMultiPart", c_byte),
        ("byMIMEType", c_byte),
        ("byRes1", c_byte),
        ("dwSendTimeOut", c_uint),
        ("byRes", c_byte * 24),
    ]


class NET_DVR_XML_CONFIG_OUTPUT(Structure):
    """ISAPI XML response output parameters for NET_DVR_STDXMLConfig."""

    _fields_ = [
        ("dwSize", c_uint),
        ("lpOutBuffer", c_void_p),
        ("dwOutBufferSize", c_uint),
        ("dwReturnedXMLSize", c_uint),
        ("lpStatusBuffer", c_void_p),
        ("dwStatusSize", c_uint),
        ("lpDataBuffer", c_void_p),
        ("byNumOfMultiPart", c_byte),
        ("byRes", c_byte * 23),
    ]


class NET_DVR_HDCFG(Structure):
    """HDD configuration."""

    _fields_ = [
        ("dwSize", c_uint),
        ("dwHDCount", c_uint),
        ("struHDInfo", NET_DVR_SINGLE_HD * MAX_DISKNUM_V30),
    ]


class NET_DVR_SMART_ATTR_INFO(Structure):
    """Single SMART attribute."""

    _fields_ = [
        ("byAttrID", c_byte),
        ("byStatusFlags", c_byte),
        ("byAttrValue", c_byte),
        ("byWorst", c_byte),
        ("dwRawValue", c_byte * 6),
        ("byRes", c_byte * 2),
    ]


class NET_DVR_HDD_SMART_INFO(Structure):
    """HDD SMART information (cmd 3262)."""

    _fields_ = [
        ("dwSize", c_uint),
        ("byHDNo", c_byte),
        ("bySelfTestStatus", c_byte),
        ("byRes1", c_byte * 2),
        ("dwAttrCount", c_uint),
        ("struSmartAttrInfo", NET_DVR_SMART_ATTR_INFO * MAX_SMART_ATTR_NUM),
        ("byRes2", c_byte * 64),
    ]


class NET_DVR_DEVICECFG_V40(Structure):
    """Device configuration (V40)."""

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
        ("wDevType", c_uint16),
        ("byDevTypeName", c_byte * DEV_TYPE_NAME_LEN),
        ("bySupport2", c_byte),
        ("byAnalogAlarmInPortNum", c_byte),
        ("byStartAlarmInNo", c_byte),
        ("byStartAlarmOutNo", c_byte),
        ("byStartIPAlarmInNo", c_byte),
        ("byStartIPAlarmOutNo", c_byte),
        ("byHighIPChanNum", c_byte),
        ("byRes2", c_byte * 9),
    ]


class NET_DVR_TIME(Structure):
    """SDK time structure used by FindFile and other APIs."""

    _fields_ = [
        ("dwYear", c_uint),
        ("dwMonth", c_uint),
        ("dwDay", c_uint),
        ("dwHour", c_uint),
        ("dwMinute", c_uint),
        ("dwSecond", c_uint),
    ]

    @classmethod
    def from_datetime(cls, dt: "datetime") -> "NET_DVR_TIME":
        t = cls()
        t.dwYear = dt.year
        t.dwMonth = dt.month
        t.dwDay = dt.day
        t.dwHour = dt.hour
        t.dwMinute = dt.minute
        t.dwSecond = dt.second
        return t

    def to_datetime(self) -> "datetime":
        from datetime import datetime as _dt
        try:
            return _dt(self.dwYear, self.dwMonth, self.dwDay,
                       self.dwHour, self.dwMinute, self.dwSecond)
        except ValueError:
            return _dt.min


class NET_DVR_FILECOND_V40(Structure):
    """File search conditions for NET_DVR_FindFile_V40."""

    _fields_ = [
        ("lChannel", c_long),
        ("dwFileType", c_uint),
        ("dwIsLocked", c_uint),
        ("dwUseCardNo", c_uint),
        ("sCardNumber", c_byte * 32),
        ("struStartTime", NET_DVR_TIME),
        ("struStopTime", NET_DVR_TIME),
        ("byDrawFrame", c_byte),
        ("byFindType", c_byte),
        ("byQuickSearch", c_byte),
        ("bySpecialFindInfoType", c_byte),
        ("dwVolumeNum", c_uint),
        ("byWorkingDeviceGUID", c_byte * 16),
        ("uSpecialFindInfo", c_byte * 68),
        ("byStreamType", c_byte),
        ("byAudioFile", c_byte),
        ("byRes2", c_byte * 30),
    ]


class NET_DVR_FINDDATA_V40(Structure):
    """File search result for NET_DVR_FindNextFile_V40."""

    _fields_ = [
        ("sFileName", c_char * 100),
        ("struStartTime", NET_DVR_TIME),
        ("struStopTime", NET_DVR_TIME),
        ("dwFileSize", c_uint),
        ("sCardNum", c_char * 32),
        ("byLocked", c_byte),
        ("byFileType", c_byte),
        ("byQuickSearch", c_byte),
        ("byRes", c_byte),
        ("dwFileIndex", c_uint),
        ("byStreamType", c_byte),
        ("byRes1", c_byte * 127),
    ]


class NET_DVR_JPEGPARA(Structure):
    """JPEG capture parameters for NET_DVR_CaptureJPEGPicture_NEW.

    wPicSize: 0xff = auto (use current stream resolution)
    wPicQuality: 0 = best, 1 = better, 2 = average
    """

    _fields_ = [
        ("wPicSize", c_ushort),
        ("wPicQuality", c_ushort),
    ]


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------


def _load_sdk_library(lib_path: str) -> ctypes.CDLL:
    """Load HCNetSDK shared library from *lib_path*.

    On Windows uses WinDLL (stdcall calling convention) which is required
    by the Hikvision SDK.  On Linux uses CDLL (cdecl).

    Also registers the SDK directory and HCNetSDKCom subdirectory so that
    dependent DLLs can be found.
    """
    import os
    lib_dir = os.path.dirname(os.path.abspath(lib_path))

    if sys.platform == "win32":
        # Register DLL search directories for dependencies
        os.add_dll_directory(lib_dir)
        hcnetsdkcom_dir = os.path.join(lib_dir, "HCNetSDKCom")
        if os.path.exists(hcnetsdkcom_dir):
            os.add_dll_directory(hcnetsdkcom_dir)

        # Hikvision SDK uses __stdcall on Windows
        return ctypes.WinDLL(lib_path)

    return ctypes.cdll.LoadLibrary(lib_path)


def _bytes_to_str(byte_array: Any) -> str:
    """Convert a ctypes byte array (c_byte[]) to a Python string."""
    raw = bytes(byte_array)
    return raw.split(b"\x00", 1)[0].decode("utf-8", errors="replace")


# ---------------------------------------------------------------------------
# Version parsing helpers
# ---------------------------------------------------------------------------


def _parse_version(dword: int) -> str:
    """Parse a packed DWORD version into 'major.minor.build' string."""
    major = (dword >> 24) & 0xFF
    minor = (dword >> 16) & 0xFF
    build = dword & 0xFFFF
    return f"{major}.{minor}.{build}"


def _parse_build_date(dword: int) -> str:
    """Parse a packed build-date DWORD into 'YYYY-MM-DD' string."""
    year = dword >> 16
    month = (dword >> 8) & 0xFF
    day = dword & 0xFF
    return f"{year:04d}-{month:02d}-{day:02d}"


# ---------------------------------------------------------------------------
# Binding class
# ---------------------------------------------------------------------------


class HCNetSDKBinding:
    """Thin wrapper around HCNetSDK native calls.

    Parameters
    ----------
    lib:
        An already-loaded library object (or mock).  Takes priority over
        *lib_path*.
    lib_path:
        Filesystem path to ``HCNetSDK.dll`` / ``libhcnetsdk.so``.
    """

    def __init__(
        self,
        lib: Any | None = None,
        lib_path: str | None = None,
    ) -> None:
        import threading
        self._lock = threading.Lock()
        self._lib_path = lib_path
        if lib is not None:
            self._lib = lib
        elif lib_path is not None:
            self._lib = _load_sdk_library(lib_path)
        else:
            raise ValueError("Either lib or lib_path must be provided")

        self._set_argtypes()

    def _set_argtypes(self) -> None:
        """Declare C function signatures for type-safe ctypes calls.

        All HCNetSDK functions use LONG (c_long) for user IDs and channel
        numbers, BOOL (c_int) return, and pointer types for structs.
        Without these declarations ctypes may pass 64-bit Python ints
        incorrectly on some platforms.
        """
        lib = self._lib
        try:
            # NET_DVR_CaptureJPEGPicture_NEW(LONG lUserID, LONG lChannel,
            #   LPNET_DVR_JPEGPARA, char*, DWORD, LPDWORD) -> BOOL
            lib.NET_DVR_CaptureJPEGPicture_NEW.argtypes = [
                c_long, c_long, ctypes.POINTER(NET_DVR_JPEGPARA),
                ctypes.c_char_p, c_uint, ctypes.POINTER(c_uint),
            ]
            lib.NET_DVR_CaptureJPEGPicture_NEW.restype = ctypes.c_int

            # NET_DVR_Login_V40(LPNET_DVR_USER_LOGIN_INFO,
            #   LPNET_DVR_DEVICEINFO_V40) -> LONG
            lib.NET_DVR_Login_V40.argtypes = [
                ctypes.POINTER(NET_DVR_USER_LOGIN_INFO),
                ctypes.POINTER(NET_DVR_DEVICEINFO_V40),
            ]
            lib.NET_DVR_Login_V40.restype = c_long

            # Simple functions
            lib.NET_DVR_Init.restype = ctypes.c_int
            lib.NET_DVR_Cleanup.restype = ctypes.c_int
            lib.NET_DVR_GetLastError.restype = c_uint
            lib.NET_DVR_Logout.argtypes = [c_long]
            lib.NET_DVR_Logout.restype = ctypes.c_int
        except AttributeError:
            # Mock/test library may not have all functions
            pass

    # -- lifecycle -----------------------------------------------------------

    def init(self) -> None:
        """Initialise the SDK (``NET_DVR_Init``).

        Sets SDK component path and timeouts.  Must be called once before
        any other SDK calls.
        """
        # Set SDK component path BEFORE Init (required for HCNetSDKCom DLLs)
        if self._lib_path is not None:
            import os
            lib_dir = os.path.dirname(os.path.abspath(self._lib_path))
            sdk_path = NET_DVR_LOCAL_SDK_PATH()
            sdk_path.sPath = lib_dir.encode("utf-8")
            self._lib.NET_DVR_SetSDKInitCfg(
                NET_SDK_INIT_CFG_SDK_PATH, ctypes.byref(sdk_path)
            )

        ok = self._lib.NET_DVR_Init()
        if not ok:
            code = self.get_last_error()
            raise SdkError(
                device_id="sdk",
                error_code=code,
                message="NET_DVR_Init failed",
            )
        # Set timeouts so SDK calls don't hang forever
        self._lib.NET_DVR_SetConnectTime(5000, 3)  # 5 sec, 3 attempts
        self._lib.NET_DVR_SetRecvTimeOut(10000)     # 10 sec receive timeout

    def cleanup(self) -> None:
        """Release SDK resources (``NET_DVR_Cleanup``)."""
        self._lib.NET_DVR_Cleanup()

    def get_last_error(self) -> int:
        """Return the last SDK error code."""
        return int(self._lib.NET_DVR_GetLastError())

    # -- authentication ------------------------------------------------------

    def login(
        self,
        host: str,
        port: int,
        username: str,
        password: str,
    ) -> tuple[int, dict]:
        """Synchronous login via ``NET_DVR_Login_V40``.

        Returns
        -------
        (user_id, device_info_dict)
        """
        login_info = NET_DVR_USER_LOGIN_INFO()
        login_info.sDeviceAddress = host.encode("utf-8")
        login_info.wPort = port
        login_info.sUserName = username.encode("utf-8")
        login_info.sPassword = password.encode("utf-8")
        login_info.bUseAsynLogin = 0  # synchronous

        device_info = NET_DVR_DEVICEINFO_V40()

        user_id = self._lib.NET_DVR_Login_V40(
            ctypes.byref(login_info),
            ctypes.byref(device_info),
        )

        if user_id < 0:
            code = self.get_last_error()
            raise SdkError(
                device_id=f"{host}:{port}",
                error_code=code,
                message="NET_DVR_Login_V40 failed",
            )

        v30 = device_info.struDeviceV30
        # Total IP channels: byIPChanNum is the low byte,
        # byHighDChanNum is the high byte (for >256 channels)
        ip_chan_num = v30.byIPChanNum + (v30.byHighDChanNum << 8)
        info: dict[str, Any] = {
            "serial_number": _bytes_to_str(v30.sSerialNumber),
            "disk_num": v30.byDiskNum,
            "chan_num": v30.byChanNum,
            "ip_chan_num": ip_chan_num,
            "start_chan": v30.byStartChan,
            "start_dchan": v30.byStartDChan,
            "dvr_type": v30.byDVRType,
            "alarm_in_num": v30.byAlarmInPortNum,
            "alarm_out_num": v30.byAlarmOutPortNum,
            "audio_chan_num": v30.byAudioChanNum,
            "zero_chan_num": v30.byZeroChanNum,
            "password_level": device_info.byPasswordLevel,
        }
        return user_id, info

    def logout(self, user_id: int) -> None:
        """Logout from device."""
        self._lib.NET_DVR_Logout(user_id)

    # -- ISAPI tunneling -----------------------------------------------------

    def std_xml_config(self, user_id: int, url: str) -> str:
        """Tunnel an ISAPI GET request through SDK (``NET_DVR_STDXMLConfig``).

        Parameters
        ----------
        user_id:
            Active login handle.
        url:
            ISAPI URL, e.g. ``"GET /ISAPI/ContentMgmt/InputProxy/channels/status"``.

        Returns
        -------
        XML response body as a string.
        """
        url_bytes = url.encode("utf-8") + b"\r\n"
        url_buffer = create_string_buffer(url_bytes)
        out_buffer = create_string_buffer(131072)  # 128 KB
        status_buffer = create_string_buffer(4096)

        input_param = NET_DVR_XML_CONFIG_INPUT()
        input_param.dwSize = ctypes.sizeof(input_param)
        input_param.lpRequestUrl = cast(url_buffer, c_void_p)
        input_param.dwRequestUrlLen = len(url_bytes)
        input_param.lpInBuffer = None
        input_param.dwInBufferSize = 0
        input_param.dwRecvTimeOut = 10000

        output_param = NET_DVR_XML_CONFIG_OUTPUT()
        output_param.dwSize = ctypes.sizeof(output_param)
        output_param.lpOutBuffer = cast(out_buffer, c_void_p)
        output_param.dwOutBufferSize = 131072
        output_param.lpStatusBuffer = cast(status_buffer, c_void_p)
        output_param.dwStatusSize = 4096

        ok = self._lib.NET_DVR_STDXMLConfig(
            user_id,
            ctypes.byref(input_param),
            ctypes.byref(output_param),
        )
        if not ok:
            code = self.get_last_error()
            raise SdkError(
                device_id=str(user_id),
                error_code=code,
                message=f"NET_DVR_STDXMLConfig failed for {url}",
            )

        return out_buffer.value.decode("utf-8", errors="ignore")

    # -- configuration queries -----------------------------------------------

    def get_device_config(self, user_id: int) -> dict:
        """Fetch device configuration (``NET_DVR_GET_DEVICECFG_V40``).

        Returns a dict with name, serial, firmware versions, channel counts,
        etc.
        """
        cfg = NET_DVR_DEVICECFG_V40()
        cfg.dwSize = ctypes.sizeof(cfg)
        bytes_returned = c_uint(0)

        ok = self._lib.NET_DVR_GetDVRConfig(
            user_id,
            NET_DVR_GET_DEVICECFG_V40,
            0,
            ctypes.byref(cfg),
            ctypes.sizeof(cfg),
            ctypes.byref(bytes_returned),
        )
        if not ok:
            code = self.get_last_error()
            raise SdkError(
                device_id=str(user_id),
                error_code=code,
                message="NET_DVR_GetDVRConfig DEVICECFG_V40 failed",
            )

        return {
            "device_name": _bytes_to_str(cfg.sDVRName),
            "serial_number": _bytes_to_str(cfg.sSerialNumber),
            "firmware_version": _parse_version(cfg.dwSoftwareVersion),
            "software_build_date": _parse_build_date(cfg.dwSoftwareBuildDate),
            "dsp_version": _parse_version(cfg.dwDSPSoftwareVersion),
            "dsp_build_date": _parse_build_date(cfg.dwDSPSoftwareBuildDate),
            "panel_version": cfg.dwPanelVersion,
            "hardware_version": cfg.dwHardwareVersion,
            "disk_num": cfg.byDiskNum,
            "chan_num": cfg.byChanNum,
            "ip_chan_num": cfg.byIPChanNum,
            "start_chan": cfg.byStartChan,
            "dvr_type": cfg.byDVRType,
            "device_type_name": _bytes_to_str(cfg.byDevTypeName),
        }

    def get_digital_channel_state(
        self, user_id: int, start_chan: int, total_chans: int
    ) -> list[dict]:
        """Fetch digital channel online/offline state (cmd 6126).

        Returns a list of dicts with channel_id and online status.
        """
        state = NET_DVR_DIGITAL_CHANNEL_STATE()
        bytes_returned = c_uint(0)

        ok = self._lib.NET_DVR_GetDVRConfig(
            user_id,
            NET_DVR_GET_DIGITAL_CHANNEL_STATE,
            0,
            ctypes.byref(state),
            ctypes.sizeof(state),
            ctypes.byref(bytes_returned),
        )
        if not ok:
            code = self.get_last_error()
            raise SdkError(
                device_id=str(user_id),
                error_code=code,
                message="NET_DVR_GetDVRConfig DIGITAL_CHANNEL_STATE failed",
            )

        channels: list[dict] = []
        for i in range(min(total_chans, MAX_CHANNUM_V30)):
            raw = state.byDigitalChanState[i]
            # 0 = offline, 1 = online, 0xFF (-1 signed) = invalid/not configured
            if raw == -1 or raw == 0xFF:
                online = None  # channel slot not used
            else:
                online = raw == 1
            channels.append({
                "channel_id": str(start_chan + i),
                "online": online,
            })
        return channels

    def get_hdd_config(self, user_id: int) -> list[dict]:
        """Fetch HDD configuration (``NET_DVR_GET_HDCFG``).

        Returns a list of dicts, one per disk.
        """
        hd_cfg = NET_DVR_HDCFG()
        hd_cfg.dwSize = ctypes.sizeof(hd_cfg)
        bytes_returned = c_uint(0)

        ok = self._lib.NET_DVR_GetDVRConfig(
            user_id,
            NET_DVR_GET_HDCFG,
            0,
            ctypes.byref(hd_cfg),
            ctypes.sizeof(hd_cfg),
            ctypes.byref(bytes_returned),
        )
        if not ok:
            code = self.get_last_error()
            raise SdkError(
                device_id=str(user_id),
                error_code=code,
                message="NET_DVR_GetDVRConfig HDCFG failed",
            )

        disks: list[dict] = []
        for i in range(hd_cfg.dwHDCount):
            hd = hd_cfg.struHDInfo[i]
            status_code = hd.dwHdStatus
            disks.append(
                {
                    "disk_id": str(hd.dwHDNo),
                    "capacity_mb": hd.dwCapacity,
                    "free_space_mb": hd.dwFreeSpace,
                    "status_code": status_code,
                    "status_name": DISK_STATUS_NAMES.get(status_code, f"unknown({status_code})"),
                    "hd_attr": hd.byHDAttr,
                    "hd_type": hd.byHDType,
                    "recycling": bool(hd.byRecycling),
                    "storage_type": hd.dwStorageType,
                }
            )
        return disks

    def get_hdd_smart_info(self, user_id: int, disk_no: int) -> dict | None:
        """Fetch HDD SMART info via binary SDK command 3262.

        Parameters
        ----------
        disk_no:
            1-based disk number (lChannel parameter).

        Returns dict with smart_status, temperature, power_on_hours or None on failure.
        """
        smart_info = NET_DVR_HDD_SMART_INFO()
        smart_info.dwSize = ctypes.sizeof(smart_info)
        bytes_returned = c_uint(0)

        ok = self._lib.NET_DVR_GetDVRConfig(
            user_id,
            NET_DVR_GET_HDD_SMART_INFO,
            disk_no,  # lChannel = disk number (1-based)
            ctypes.byref(smart_info),
            ctypes.sizeof(smart_info),
            ctypes.byref(bytes_returned),
        )
        if not ok:
            return None

        result: dict = {
            "smart_status": "ok",
            "temperature": None,
            "power_on_hours": None,
        }

        for i in range(min(smart_info.dwAttrCount, MAX_SMART_ATTR_NUM)):
            attr = smart_info.struSmartAttrInfo[i]
            attr_id = attr.byAttrID & 0xFF
            # Raw value is 6 bytes, little-endian
            raw_bytes = bytes(attr.dwRawValue)
            raw_val = int.from_bytes(raw_bytes[:4], byteorder="little")

            if attr_id == 9:  # Power-On Hours
                result["power_on_hours"] = raw_val
            elif attr_id == 194:  # Temperature
                result["temperature"] = raw_val & 0xFF
            elif attr_id == 5:  # Reallocated Sectors
                if raw_val > 100:
                    result["smart_status"] = "error"
                elif raw_val > 0 and result["smart_status"] == "ok":
                    result["smart_status"] = "warning"

        # Check self-test status
        if smart_info.bySelfTestStatus in (3, 8):  # SMART_FAILED / ABNORMAL
            result["smart_status"] = "error"

        return result

    def find_recordings(
        self, user_id: int, channel: int,
        start_time: "datetime", end_time: "datetime",
    ) -> dict:
        """Search for recording files on a channel in a time window.

        Uses NET_DVR_FindFile_V40 (safe binary SDK, no ISAPI tunnel).
        Returns dict with has_recordings (bool) and files_count (int).
        """
        from datetime import datetime as _dt  # noqa: F811

        cond = NET_DVR_FILECOND_V40()
        cond.lChannel = channel
        cond.dwFileType = 0xFF  # all types
        cond.dwIsLocked = 0xFF  # all lock states
        cond.dwUseCardNo = 0
        cond.struStartTime = NET_DVR_TIME.from_datetime(start_time)
        cond.struStopTime = NET_DVR_TIME.from_datetime(end_time)

        find_handle = self._lib.NET_DVR_FindFile_V40(
            user_id, ctypes.byref(cond),
        )
        if find_handle < 0:
            return {"has_recordings": False, "files_count": 0}

        files_count = 0
        find_data = NET_DVR_FINDDATA_V40()

        while True:
            result = self._lib.NET_DVR_FindNextFile_V40(
                find_handle, ctypes.byref(find_data),
            )
            if result == NET_DVR_FILE_SUCCESS:
                files_count += 1
                # We only need to know if there are any recordings,
                # no need to count all files — break after first.
                break
            elif result == NET_DVR_ISFINDING:
                continue
            else:  # NOMOREFILE or EXCEPTION
                break

        self._lib.NET_DVR_FindClose_V30(find_handle)

        return {"has_recordings": files_count > 0, "files_count": files_count}

    def capture_jpeg(self, user_id: int, channel: int) -> bytes:
        """Capture JPEG snapshot via ``NET_DVR_CaptureJPEGPicture_NEW``.

        Parameters
        ----------
        channel:
            1-based channel number (lChannel).

        Returns raw JPEG bytes.
        """
        jpeg_para = NET_DVR_JPEGPARA()
        jpeg_para.wPicSize = 0xFF   # auto resolution
        jpeg_para.wPicQuality = 0   # best quality

        buffer_size = 2 * 1024 * 1024  # 2 MB
        pic_buffer = create_string_buffer(buffer_size)
        size_returned = c_uint(0)

        ok = self._lib.NET_DVR_CaptureJPEGPicture_NEW(
            user_id,
            channel,
            ctypes.byref(jpeg_para),
            pic_buffer,
            buffer_size,
            ctypes.byref(size_returned),
        )
        if not ok:
            code = self.get_last_error()
            raise SdkError(
                device_id=str(user_id),
                error_code=code,
                message=f"NET_DVR_CaptureJPEGPicture_NEW failed (ch={channel})",
            )

        actual_size = size_returned.value
        if actual_size <= 0:
            raise SdkError(
                device_id=str(user_id),
                error_code=0,
                message=f"Snapshot returned empty buffer (ch={channel})",
            )

        data = pic_buffer.raw[:actual_size]
        if len(data) < 2 or data[:2] != b"\xff\xd8":
            raise SdkError(
                device_id=str(user_id),
                error_code=0,
                message=f"Snapshot data is not valid JPEG (ch={channel})",
            )
        return data
