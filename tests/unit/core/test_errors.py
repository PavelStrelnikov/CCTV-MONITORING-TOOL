import pytest
from cctv_monitor.core.errors import (
    CCTVMonitorError,
    DeviceConnectionError,
    DeviceAuthenticationError,
    DeviceTimeoutError,
    DeviceProtocolError,
    DeviceUnsupportedFeatureError,
    PollingFailedError,
    DeviceUnreachableError,
    ErrorCode,
)


def test_base_error_with_message():
    err = CCTVMonitorError(message="Something went wrong")
    assert str(err) == "Something went wrong"
    assert err.message == "Something went wrong"
    assert err.error_code is None
    assert err.device_id is None


def test_base_error_with_all_fields():
    err = CCTVMonitorError(
        message="Detailed error",
        error_code=ErrorCode.DEVICE_CONNECTION_FAILED,
        device_id="nvr-01",
    )
    assert err.message == "Detailed error"
    assert err.error_code == ErrorCode.DEVICE_CONNECTION_FAILED
    assert err.device_id == "nvr-01"
    assert "nvr-01" in str(err)


def test_device_connection_error():
    err = DeviceConnectionError(message="Connection refused", device_id="nvr-01")
    assert err.device_id == "nvr-01"
    assert err.error_code == ErrorCode.DEVICE_CONNECTION_FAILED
    assert isinstance(err, CCTVMonitorError)
    assert "Connection refused" in str(err)


def test_device_auth_error():
    err = DeviceAuthenticationError(message="Invalid credentials", device_id="nvr-01")
    assert err.error_code == ErrorCode.DEVICE_AUTH_FAILED
    assert isinstance(err, CCTVMonitorError)


def test_device_timeout_error():
    err = DeviceTimeoutError(message="Timeout after 5000ms", device_id="nvr-01")
    assert err.error_code == ErrorCode.DEVICE_TIMEOUT
    assert isinstance(err, CCTVMonitorError)


def test_device_protocol_error():
    err = DeviceProtocolError(message="Unexpected XML response", device_id="nvr-01")
    assert err.error_code == ErrorCode.DEVICE_PROTOCOL_ERROR
    assert isinstance(err, CCTVMonitorError)


def test_device_unsupported_feature_error():
    err = DeviceUnsupportedFeatureError(
        message="Snapshot not supported", device_id="nvr-01"
    )
    assert err.error_code == ErrorCode.DEVICE_UNSUPPORTED_FEATURE
    assert isinstance(err, CCTVMonitorError)


def test_polling_failed_error():
    err = PollingFailedError(message="Polling cycle failed", device_id="nvr-01")
    assert isinstance(err, CCTVMonitorError)


def test_device_unreachable_error():
    err = DeviceUnreachableError(message="Device not responding", device_id="nvr-01")
    assert isinstance(err, CCTVMonitorError)


def test_error_code_enum():
    assert ErrorCode.DEVICE_CONNECTION_FAILED == "DEVICE_CONNECTION_FAILED"
    assert ErrorCode.DEVICE_AUTH_FAILED == "DEVICE_AUTH_FAILED"
    assert ErrorCode.DEVICE_TIMEOUT == "DEVICE_TIMEOUT"
    assert ErrorCode.DEVICE_PROTOCOL_ERROR == "DEVICE_PROTOCOL_ERROR"
    assert ErrorCode.DEVICE_UNSUPPORTED_FEATURE == "DEVICE_UNSUPPORTED_FEATURE"


def test_error_serialization():
    err = DeviceConnectionError(message="Connection refused", device_id="nvr-01")
    # Errors should be easily serializable to dict for logging/alerts
    assert hasattr(err, "to_dict")
    d = err.to_dict()
    assert d["message"] == "Connection refused"
    assert d["error_code"] == "DEVICE_CONNECTION_FAILED"
    assert d["device_id"] == "nvr-01"
    assert d["error_type"] == "DeviceConnectionError"
