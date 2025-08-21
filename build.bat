@echo off
chcp 65001 >nul
echo ========================================
echo Windows Simple Capture Build Script
echo ========================================
echo.
echo Please select build version:
echo.
echo 1. Self-Contained (64-bit, 65MB, no .NET runtime required)
echo    - Pros: Truly portable, runs on any Win10+ 64-bit system
echo    - Cons: Larger file size
echo.
echo 2. Framework-Dependent (64-bit, 0.71MB, requires .NET 6 runtime)
echo    - Pros: Very small size, fast startup, LTS compatibility
echo    - Cons: Requires .NET 6 Desktop Runtime pre-installed
echo.
echo 3. Self-Contained (32-bit, for older systems)
echo    - Pros: Runs on 32-bit Windows systems
echo    - Cons: Larger file size
echo.
echo 4. Framework-Dependent (32-bit, for older systems)
echo    - Pros: Smaller size for 32-bit systems
echo    - Cons: Requires .NET 6 Desktop Runtime pre-installed
echo.
echo 5. Build all versions
echo.
set /p choice="Enter your choice (1/2/3/4/5): "

if "%choice%"=="1" (
    echo.
    echo Starting 64-bit self-contained build...
    call build-self-contained.bat
) else if "%choice%"=="2" (
    echo.
    echo Starting 64-bit framework-dependent build...
    call build-framework-dependent.bat
) else if "%choice%"=="3" (
    echo.
    echo Starting 32-bit self-contained build...
    call build-self-contained-x86.bat
) else if "%choice%"=="4" (
    echo.
    echo Starting 32-bit framework-dependent build...
    call build-framework-dependent-x86.bat
) else if "%choice%"=="5" (
    echo.
    echo Building all versions...
    echo.
    echo [1/4] Building 64-bit framework-dependent version...
    call build-framework-dependent.bat
    echo.
    echo [2/4] Building 64-bit self-contained version...
    call build-self-contained.bat
    echo.
    echo [3/4] Building 32-bit framework-dependent version...
    call build-framework-dependent-x86.bat
    echo.
    echo [4/4] Building 32-bit self-contained version...
    call build-self-contained-x86.bat
    echo.
    echo ========================================
    echo All versions built successfully!
    echo ========================================
    echo 64-bit Framework-dependent: bin\Release\net6.0-windows\win-x64\publish\WindowsSimpleCapture.exe (~0.71MB)
    echo 64-bit Self-contained: bin\Release\net6.0-windows\win-x64\publish\WindowsSimpleCapture.exe (~65MB)
    echo 32-bit Framework-dependent: bin\Release\net6.0-windows\win-x86\publish\WindowsSimpleCapture.exe
    echo 32-bit Self-contained: bin\Release\net6.0-windows\win-x86\publish\WindowsSimpleCapture.exe
    echo.
    pause
) else (
    echo Invalid choice, please run the script again
    pause
)