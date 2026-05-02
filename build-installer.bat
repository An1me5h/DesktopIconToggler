@echo off
echo === Desktop Icon Toggler — Installer Builder ===
echo.

echo [1/2] Publishing application (self-contained single exe)...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
if %errorlevel% neq 0 (
    echo.
    echo ERROR: dotnet publish failed. Make sure .NET 6 SDK is installed.
    pause
    exit /b 1
)
echo     Done.
echo.

echo [2/2] Compiling installer with Inno Setup...
set ISCC="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if not exist %ISCC% (
    echo.
    echo ERROR: Inno Setup 6 not found.
    echo Please download and install it from:
    echo   https://jrsoftware.org/isdl.php
    echo Then re-run this script.
    pause
    exit /b 1
)

if not exist installer mkdir installer
%ISCC% installer.iss
if %errorlevel% neq 0 (
    echo.
    echo ERROR: Inno Setup compilation failed.
    pause
    exit /b 1
)

echo.
echo === Build complete! ===
echo Installer is at: installer\DesktopIconTogglerSetup-v1.0.0.exe
echo.
pause
