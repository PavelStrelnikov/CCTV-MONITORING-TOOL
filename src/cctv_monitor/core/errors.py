from enum import StrEnum


class ErrorCode(StrEnum):
    DEVICE_CONNECTION_FAILED = "DEVICE_CONNECTION_FAILED"
    DEVICE_AUTH_FAILED = "DEVICE_AUTH_FAILED"
    DEVICE_TIMEOUT = "DEVICE_TIMEOUT"
    DEVICE_PROTOCOL_ERROR = "DEVICE_PROTOCOL_ERROR"
    DEVICE_UNSUPPORTED_FEATURE = "DEVICE_UNSUPPORTED_FEATURE"
    POLLING_FAILED = "POLLING_FAILED"
    DEVICE_UNREACHABLE = "DEVICE_UNREACHABLE"


class CCTVMonitorError(Exception):
    """Base error for all CCTV Monitor errors."""

    def __init__(
        self,
        message: str = "",
        error_code: ErrorCode | None = None,
        device_id: str | None = None,
    ) -> None:
        self.message = message
        self.error_code = error_code
        self.device_id = device_id
        if device_id:
            super().__init__(f"[{device_id}] {message}")
        else:
            super().__init__(message)

    def to_dict(self) -> dict:
        return {
            "error_type": type(self).__name__,
            "message": self.message,
            "error_code": self.error_code.value if self.error_code else None,
            "device_id": self.device_id,
        }


class DeviceConnectionError(CCTVMonitorError):
    """Device is unreachable."""

    def __init__(self, message: str = "", device_id: str | None = None) -> None:
        super().__init__(message, ErrorCode.DEVICE_CONNECTION_FAILED, device_id)


class DeviceAuthenticationError(CCTVMonitorError):
    """Authentication failed."""

    def __init__(self, message: str = "", device_id: str | None = None) -> None:
        super().__init__(message, ErrorCode.DEVICE_AUTH_FAILED, device_id)


class DeviceTimeoutError(CCTVMonitorError):
    """Device did not respond in time."""

    def __init__(self, message: str = "", device_id: str | None = None) -> None:
        super().__init__(message, ErrorCode.DEVICE_TIMEOUT, device_id)


class DeviceProtocolError(CCTVMonitorError):
    """Unexpected protocol response."""

    def __init__(self, message: str = "", device_id: str | None = None) -> None:
        super().__init__(message, ErrorCode.DEVICE_PROTOCOL_ERROR, device_id)


class DeviceUnsupportedFeatureError(CCTVMonitorError):
    """Device does not support requested feature."""

    def __init__(self, message: str = "", device_id: str | None = None) -> None:
        super().__init__(message, ErrorCode.DEVICE_UNSUPPORTED_FEATURE, device_id)


class PollingFailedError(CCTVMonitorError):
    """Polling cycle failed."""

    def __init__(self, message: str = "", device_id: str | None = None) -> None:
        super().__init__(message, ErrorCode.POLLING_FAILED, device_id)


class DeviceUnreachableError(CCTVMonitorError):
    """Device is not responding."""

    def __init__(self, message: str = "", device_id: str | None = None) -> None:
        super().__init__(message, ErrorCode.DEVICE_UNREACHABLE, device_id)
