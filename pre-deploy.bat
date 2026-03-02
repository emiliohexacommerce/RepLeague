@echo off
REM Pre-deployment script for RepLeague (Windows)
REM This script prepares the application for deployment to Azure

setlocal enabledelayedexpansion

echo ==========================================
echo Pre-Deployment Setup for RepLeague
echo ==========================================

REM Check prerequisites
echo Checking prerequisites...

REM Check .NET
dotnet --version >nul 2>&1
if !errorlevel! neq 0 (
    echo ERROR: .NET SDK not found. Please install .NET 9.0
    exit /b 1
)

REM Check Node.js
node --version >nul 2>&1
if !errorlevel! neq 0 (
    echo ERROR: Node.js not found. Please install Node.js 18+
    exit /b 1
)

REM Check npm
npm --version >nul 2>&1
if !errorlevel! neq 0 (
    echo ERROR: npm not found. Please make sure Node.js is properly installed
    exit /b 1
)

echo ✓ All prerequisites met

REM Restore .NET packages
echo.
echo Restoring .NET packages...
cd backend\src\RepLeague.API
dotnet restore
dotnet add package Microsoft.ApplicationInsights.AspNetCore
cd ..\..\..

REM Install Frontend dependencies
echo.
echo Installing Frontend dependencies...
cd frontend\replague
call npm ci
cd ..\..

REM Create wwwroot directory if it doesn't exist
echo.
echo Creating wwwroot directory...
if not exist "backend\src\RepLeague.API\wwwroot" mkdir backend\src\RepLeague.API\wwwroot

echo.
echo ==========================================
echo Pre-Deployment Setup Complete!
echo ==========================================
