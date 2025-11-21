@echo off
chcp 65001 >nul
echo ========================================
echo    Photoshop 快捷键工具 - 一键打包
echo ========================================
echo.

:: 设置打包目录名称（包含日期时间）
for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
set "YYYY=%dt:~0,4%"
set "MM=%dt:~4,2%"
set "DD=%dt:~6,2%"
set "HH=%dt:~8,2%"
set "Min=%dt:~10,2%"
set "timestamp=%YYYY%%MM%%DD%_%HH%%Min%"

set "PACKAGE_NAME=Photoshop快捷键工具_v1.0_%timestamp%"
set "PACKAGE_DIR=发布包\%PACKAGE_NAME%"

echo 正在准备打包...
echo 目标目录: %PACKAGE_DIR%
echo.

:: 删除旧的发布包目录（如果存在）
if exist "发布包" (
    echo 清理旧的发布包...
    rd /s /q "发布包"
)

:: 创建发布包目录结构
echo 创建目录结构...
mkdir "%PACKAGE_DIR%"
mkdir "%PACKAGE_DIR%\PhotoshopScripts"
mkdir "%PACKAGE_DIR%\配置文件"
echo.

:: 检查编译文件是否存在
if not exist "bin\Debug\net8.0-windows\kuaijiejian.exe" (
    echo [错误] 找不到编译后的程序文件！
    echo 请先运行"重新编译并启动.bat"进行编译。
    echo.
    pause
    exit /b 1
)

:: 复制主程序文件
echo [1/6] 复制主程序文件...
copy /Y "bin\Debug\net8.0-windows\kuaijiejian.exe" "%PACKAGE_DIR%\" >nul
copy /Y "bin\Debug\net8.0-windows\kuaijiejian.dll" "%PACKAGE_DIR%\" >nul
copy /Y "bin\Debug\net8.0-windows\kuaijiejian.runtimeconfig.json" "%PACKAGE_DIR%\" >nul
copy /Y "bin\Debug\net8.0-windows\kuaijiejian.deps.json" "%PACKAGE_DIR%\" >nul
echo    ✓ 主程序文件复制完成

:: 复制依赖库
echo [2/6] 复制依赖库...
copy /Y "bin\Debug\net8.0-windows\HandyControl.dll" "%PACKAGE_DIR%\" >nul
echo    ✓ 依赖库复制完成

:: 复制配置文件
echo [3/6] 复制配置文件...
if exist "functions_config.json" (
    copy /Y "functions_config.json" "%PACKAGE_DIR%\配置文件\" >nul
)
if exist "theme_config.json" (
    copy /Y "theme_config.json" "%PACKAGE_DIR%\配置文件\" >nul
)
if exist "bin\Debug\net8.0-windows\display_mode_config.json" (
    copy /Y "bin\Debug\net8.0-windows\display_mode_config.json" "%PACKAGE_DIR%\配置文件\" >nul
)
if exist "bin\Debug\net8.0-windows\functions_config.json" (
    copy /Y "bin\Debug\net8.0-windows\functions_config.json" "%PACKAGE_DIR%\配置文件\" >nul
)
echo    ✓ 配置文件复制完成

:: 复制Photoshop脚本
echo [4/6] 复制Photoshop脚本文件...
xcopy /Y /Q "PhotoshopScripts\*.jsx" "%PACKAGE_DIR%\PhotoshopScripts\" >nul 2>&1
echo    ✓ Photoshop脚本复制完成

:: 复制说明文档
echo [5/6] 复制说明文档...
if exist "使用说明.html" (
    copy /Y "使用说明.html" "%PACKAGE_DIR%\" >nul
)
if exist "使用说明.txt" (
    copy /Y "使用说明.txt" "%PACKAGE_DIR%\" >nul
)
if exist "README.md" (
    copy /Y "README.md" "%PACKAGE_DIR%\" >nul
)
echo    ✓ 说明文档复制完成

:: 创建快速启动脚本
echo [6/6] 创建快速启动脚本...
(
    echo @echo off
    echo chcp 65001 ^>nul
    echo echo 正在启动 Photoshop 快捷键工具...
    echo start "" "kuaijiejian.exe"
    echo exit
) > "%PACKAGE_DIR%\启动工具.bat"
echo    ✓ 快速启动脚本创建完成

echo.
echo ========================================
echo           打包完成！
echo ========================================
echo.
echo 打包目录: %PACKAGE_DIR%
echo.
echo 接下来您可以：
echo  1. 打开"发布包"文件夹查看打包结果
echo  2. 将整个"%PACKAGE_NAME%"文件夹发送给其他用户
echo  3. 用户只需解压后双击"启动工具.bat"即可使用
echo.

:: 打开发布包目录
echo 是否要打开发布包目录？
pause
explorer "发布包"

echo.
echo 打包流程结束。
pause

