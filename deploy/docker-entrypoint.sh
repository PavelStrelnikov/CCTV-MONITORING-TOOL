#!/bin/bash
set -e

echo "Running database migrations..."
alembic upgrade head

echo "Starting CCTV Monitor backend..."
exec python -m cctv_monitor.main
