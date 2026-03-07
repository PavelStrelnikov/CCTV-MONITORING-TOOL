"""Integration tests for repositories.

These tests require a running PostgreSQL database.
Run with: pytest tests/integration/ -v

To set up the database:
  docker compose up -d
  alembic upgrade head
"""
