@echo off
chcp 65001 >nul
echo [1/4] 关闭旧进程...
taskkill /F /IM kuaijiejian.exe >nul 2>&1
timeout /t 2 /nobreak >nul

echo [2/4] 清理编译缓存...
rmdir /s /q obj >nul 2>&1
rmdir /s /q bin >nul 2>&1

echo [3/4] 重新编译...
dotnet build -v q

if errorlevel 1 (
    echo.
    echo ❌ 编译失败！
    pause
    exit /b 1
)

echo [4/4] 正在启动...
start "" "bin\Debug\net8.0-windows\kuaijiejian.exe"

echo.
echo ✅ 完成！已清理缓存并重新编译
timeout /t 2 /nobreak >nul
exit

