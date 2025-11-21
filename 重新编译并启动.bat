@echo off
title Build and Run

echo ========================================
echo Step 1/3: Kill old process...
echo ========================================
taskkill /F /IM kuaijiejian.exe 2>nul
if errorlevel 1 (
    echo No running process found
) else (
    echo Process killed
    timeout /t 1 /nobreak >nul
)

echo.
echo ========================================
echo Step 2/3: Building...
echo ========================================
dotnet build kuaijiejian.csproj -c Debug

if %errorlevel% neq 0 (
    echo.
    echo ========================================
    echo BUILD FAILED! Check errors above
    echo ========================================
    pause
    exit /b 1
)

echo.
echo ========================================
echo Step 3/3: Starting...
echo ========================================
start "" "bin\Debug\net8.0-windows\kuaijiejian.exe"

echo.
echo Build and Start SUCCESS!
echo ========================================
timeout /t 3 /nobreak >nul
