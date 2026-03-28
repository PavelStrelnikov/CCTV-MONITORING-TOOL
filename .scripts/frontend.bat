@echo off
pushd "%~dp0.."
cd frontend
npx vite > "..\logs\frontend.log" 2>&1
popd
