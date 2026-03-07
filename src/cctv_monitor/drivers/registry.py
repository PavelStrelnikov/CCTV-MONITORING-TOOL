from cctv_monitor.core.types import DeviceVendor


class DriverRegistry:
    def __init__(self) -> None:
        self._drivers: dict[DeviceVendor, type] = {}

    def register(self, vendor: DeviceVendor, driver_cls: type) -> None:
        self._drivers[vendor] = driver_cls

    def get(self, vendor: DeviceVendor) -> type:
        if vendor not in self._drivers:
            raise KeyError(f"No driver registered for vendor: {vendor}")
        return self._drivers[vendor]

    @property
    def vendors(self) -> list[DeviceVendor]:
        return list(self._drivers.keys())
