"""Tests for HCNetSDK ctypes bindings."""

import ctypes
from unittest.mock import MagicMock

import pytest

from cctv_monitor.drivers.hikvision.errors import SdkError
from cctv_monitor.drivers.hikvision.transports.sdk_bindings import (
    HCNetSDKBinding,
    NET_DVR_DEVICEINFO_V30,
    NET_DVR_DEVICEINFO_V40,
    NET_DVR_USER_LOGIN_INFO,
)


# ---------------------------------------------------------------------------
# Structure layout tests
# ---------------------------------------------------------------------------


def test_deviceinfo_v30_struct_size():
    """NET_DVR_DEVICEINFO_V30 has expected key fields."""
    info = NET_DVR_DEVICEINFO_V30()
    assert hasattr(info, "sSerialNumber")
    assert len(info.sSerialNumber) == 48
    assert hasattr(info, "byDiskNum")
    assert hasattr(info, "byChanNum")
    assert hasattr(info, "byIPChanNum")
    assert hasattr(info, "byStartChan")


def test_deviceinfo_v40_struct_contains_v30():
    """NET_DVR_DEVICEINFO_V40 wraps a V30 struct."""
    info = NET_DVR_DEVICEINFO_V40()
    assert hasattr(info, "struDeviceV30")
    assert isinstance(info.struDeviceV30, NET_DVR_DEVICEINFO_V30)


def test_login_info_struct_fields():
    """NET_DVR_USER_LOGIN_INFO has address, port, credentials, async flag."""
    login = NET_DVR_USER_LOGIN_INFO()
    assert hasattr(login, "sDeviceAddress")
    assert hasattr(login, "wPort")
    assert hasattr(login, "sUserName")
    assert hasattr(login, "sPassword")
    assert hasattr(login, "bUseAsynLogin")


# ---------------------------------------------------------------------------
# Binding method tests (with mock SDK library)
# ---------------------------------------------------------------------------


@pytest.fixture
def mock_lib():
    """Return a MagicMock standing in for the native SDK library."""
    lib = MagicMock()
    lib.NET_DVR_Init.return_value = True
    lib.NET_DVR_Cleanup.return_value = True
    lib.NET_DVR_GetLastError.return_value = 0
    lib.NET_DVR_Logout.return_value = True
    return lib


@pytest.fixture
def binding(mock_lib):
    return HCNetSDKBinding(lib=mock_lib)


def test_binding_init_calls_sdk_init(binding, mock_lib):
    """init() delegates to NET_DVR_Init."""
    binding.init()
    mock_lib.NET_DVR_Init.assert_called_once()


def test_binding_init_raises_on_failure(mock_lib):
    """init() raises SdkError when NET_DVR_Init returns False."""
    mock_lib.NET_DVR_Init.return_value = False
    mock_lib.NET_DVR_GetLastError.return_value = 3
    binding = HCNetSDKBinding(lib=mock_lib)
    with pytest.raises(SdkError):
        binding.init()


def test_binding_cleanup_calls_sdk_cleanup(binding, mock_lib):
    """cleanup() delegates to NET_DVR_Cleanup."""
    binding.cleanup()
    mock_lib.NET_DVR_Cleanup.assert_called_once()


def test_binding_login_success(binding, mock_lib):
    """login() returns (user_id, device_info_dict) on success."""
    mock_lib.NET_DVR_Login_V40.return_value = 1

    user_id, info = binding.login("192.168.1.64", 8000, "admin", "password")

    assert user_id == 1
    assert isinstance(info, dict)
    mock_lib.NET_DVR_Login_V40.assert_called_once()


def test_binding_login_failure_raises(binding, mock_lib):
    """login() raises SdkError when NET_DVR_Login_V40 returns -1."""
    mock_lib.NET_DVR_Login_V40.return_value = -1
    mock_lib.NET_DVR_GetLastError.return_value = 1

    with pytest.raises(SdkError):
        binding.login("192.168.1.64", 8000, "admin", "wrong")


def test_binding_logout(binding, mock_lib):
    """logout() calls NET_DVR_Logout with user_id."""
    binding.logout(1)
    mock_lib.NET_DVR_Logout.assert_called_once_with(1)
