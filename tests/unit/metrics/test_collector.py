from cctv_monitor.metrics.collector import MetricsCollector


def test_record_and_get_summary():
    collector = MetricsCollector()
    collector.record_poll_result("nvr-01", "device_info", success=True)
    collector.record_poll_result("nvr-01", "device_info", success=True)
    collector.record_poll_result("nvr-01", "device_info", success=False)
    summary = collector.get_summary()
    assert summary["total_polls"] == 3
    assert summary["successful_polls"] == 2
    assert summary["failed_polls"] == 1


def test_record_response_time():
    collector = MetricsCollector()
    collector.record_device_response_time("nvr-01", 150.0)
    collector.record_device_response_time("nvr-01", 250.0)
    summary = collector.get_summary()
    assert summary["devices"]["nvr-01"]["avg_response_ms"] == 200.0


def test_empty_summary():
    collector = MetricsCollector()
    summary = collector.get_summary()
    assert summary["total_polls"] == 0
    assert summary["successful_polls"] == 0
    assert summary["failed_polls"] == 0
    assert summary["devices"] == {}
