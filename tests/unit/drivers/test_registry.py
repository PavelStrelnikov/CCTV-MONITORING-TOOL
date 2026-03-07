import pytest
from cctv_monitor.drivers.registry import DriverRegistry
from cctv_monitor.core.types import DeviceVendor


class FakeDriver:
    """Fake driver implementing DeviceDriver protocol."""

    async def connect(self, config):
        pass

    async def disconnect(self):
        pass

    async def get_device_info(self):
        pass

    async def get_camera_statuses(self):
        return []

    async def get_disk_statuses(self):
        return []

    async def get_recording_statuses(self):
        return []

    async def get_snapshot(self, channel_id):
        pass

    async def check_health(self):
        pass

    async def detect_capabilities(self):
        pass


def test_register_and_get_driver():
    registry = DriverRegistry()
    registry.register(DeviceVendor.HIKVISION, FakeDriver)
    driver_cls = registry.get(DeviceVendor.HIKVISION)
    assert driver_cls is FakeDriver


def test_get_unknown_vendor_raises():
    registry = DriverRegistry()
    with pytest.raises(KeyError, match="dahua"):
        registry.get(DeviceVendor.DAHUA)


def test_list_registered_vendors():
    registry = DriverRegistry()
    registry.register(DeviceVendor.HIKVISION, FakeDriver)
    assert DeviceVendor.HIKVISION in registry.vendors


def test_register_multiple_vendors():
    registry = DriverRegistry()
    registry.register(DeviceVendor.HIKVISION, FakeDriver)
    registry.register(DeviceVendor.DAHUA, FakeDriver)
    assert len(registry.vendors) == 2
