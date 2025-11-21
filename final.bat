@echo off
chcp 65001 >nul
taskkill /F /IM kuaijiejian.exe >nul 2>&1
dotnet restore
dotnet build kuaijiejian.csproj -c Debug
if %errorlevel% neq 0 pause
start "" "bin\Debug\net8.0-windows\kuaijiejian.exe"


