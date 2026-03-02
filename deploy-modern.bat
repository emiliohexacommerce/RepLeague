@echo off
REM ============================================================
REM Script de Despliegue Moderno - RepLeague a Web Apps Existentes
REM ============================================================
REM Configuración
set RESOURCE_GROUP=DISCENTRO
set BACKEND_APP=api-repleague
set FRONTEND_APP=replague
set AZ_CLI=C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd
set BACKEND_DIR=.\backend
set FRONTEND_DIR=.\frontend\replague

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
call dotnet publish -c Release -o .\publish\api
if %ERRORLEVEL% NEQ 0 (
    popd
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
    echo ❌ Error en npm install del Frontend
    exit /b 1
)
call npm run build -- --configuration production --base-href /
if %ERRORLEVEL% NEQ 0 (
    popd
    echo ❌ Error compilando Frontend
    exit /b 1
)
echo ✓ Frontend compilado
popd

REM ============================================================
REM 3. Desplegar Backend (con comando moderno)
REM ============================================================
echo.
echo [3/4] Desplegando Backend a %BACKEND_APP%...
pushd %BACKEND_DIR%\publish\api

REM Crear ZIP para despliegue
if exist "api-deploy.zip" del "api-deploy.zip"
powershell -Command "Get-ChildItem -Path '.' | Compress-Archive -DestinationPath 'api-deploy.zip' -Force"

REM Desplegar usando comando moderno az webapp deploy
call "%AZ_CLI%" webapp deploy --resource-group %RESOURCE_GROUP% --name %BACKEND_APP% --src-path "api-deploy.zip" --type zip
if %ERRORLEVEL% NEQ 0 (
    echo ⚠️  Error en despliegue, intentando alternativa...
    REM Fallback al comando antiguo
    call "%AZ_CLI%" webapp deployment source config-zip --resource-group %RESOURCE_GROUP% --name %BACKEND_APP% --src "api-deploy.zip"
    if %ERRORLEVEL% NEQ 0 (
        popd
        echo ❌ Error desplegando Backend
        exit /b 1
    )
)
echo ✓ Backend desplegado
popd

timeout /t 30 /nobreak

REM ============================================================
REM 4. Desplegar Frontend
REM ============================================================
echo.
echo [4/4] Desplegando Frontend a %FRONTEND_APP%...
pushd %FRONTEND_DIR%\dist

REM Crear ZIP para despliegue
if exist "frontend-deploy.zip" del "frontend-deploy.zip"
powershell -Command "Get-ChildItem -Path '.' | Compress-Archive -DestinationPath 'frontend-deploy.zip' -Force"

REM Desplegar usando comando moderno
call "%AZ_CLI%" webapp deploy --resource-group %RESOURCE_GROUP% --name %FRONTEND_APP% --src-path "frontend-deploy.zip" --type zip
if %ERRORLEVEL% NEQ 0 (
    echo ⚠️  Error en despliegue, intentando alternativa...
    REM Fallback al comando antiguo
    call "%AZ_CLI%" webapp deployment source config-zip --resource-group %RESOURCE_GROUP% --name %FRONTEND_APP% --src "frontend-deploy.zip"
    if %ERRORLEVEL% NEQ 0 (
        popd
        echo ❌ Error desplegando Frontend
        exit /b 1
    )
)
echo ✓ Frontend desplegado
popd

REM ============================================================
REM 5. URLs de Acceso
REM ============================================================
echo.
echo ========================================
echo ✅ Despliegue completado exitosamente
echo ========================================
echo.
echo URLs de acceso:
echo  Backend:  https://%BACKEND_APP%.azurewebsites.net
echo  Frontend: https://%FRONTEND_APP%.azurewebsites.net
echo.
echo Swagger (para probar API):
echo  https://%BACKEND_APP%.azurewebsites.net/swagger
echo.

pause
