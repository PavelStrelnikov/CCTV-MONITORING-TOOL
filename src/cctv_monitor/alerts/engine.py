from datetime import datetime, timezone
from cctv_monitor.models.device_health import DeviceHealthSummary
from cctv_monitor.models.alert import AlertEvent
from cctv_monitor.core.types import AlertStatus
from cctv_monitor.alerts.rules import ALL_RULES


class AlertEngine:
    def evaluate(
        self, health: DeviceHealthSummary, active_alerts: list[AlertEvent],
    ) -> tuple[list[AlertEvent], list[AlertEvent]]:
        now = datetime.now(timezone.utc)
        active_types = {a.alert_type for a in active_alerts}
        triggered_types = set()
        new_alerts: list[AlertEvent] = []

        for rule in ALL_RULES:
            result = rule(health)
            if result is not None:
                alert_type, severity, message = result
                triggered_types.add(alert_type)
                if alert_type not in active_types:
                    new_alerts.append(AlertEvent(
                        device_id=health.device_id,
                        alert_type=alert_type,
                        severity=severity,
                        message=message,
                        source="polling",
                        status=AlertStatus.ACTIVE,
                        created_at=now,
                    ))

        resolved = [a for a in active_alerts if a.alert_type not in triggered_types]
        return new_alerts, resolved
