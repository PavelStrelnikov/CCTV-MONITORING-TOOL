from pathlib import Path
import yaml


def parse_seed_file(path: str) -> dict:
    content = Path(path).read_text(encoding="utf-8")
    data = yaml.safe_load(content)
    if data is None:
        data = {}
    return {
        "policies": data.get("polling_policies", []),
        "devices": data.get("devices", []),
    }
