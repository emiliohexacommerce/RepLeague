@echo off
REM ============================================================
REM Script de Despliegue - RepLeague a Web Apps Existentes
REM Solo binarios compilados (sin código fuente)
REM ============================================================

setlocal enabledelayedexpansion

REM Configuración
set RESOURCE_GROUP=DISCENTRO
set BACKEND_APP=api-repleague
set FRONTEND_APP=replague
set AZ_CLI=C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd
set BACKEND_DIR=backend
set FRONTEND_DIR=frontend\replague

echo.
echo ========================================
echo RepLeague - Despliegue a Azure Web Apps
echo ========================================
echo Resource Group: %RESOURCE_GROUP%
echo Backend App:    %BACKEND_APP%
echo Frontend App:   %FRONTEND_APP%
echo.

REM ============================================================
REM 1. Compilar Backend
REM ============================================================
echo [1/4] Compilando Backend (.NET)...
pushd %BACKEND_DIR%
dotnet publish -c Release -o .\publish\api --no-self-contained
if %ERRORLEVEL% NEQ 0 (
    popd
    echo.
    echo ❌ Error compilando Backend
    exit /b 1
)
echo ✓ Backend compilado
popd

REM ============================================================
REM 2. Compilar Frontend
REM ============================================================
echo.
echo [2/4] Compilando Frontend (Angular)...
pushd %FRONTEND_DIR%
call npm install --legacy-peer-deps
if %ERRORLEVEL% NEQ 0 (
    popd
    echo.
    echo ❌ Error en npm install del Frontend
    exit /b 1
)
call npm run build -- --configuration production --base-href /
if %ERRORLEVEL% NEQ 0 (
    popd
    echo.
    echo ❌ Error compilando Frontend
    exit /b 1
)
echo ✓ Frontend compilado
popd

REM ============================================================
REM 3. Desplegar Backend (solo binarios)
REM ============================================================
echo.
echo [3/4] Desplegando Backend a %BACKEND_APP%...
pushd %BACKEND_DIR%\publish\api

REM Crear ZIP con solo binarios
if exist "api-deploy.zip" del "api-deploy.zip"
echo   Creando archivo ZIP...
powershell -NoProfile -Command "Get-ChildItem -Path '.' -Recurse | Where-Object {$_.PsIsContainer -eq $false} | Compress-Archive -DestinationPath 'api-deploy.zip' -Force"

if %ERRORLEVEL% NEQ 0 (
    popd
    echo.
    echo ❌ Error creando ZIP del Backend
    exit /b 1
)

REM Desplegar
echo   Subiendo a Azure... (esto puede tomar 1-2 minutos)
call "%AZ_CLI%" webapp deployment source config-zip ^
    --resource-group %RESOURCE_GROUP% ^
    --name %BACKEND_APP% ^
    --src "api-deploy.zip"

if %ERRORLEVEL% NEQ 0 (
    popd
    echo.
    echo ❌ Error desplegando Backend
    exit /b 1
)
echo ✓ Backend desplegado
popd

REM Esperar a que el backend esté listo
echo.
echo ⏳ Esperando reinicio del backend... (45 segundos)
timeout /t 45 /nobreak

REM ============================================================
REM 4. Desplegar Frontend (solo dist)
REM ============================================================
echo.
echo [4/4] Desplegando Frontend a %FRONTEND_APP%...
pushd %FRONTEND_DIR%\dist

REM Crear ZIP con solo archivos compilados
if exist "frontend-deploy.zip" del "frontend-deploy.zip"
echo   Creando archivo ZIP...
powershell -NoProfile -Command "Get-ChildItem -Path '.' -Recurse | Where-Object {$_.PsIsContainer -eq $false} | Compress-Archive -DestinationPath 'frontend-deploy.zip' -Force"

if %ERRORLEVEL% NEQ 0 (
    popd
    echo.
    echo ❌ Error creando ZIP del Frontend
    exit /b 1
)

REM Desplegar
echo   Subiendo a Azure... (esto puede tomar 1-2 minutos)
call "%AZ_CLI%" webapp deployment source config-zip ^
    --resource-group %RESOURCE_GROUP% ^
    --name %FRONTEND_APP% ^
    --src "frontend-deploy.zip"

if %ERRORLEVEL% NEQ 0 (
    popd
    echo.
    echo ❌ Error desplegando Frontend
    exit /b 1
)
echo ✓ Frontend desplegado
popd

REM ============================================================
REM 5. Resumen
REM ============================================================
echo.
echo ========================================
echo ✅ Despliegue completado exitosamente
echo ========================================
echo.
echo URLs de acceso:
echo   Backend:  https://%BACKEND_APP%.azurewebsites.net
echo   Frontend: https://%FRONTEND_APP%.azurewebsites.net
echo.
echo Swagger (para probar API):
echo   https://%BACKEND_APP%.azurewebsites.net/swagger
echo.
echo Verifica en 2-3 minutos que ambas aplicaciones estén en línea.
echo.

pause
