@echo off
chcp 65001 >nul
echo ========================================
echo Framework Dependent Build (0.71MB, requires .NET 6 runtime)
echo ========================================
echo.

REM Clean previous publish files
echo Cleaning publish directory...
if exist "bin\Release\net6.0-windows\win-x64\publish" (
    rmdir /s /q "bin\Release\net6.0-windows\win-x64\publish"
)

echo Starting build...
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Build Successful!
    echo ========================================
    echo File location: bin\Release\net6.0-windows\win-x64\publish\WindowsSimpleCapture.exe
    
    REM Show file size
    setlocal enabledelayedexpansion
    for %%I in ("bin\Release\net6.0-windows\win-x64\publish\WindowsSimpleCapture.exe") do (
        set size=%%~zI
        set /a sizeMB=!size!/1048576
        if !sizeMB! EQU 0 (
            set /a sizeKB=!size!/1024
            echo File size: !sizeKB! KB
        ) else (
            echo File size: !sizeMB! MB
        )
    )
    
    echo.
    echo Features: Very small size, fast startup, .NET 6 LTS compatibility
    echo Requirements: .NET 6 Desktop Runtime must be pre-installed
    echo Download: https://dotnet.microsoft.com/download/dotnet/6.0
    echo.
    echo Checking if .NET 6 runtime is installed...
    dotnet --list-runtimes | findstr "Microsoft.WindowsDesktop.App 6." >nul
    if !ERRORLEVEL! EQU 0 (
        echo [OK] .NET 6 Desktop Runtime is installed, ready to run
    ) else (
        echo [WARNING] .NET 6 Desktop Runtime not found, please install first
    )
) else (
    echo.
    echo ========================================
    echo Build Failed! Please check error messages
    echo ========================================
)

echo.
pause