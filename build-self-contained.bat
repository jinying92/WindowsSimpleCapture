@echo off
chcp 65001 >nul
echo ========================================
echo Self-Contained Build (65MB, no .NET runtime required)
echo ========================================
echo.

REM Clean previous publish files
echo Cleaning publish directory...
if exist "bin\Release\net6.0-windows\win-x64\publish" (
    rmdir /s /q "bin\Release\net6.0-windows\win-x64\publish"
)

REM Backup original project file
echo Backing up original project configuration...
copy "WindowsSimpleCapture.csproj" "WindowsSimpleCapture.csproj.backup" >nul

REM Create self-contained project configuration
echo Creating self-contained configuration...
echo ^<Project Sdk="Microsoft.NET.Sdk"^> > WindowsSimpleCapture.csproj
echo. >> WindowsSimpleCapture.csproj
echo   ^<PropertyGroup^> >> WindowsSimpleCapture.csproj
echo     ^<OutputType^>WinExe^</OutputType^> >> WindowsSimpleCapture.csproj
echo     ^<TargetFramework^>net6.0-windows^</TargetFramework^> >> WindowsSimpleCapture.csproj
echo     ^<Nullable^>enable^</Nullable^> >> WindowsSimpleCapture.csproj
echo     ^<UseWPF^>true^</UseWPF^> >> WindowsSimpleCapture.csproj
echo     ^<SelfContained^>true^</SelfContained^> >> WindowsSimpleCapture.csproj
echo     ^<PublishSingleFile^>true^</PublishSingleFile^> >> WindowsSimpleCapture.csproj
echo     ^<PublishTrimmed^>false^</PublishTrimmed^> >> WindowsSimpleCapture.csproj
echo     ^<IncludeNativeLibrariesForSelfExtract^>true^</IncludeNativeLibrariesForSelfExtract^> >> WindowsSimpleCapture.csproj
echo     ^<EnableCompressionInSingleFile^>true^</EnableCompressionInSingleFile^> >> WindowsSimpleCapture.csproj
echo     ^<IncludeAllContentForSelfExtract^>true^</IncludeAllContentForSelfExtract^> >> WindowsSimpleCapture.csproj
echo   ^</PropertyGroup^> >> WindowsSimpleCapture.csproj
echo. >> WindowsSimpleCapture.csproj
echo   ^<ItemGroup^> >> WindowsSimpleCapture.csproj
echo     ^<PackageReference Include="System.Drawing.Common" Version="8.0.0" /^> >> WindowsSimpleCapture.csproj
echo   ^</ItemGroup^> >> WindowsSimpleCapture.csproj
echo. >> WindowsSimpleCapture.csproj
echo ^</Project^> >> WindowsSimpleCapture.csproj

echo Starting build...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

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
        echo File size: !sizeMB! MB
    )
    
    echo.
    echo Features: No .NET runtime required, runs directly on any Win10+ system
) else (
    echo.
    echo ========================================
    echo Build Failed! Please check error messages
    echo ========================================
)

REM Restore original configuration
echo Restoring original project configuration...
if exist "WindowsSimpleCapture.csproj.backup" (
    copy "WindowsSimpleCapture.csproj.backup" "WindowsSimpleCapture.csproj" >nul
    del "WindowsSimpleCapture.csproj.backup" >nul
)

echo.
pause