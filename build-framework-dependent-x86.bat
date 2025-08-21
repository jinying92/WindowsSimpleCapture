@echo off
chcp 65001 >nul
echo ========================================
echo Framework Dependent Build for 32-bit Systems (x86)
echo ========================================
echo.

REM Clean previous publish files
echo Cleaning publish directory...
if exist "bin\Release\net6.0-windows\win-x86\publish" (
    rmdir /s /q "bin\Release\net6.0-windows\win-x86\publish"
)

echo Starting build...
dotnet publish -c Release -r win-x86 --self-contained false -p:PublishSingleFile=true

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Build Successful!
    echo ========================================
    echo File location: bin\Release\net6.0-windows\win-x86\publish\WindowsSimpleCapture.exe
    
    REM Show file size
    setlocal enabledelayedexpansion
    for %%I in ("bin\Release\net6.0-windows\win-x86\publish\WindowsSimpleCapture.exe") do (
        set size=%%~zI
        set /a sizeMB=!size!/1048576
        echo File size: !sizeMB! MB
    )
    
    echo.
    echo Features: Requires .NET 6 runtime, smaller file size
) else (
    echo.
    echo ========================================
    echo Build Failed!
    echo ========================================
)

echo.
echo Press any key to exit...
pause >nul