# CCTV Monitoring Tool — Production Deployment Guide

## Requirements

- Linux server (Ubuntu 22.04+ recommended)
- Docker Engine 24+ and Docker Compose v2
- Domain name (optional, for automatic SSL via Let's Encrypt)
- Open ports: 80 (HTTP), 443 (HTTPS)
- Minimum 2GB RAM, 10GB disk

## Architecture

```
Internet → Caddy (:80/:443, auto SSL)
              ├── /api/* → backend:8001 (FastAPI)
              └── /*     → static frontend (React SPA)
           PostgreSQL (:5432, internal only)
           Telegram Bot → backend:8001 (internal API)
```

4 Docker services: `postgres`, `backend`, `telegram-bot`, `caddy`

## Quick Start

### 1. Clone repository

```bash
git clone https://github.com/PavelStrelnikov/CCTV-MONITORING-TOOL.git
cd CCTV-MONITORING-TOOL/deploy
```

### 2. Configure environment

```bash
cp .env.example .env
nano .env
```

Fill in all required values. Generate secrets:

```bash
# Database password
openssl rand -base64 32

# Encryption key
openssl rand -base64 32

# JWT secret
openssl rand -base64 64

# Admin password hash (replace YOUR_PASSWORD)
python3 -c "import bcrypt; print(bcrypt.hashpw(b'YOUR_PASSWORD', bcrypt.gensalt()).decode())"

# Internal API token (for Telegram bot)
openssl rand -base64 32
```

### 3. Build and start

```bash
docker compose -f docker-compose.prod.yml up -d --build
```

Wait for all services to start:

```bash
docker compose -f docker-compose.prod.yml ps
```

### 4. Verify

```bash
# Health check
curl http://localhost/health

# Or with domain
curl https://your-domain.com/health
```

Open browser → you should see the login page.

## Migrating Data from Development

### Export from dev machine (Windows)

```bash
# Create backup from dev PostgreSQL
docker exec cctv_monitoring_postgres pg_dump -U cctv_admin -d cctv_monitoring --format=custom > migration.dump
```

### Transfer to production server

```bash
scp migration.dump user@your-server:~/CCTV-MONITORING-TOOL/deploy/backups/
```

### Import on production

```bash
cd ~/CCTV-MONITORING-TOOL/deploy

# Restore database
./scripts/db-backup.sh restore backups/migration.dump

# Restart backend to apply any pending migrations
docker compose -f docker-compose.prod.yml restart backend
```

## Backup & Restore

### Create backup

```bash
cd ~/CCTV-MONITORING-TOOL/deploy
./scripts/db-backup.sh backup
# Output: backups/cctv_20260321_120000.dump
```

### Restore from backup

```bash
./scripts/db-backup.sh restore backups/cctv_20260321_120000.dump
docker compose -f docker-compose.prod.yml restart backend
```

### Scheduled backups (cron)

```bash
# Daily backup at 3:00 AM
crontab -e
0 3 * * * /home/user/CCTV-MONITORING-TOOL/deploy/scripts/db-backup.sh backup
```

## SSL with Let's Encrypt

Set your domain in `.env`:

```
DOMAIN=cctv.your-domain.com
```

Ensure DNS A record points to your server's IP. Caddy will automatically obtain and renew the SSL certificate.

For local testing without SSL, keep `DOMAIN=localhost`.

## Management

### View logs

```bash
# All services
docker compose -f docker-compose.prod.yml logs -f

# Specific service
docker compose -f docker-compose.prod.yml logs -f backend
docker compose -f docker-compose.prod.yml logs -f telegram-bot
```

### Restart services

```bash
docker compose -f docker-compose.prod.yml restart backend
docker compose -f docker-compose.prod.yml restart telegram-bot
```

### Stop all services

```bash
docker compose -f docker-compose.prod.yml down
```

### Update to latest version

```bash
cd ~/CCTV-MONITORING-TOOL
git pull
cd deploy
docker compose -f docker-compose.prod.yml up -d --build
```

Database migrations run automatically on backend startup.

## Troubleshooting

### Backend won't start

```bash
docker compose -f docker-compose.prod.yml logs backend
```

Common issues:
- Missing environment variables in `.env`
- PostgreSQL not ready yet (wait for healthcheck)
- Invalid `ADMIN_PASSWORD_HASH` format

### Can't connect from browser

- Check firewall: ports 80 and 443 must be open
- Check Caddy logs: `docker compose -f docker-compose.prod.yml logs caddy`
- For SSL issues, ensure DNS is configured and domain resolves to server IP

### Telegram bot not working

- Ensure `TELEGRAM_BOT_TOKEN` is set in `.env`
- Ensure `INTERNAL_API_TOKEN` matches between bot and backend
- Check logs: `docker compose -f docker-compose.prod.yml logs telegram-bot`

## File Structure

```
deploy/
├── docker-compose.prod.yml   ← Main compose file
├── Dockerfile.backend         ← Backend image (Python + SDK)
├── Dockerfile.caddy           ← Frontend build + Caddy reverse proxy
├── Caddyfile                  ← Caddy configuration
├── docker-entrypoint.sh       ← Backend startup (migrations + app)
├── .dockerignore              ← Docker build exclusions
├── .env.example               ← Environment template
├── .env                       ← Your configuration (git-ignored)
├── backups/                   ← Database dumps
│   └── cctv_*.dump
├── scripts/
│   └── db-backup.sh           ← Backup/restore utility
└── DEPLOY.md                  ← This file
```
