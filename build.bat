@echo off
REM Build script for RepLeague (Windows)
REM Separa backend .NET y frontend Angular

setlocal enabledelayedexpansion

echo ==========================================
echo Building RepLeague Application
echo ==========================================

REM Build Backend (.NET)
echo.
echo Building Backend (.NET 9.0)...
cd backend\src\RepLeague.API
dotnet publish -c Release -o ..\..\publish\api
cd ..\..\..

if errorlevel 1 (
    echo ERROR: Backend build failed!
    exit /b 1
)

REM Build Frontend (Angular)
echo.
echo Building Frontend (Angular 18)...
cd frontend\replague
call npm ci
call npm run build -- --configuration production --base-href /
cd ..\..

if errorlevel 1 (
    echo ERROR: Frontend build failed!
    exit /b 1
)

echo.
echo ==========================================
echo Build Complete!
echo ==========================================
echo Backend published to: backend\publish\api
echo Frontend built to: frontend\replague\dist

