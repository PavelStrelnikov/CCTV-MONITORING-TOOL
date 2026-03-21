from fastapi import FastAPI, Request, Response
from fastapi.middleware.cors import CORSMiddleware
from starlette.middleware.base import BaseHTTPMiddleware

from cctv_monitor.core.config import Settings


# Paths that never require authentication
_PUBLIC_PATHS = frozenset({"/health", "/api/auth/login"})
# Path prefixes that accept ?token= query parameter (for <img src>)
_TOKEN_QUERY_PREFIXES = ("/api/devices/", "/api/snapshots/")


class JWTAuthMiddleware(BaseHTTPMiddleware):
    """Verify JWT on all /api/* requests unless auth is disabled."""

    async def dispatch(self, request: Request, call_next):
        settings: Settings = request.app.state.settings

        # Auth disabled (dev mode) — pass through
        if not settings.JWT_SECRET_KEY:
            return await call_next(request)

        path = request.url.path

        # Public endpoints
        if path in _PUBLIC_PATHS:
            return await call_next(request)

        # Only protect /api/* paths
        if not path.startswith("/api/"):
            return await call_next(request)

        # Extract token from Authorization header or ?token= query param
        token = None
        auth_header = request.headers.get("authorization", "")
        if auth_header.startswith("Bearer "):
            token = auth_header[7:]
        elif any(path.startswith(p) for p in _TOKEN_QUERY_PREFIXES):
            token = request.query_params.get("token")

        if not token:
            return Response(status_code=401, content="Missing authentication token")

        from cctv_monitor.api.auth import decode_token

        try:
            decode_token(token, settings)
        except Exception:
            return Response(status_code=401, content="Invalid or expired token")

        return await call_next(request)


def create_app(settings: Settings | None = None) -> FastAPI:
    app = FastAPI(title="CCTV Monitor", version="0.1.0")

    # CORS — configurable origins
    origins = ["http://localhost:5173"]
    if settings and settings.CORS_ORIGINS:
        origins = [o.strip() for o in settings.CORS_ORIGINS.split(",") if o.strip()]

    app.add_middleware(
        CORSMiddleware,
        allow_origins=origins,
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    # JWT auth middleware (must be added after CORS so CORS headers are set on 401)
    app.add_middleware(JWTAuthMiddleware)

    from cctv_monitor.api.auth import router as auth_router
    from cctv_monitor.api.routes.devices import router as devices_router
    from cctv_monitor.api.routes.status import router as status_router
    from cctv_monitor.api.routes.tags import router as tags_router
    from cctv_monitor.api.routes.history import router as history_router
    from cctv_monitor.api.routes.alerts_routes import router as alerts_router
    from cctv_monitor.api.routes.settings import router as settings_router
    from cctv_monitor.api.routes.telegram import router as telegram_router
    from cctv_monitor.api.routes.folders import router as folders_router

    app.include_router(auth_router, prefix="/api")
    app.include_router(devices_router, prefix="/api")
    app.include_router(status_router, prefix="/api")
    app.include_router(tags_router, prefix="/api")
    app.include_router(history_router, prefix="/api")
    app.include_router(alerts_router, prefix="/api")
    app.include_router(settings_router, prefix="/api")
    app.include_router(telegram_router, prefix="/api")
    app.include_router(folders_router, prefix="/api")

    @app.get("/health")
    async def health():
        return {"status": "ok", "service": "cctv-monitor"}

    return app
