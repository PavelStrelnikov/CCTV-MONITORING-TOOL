#!/usr/bin/env python3
"""
Import devices from the hardcoded list (parsed from Excel screenshot).
Requires a running backend at http://localhost:8000.

Usage:
    python scripts/import_devices.py [--dry-run]
"""
import argparse
import sys

import httpx

API_BASE = "http://localhost:8001/api"

# Devices parsed from the Excel table screenshot.
# Fields: (customer_id, name, ip, username, password, web_port, sdk_port)
DEVICES = [
    ("62865158-1", "אלונה חדשניות 16 (1)", "213.57.74.71", "admin", "Dg@niyot16", 8080, 8008),
    ("62865158-2", "אלונה חדשניות 16 (2)", "213.57.74.71", "admin", "Dg@niyot16", 8081, 8009),
    ("61964393-1", "שבזי 19 יבנה (1)", "213.57.78.27", "admin", "Shavazi#19!", 8080, 8008),
    ("61964393-2", "שבזי 19 יבנה (2)", "213.57.78.27", "admin", "Shavazi#19!", 8081, 8009),
    ("62612766", "קיבוץ גלויות 53 אבן יהודה", "213.57.74.74", "admin", "KibutzGal#53!", 8080, 8008),
    ("62454174", "רמות (לבן) צמרות יבניאל 47", "5.29.133.222", "admin", "Ramot2020", 8080, 8008),
    ("62454060", "בית שהר (צימר) צמרות יבניאל", "5.29.165.102", "admin", "BaitShahar2024", 8080, 8008),
    ("62552976", "בית ספר צמרות יבניאל 44", "213.57.74.56", "admin", "Sch@@lYav", 8080, 8008),
    ("63557915", "בית שחרית יבניאל", "5.29.130.223", "admin", "Shahrit@2024", 8080, 8008),
    ("62723466", "מרדכי ביטרמן 2 א פ\"י", "213.57.74.164", "admin", "Biterman@a", 8080, 8008),
    ("62833042", "רמות 10 תל אביב סיבינס", "5.29.134.46", "ramot", "R@mot2025", 8080, 8008),
    ("63589585", "רמיהירו 18 חרוזין", "5.29.137.252", "admin", "Ramot@2025", 8080, 8008),
    ("62765048", "אסד 79 באר שבע (הדרוף 16)", "213.57.74.73", "admin", "BeerSheva@7", 8080, 8008),
    ("61955768", "ברנר 17 טבריה דיירי רחוב", "213.57.74.62", "admin", "Brener#17!", 8443, 8008),
    ("61957768", "בר כוכבא 58 טבריה", "213.57.74.63", "admin", "BarKohva#58!", 8080, 8008),
    ("62211497", "חנביאים 8 טבריה דיירי רחוב", "213.57.74.80", "admin", "Haneviim#8!", 8080, 8008),
    ("63589897", "עיון בהלול 20 טבריה", "5.29.134.67", "admin", "Bahalul#20!", 8080, 8008),
    ("63682535", "גנון הרצליה 33 טבריה", "5.29.129.30", "admin", "Hertzlia#33!", 8080, 8008),
    ("09-7424681-A", "BigAlex N13 אחד השרון (A)", "149.106.149.78", "admin", "BigAlex@A", 8080, 8009),
    ("09-7424681-B", "BigAlex 213 (B)", "149.106.149.78", "admin", "BigAlex@B", 8080, 8008),
]


def import_devices(dry_run: bool = False) -> None:
    created = 0
    skipped = 0
    errors = 0

    client = httpx.Client(base_url=API_BASE, timeout=10)

    for device_id, name, host, username, password, web_port, sdk_port in DEVICES:
        payload = {
            "device_id": device_id,
            "name": name,
            "vendor": "hikvision",
            "host": host,
            "web_port": web_port,
            "sdk_port": sdk_port,
            "username": username,
            "password": password,
            "transport_mode": "auto",
        }

        if dry_run:
            print(f"  [DRY-RUN] {device_id:20s}  {name}")
            created += 1
            continue

        try:
            resp = client.post("/devices", json=payload)
            if resp.status_code == 201:
                print(f"  [CREATED] {device_id:20s}  {name}")
                created += 1
            elif resp.status_code == 409 or (resp.status_code == 400 and "already exists" in resp.text.lower()):
                print(f"  [EXISTS]  {device_id:20s}  {name}")
                skipped += 1
            else:
                print(f"  [ERROR]   {device_id:20s}  {name}  -> {resp.status_code}: {resp.text[:200]}")
                errors += 1
        except httpx.HTTPError as e:
            print(f"  [ERROR]   {device_id:20s}  {name}  -> {e}")
            errors += 1

    print(f"\nDone: {created} created, {skipped} skipped, {errors} errors")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Import devices into CCTV Monitor")
    parser.add_argument("--dry-run", action="store_true", help="Print devices without creating them")
    args = parser.parse_args()

    print(f"Importing {len(DEVICES)} devices into {API_BASE}...")
    if args.dry_run:
        print("(DRY RUN — no changes will be made)\n")
    else:
        print()

    import_devices(dry_run=args.dry_run)
