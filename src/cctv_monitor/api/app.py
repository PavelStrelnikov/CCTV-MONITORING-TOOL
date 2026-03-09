from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware


def create_app() -> FastAPI:
    app = FastAPI(title="CCTV Monitor", version="0.1.0")

    app.add_middleware(
        CORSMiddleware,
        allow_origins=["http://localhost:5173"],
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    from cctv_monitor.api.routes.devices import router as devices_router
    from cctv_monitor.api.routes.status import router as status_router
    from cctv_monitor.api.routes.tags import router as tags_router
    from cctv_monitor.api.routes.history import router as history_router
    from cctv_monitor.api.routes.alerts_routes import router as alerts_router
    from cctv_monitor.api.routes.settings import router as settings_router

    app.include_router(devices_router, prefix="/api")
    app.include_router(status_router, prefix="/api")
    app.include_router(tags_router, prefix="/api")
    app.include_router(history_router, prefix="/api")
    app.include_router(alerts_router, prefix="/api")
    app.include_router(settings_router, prefix="/api")

    @app.get("/health")
    async def health():
        return {"status": "ok", "service": "cctv-monitor"}

    return app
