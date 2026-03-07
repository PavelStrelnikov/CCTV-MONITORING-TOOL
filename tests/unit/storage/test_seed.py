from cctv_monitor.storage.seed import parse_seed_file


def test_parse_seed_file(tmp_path):
    seed_content = """
polling_policies:
  - name: standard
    device_info_interval: 300
    camera_status_interval: 120
    disk_status_interval: 600
    snapshot_interval: 900

devices:
  - device_id: test-01
    name: Test Device
    vendor: hikvision
    host: "10.0.0.1"
    port: 80
    username: admin
    password: "test123"
    transport_mode: isapi
    polling_policy_id: standard
"""
    seed_file = tmp_path / "seed.yaml"
    seed_file.write_text(seed_content)
    result = parse_seed_file(str(seed_file))
    assert len(result["policies"]) == 1
    assert result["policies"][0]["name"] == "standard"
    assert len(result["devices"]) == 1
    assert result["devices"][0]["device_id"] == "test-01"


def test_parse_seed_file_empty_sections(tmp_path):
    seed_file = tmp_path / "empty.yaml"
    seed_file.write_text("---\n")
    result = parse_seed_file(str(seed_file))
    assert result["policies"] == []
    assert result["devices"] == []


def test_parse_seed_file_multiple_devices(tmp_path):
    seed_content = """
polling_policies: []
devices:
  - device_id: dev-01
    name: Device 1
    vendor: hikvision
    host: "10.0.0.1"
    port: 80
    username: admin
    password: pass1
    transport_mode: isapi
    polling_policy_id: standard
  - device_id: dev-02
    name: Device 2
    vendor: dahua
    host: "10.0.0.2"
    port: 80
    username: admin
    password: pass2
    transport_mode: isapi
    polling_policy_id: light
"""
    seed_file = tmp_path / "multi.yaml"
    seed_file.write_text(seed_content)
    result = parse_seed_file(str(seed_file))
    assert len(result["devices"]) == 2
    assert result["devices"][1]["vendor"] == "dahua"
