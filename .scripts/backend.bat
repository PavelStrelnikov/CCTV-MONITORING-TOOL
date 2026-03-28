@echo off
cd /d "%~dp0.."
call venv\Scripts\activate.bat
python -m cctv_monitor.main > logs\backend.log 2>&1
