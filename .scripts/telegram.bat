@echo off
cd /d "%~dp0.."
call venv\Scripts\activate.bat
python -m cctv_monitor.telegram.main > logs\telegram.log 2>&1
