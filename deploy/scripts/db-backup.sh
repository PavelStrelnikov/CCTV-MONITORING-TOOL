#!/bin/bash
# =============================================================================
# CCTV Monitoring Tool — Database Backup & Restore
# =============================================================================
# Usage:
#   ./db-backup.sh backup                    Create a backup
#   ./db-backup.sh restore backups/file.dump Restore from a backup file
#
# Run from the deploy/ directory (where docker-compose.prod.yml is located).
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
DEPLOY_DIR="$(dirname "$SCRIPT_DIR")"
COMPOSE_FILE="$DEPLOY_DIR/docker-compose.prod.yml"
BACKUP_DIR="$DEPLOY_DIR/backups"

# Load .env if it exists
if [ -f "$DEPLOY_DIR/.env" ]; then
    set -a
    source "$DEPLOY_DIR/.env"
    set +a
fi

DB_USER="${POSTGRES_USER:-cctv_admin}"
DB_NAME="${POSTGRES_DB:-cctv_monitoring}"

usage() {
    echo "Usage: $0 {backup|restore} [filename]"
    echo ""
    echo "Commands:"
    echo "  backup              Create database backup to backups/ directory"
    echo "  restore <file>      Restore database from backup file"
    echo ""
    echo "Examples:"
    echo "  $0 backup"
    echo "  $0 restore backups/cctv_20260321_120000.dump"
    exit 1
}

do_backup() {
    mkdir -p "$BACKUP_DIR"
    local TIMESTAMP
    TIMESTAMP=$(date +%Y%m%d_%H%M%S)
    local FILENAME="cctv_${TIMESTAMP}.dump"
    local FILEPATH="$BACKUP_DIR/$FILENAME"

    echo "Creating backup: $FILEPATH"
    docker compose -f "$COMPOSE_FILE" exec -T postgres \
        pg_dump -U "$DB_USER" -d "$DB_NAME" --format=custom \
        > "$FILEPATH"

    local SIZE
    SIZE=$(du -h "$FILEPATH" | cut -f1)
    echo "Backup complete: $FILEPATH ($SIZE)"
}

do_restore() {
    local FILEPATH="$1"

    if [ ! -f "$FILEPATH" ]; then
        echo "Error: File not found: $FILEPATH"
        exit 1
    fi

    echo "WARNING: This will overwrite the current database!"
    echo "File: $FILEPATH"
    read -p "Continue? [y/N] " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Aborted."
        exit 0
    fi

    echo "Restoring from: $FILEPATH"
    docker compose -f "$COMPOSE_FILE" exec -T postgres \
        pg_restore --clean --if-exists -U "$DB_USER" -d "$DB_NAME" \
        < "$FILEPATH"

    echo "Restore complete. Restart backend to apply migrations:"
    echo "  docker compose -f $COMPOSE_FILE restart backend"
}

# --- Main ---
if [ $# -lt 1 ]; then
    usage
fi

case "$1" in
    backup)
        do_backup
        ;;
    restore)
        if [ $# -lt 2 ]; then
            echo "Error: restore requires a filename argument"
            usage
        fi
        do_restore "$2"
        ;;
    *)
        usage
        ;;
esac
