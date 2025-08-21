@echo off
chcp 65001 >nul
echo ========================================
echo Windows Simple Capture Build Script
echo ========================================
echo.
echo Please select build version:
echo.
echo 1. Self-Contained (65MB, no .NET runtime required)
echo    - Pros: Truly portable, runs on any Win10+ system
echo    - Cons: Larger file size
echo.
echo 2. Framework-Dependent (0.71MB, requires .NET 6 runtime)
echo    - Pros: Very small size, fast startup, LTS compatibility
echo    - Cons: Requires .NET 6 Desktop Runtime pre-installed
echo.
echo 3. Build both versions
echo.
set /p choice="Enter your choice (1/2/3): "

if "%choice%"=="1" (
    echo.
    echo Starting self-contained build...
    call build-self-contained.bat
) else if "%choice%"=="2" (
    echo.
    echo Starting framework-dependent build...
    call build-framework-dependent.bat
) else if "%choice%"=="3" (
    echo.
    echo Building both versions...
    echo.
    echo [1/2] Building framework-dependent version...
    call build-framework-dependent.bat
    echo.
    echo [2/2] Building self-contained version...
    call build-self-contained.bat
    echo.
    echo ========================================
    echo Both versions built successfully!
    echo ========================================
    echo Framework-dependent: bin\Release\net6.0-windows\win-x64\publish\WindowsSimpleCapture.exe (~0.71MB)
    echo Self-contained: bin\Release\net6.0-windows\win-x64\publish\WindowsSimpleCapture.exe (~65MB)
    echo.
    pause
) else (
    echo Invalid choice, please run the script again
    pause
)