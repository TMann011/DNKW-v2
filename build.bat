@echo off
setlocal enabledelayedexpansion

echo =====================================================================
echo   DEATH NOTE Cheater Within - Portable Build Script
echo =====================================================================
echo.

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] .NET SDK is not installed or not in PATH.
    echo Please install .NET 6.0+ SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

set "CUSTOM_GAMEDIR=%~1"
if not "%CUSTOM_GAMEDIR%"=="" (
    echo [*] Using user-specified Game Directory: "%CUSTOM_GAMEDIR%"
    dotnet build -c Release -p:GameDir="%CUSTOM_GAMEDIR%" -p:PathMap="%CD%=." -p:Deterministic=true
) else (
    echo [*] Searching for DEATH NOTE Killer Within installation...
    dotnet build -c Release -p:PathMap="%CD%=." -p:Deterministic=true
)

if %errorlevel% equ 0 (
    echo.
    echo [SUCCESS] Build completed successfully.
    echo Binary: bin\Release\net6.0\DeathNoteCheaterWithin.dll
) else (
    echo.
    echo [NOTE] If build failed due to missing reference DLLs, specify your game folder:
    echo        build.bat "C:\Path\To\DEATH NOTE Killer Within"
)

pause
