@echo off
chcp 65001 >nul
echo ========================================
echo   CCTV Monitoring Tool - Stop
echo ========================================
echo.

echo Stopping backend (port 8001)...
for /f "tokens=5" %%p in ('netstat -ano ^| findstr ":8001.*LISTENING"') do (
    taskkill /f /pid %%p >nul 2>&1
)

echo Stopping frontend (port 5173)...
for /f "tokens=5" %%p in ('netstat -ano ^| findstr ":5173.*LISTENING"') do (
    taskkill /f /pid %%p >nul 2>&1
)

echo Stopping Telegram bot...
for /f "tokens=2" %%p in ('wmic process where "commandline like '%%cctv_monitor.telegram.main%%'" get processid /format:value 2^>nul ^| findstr "="') do (
    taskkill /f /pid %%p >nul 2>&1
)

echo.
echo All services stopped.
