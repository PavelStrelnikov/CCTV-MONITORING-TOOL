from cctv_monitor.core.errors import CCTVMonitorError, ErrorCode


class HikvisionError(CCTVMonitorError):
    """Base Hikvision-specific error."""


class IsapiError(HikvisionError):
    """ISAPI request failed."""

    def __init__(self, device_id: str, status_code: int, message: str = "") -> None:
        self.status_code = status_code
        super().__init__(
            message=f"ISAPI {status_code}: {message}",
            error_code=ErrorCode.DEVICE_PROTOCOL_ERROR,
            device_id=device_id,
        )


class IsapiAuthError(HikvisionError):
    """ISAPI authentication failed."""

    def __init__(self, device_id: str) -> None:
        super().__init__(
            message="ISAPI authentication failed",
            error_code=ErrorCode.DEVICE_AUTH_FAILED,
            device_id=device_id,
        )


class SdkError(HikvisionError):
    """SDK call failed."""

    def __init__(self, device_id: str, error_code: int, message: str = "") -> None:
        self.sdk_error_code = error_code
        super().__init__(
            message=f"SDK error {error_code}: {message}",
            error_code=ErrorCode.DEVICE_PROTOCOL_ERROR,
            device_id=device_id,
        )
