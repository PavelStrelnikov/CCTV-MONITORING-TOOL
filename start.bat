@echo off
chcp 65001 >nul
cd /d "%~dp0"

if not exist logs mkdir logs

echo ========================================
echo   CCTV Monitoring Tool - Start
echo ========================================
echo.

echo [1/3] Backend (uvicorn :8001)...
start "CCTV Backend" /min "%~dp0.scripts\backend.bat"

echo [2/3] Frontend (vite :5173)...
start "CCTV Frontend" /min "%~dp0.scripts\frontend.bat"

echo [3/3] Telegram bot...
start "CCTV Telegram" /min "%~dp0.scripts\telegram.bat"

echo.
echo All services started (minimized).
echo   Backend:  http://localhost:8001
echo   Frontend: http://localhost:5173
echo.
echo Logs:  logs\backend.log
echo        logs\frontend.log
echo        logs\telegram.log
echo.
echo Use stop.bat to stop all services.
