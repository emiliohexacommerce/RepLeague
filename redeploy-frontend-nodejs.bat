@echo off
REM Redeploy del Frontend - Con servidor Node.js
setlocal enabledelayedexpansion

echo.
echo === Preparando archivos para despliegue ===
echo.

REM Copiar server.js y package.json al dist
echo Copiando server.js al directorio dist...
copy "frontend\replague\server.js" "frontend\replague\dist\server.js" /Y >nul
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Error copiando server.js
    exit /b 1
)

echo Copiando package.json al directorio dist...
copy "frontend\replague\package.json" "frontend\replague\dist\package.json" /Y >nul
if %ERRORLEVEL% NEQ 0 (
    echo ❌ Error copiando package.json
    exit /b 1
)

echo ✓ Archivos copiados

echo.
echo === Creando archivo de despliegue ===

cd .\frontend\replague\dist

if exist "frontend-deploy.zip" del "frontend-deploy.zip"

echo Creando ZIP...
powershell -NoProfile -Command "Get-ChildItem -Path '.' -Recurse | Where-Object { -not $_.PSIsContainer } | Compress-Archive -DestinationPath 'frontend-deploy.zip' -Force"

if %ERRORLEVEL% NEQ 0 (
    echo ❌ Error creando ZIP
    exit /b 1
)

echo ✓ ZIP creado

echo.
echo === Desplegando a Azure ===

set AZ_CLI=C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd
call "%AZ_CLI%" webapp deployment source config-zip --resource-group DISCENTRO --name repleague --src "frontend-deploy.zip"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ Error en el despliegue
    cd ..\..\..
    exit /b 1
)

echo.
echo ✓ Despliegue iniciado
echo Aguardando 30 segundos para que se inicie...
timeout /t 30 /nobreak

cd ..\..\..

echo.
echo === Verificando acceso ===

powershell -NoProfile -Command "
try {
    $resp = Invoke-WebRequest -Uri 'https://repleague.azurewebsites.net/' -UseBasicParsing -TimeoutSec 20
    Write-Host '✅ Frontend respondió: Status ' $resp.StatusCode -ForegroundColor Green
    if ($resp.Content -match '<body' -or $resp.Content -match '<app-root') {
        Write-Host '✅ Contenido Angular detectado' -ForegroundColor Green
    }
} catch {
    Write-Host '⏳ Frontend aún se está iniciando...' -ForegroundColor Yellow
}
"

echo.
echo ✅ Despliegue completado
echo Accede a: https://repleague.azurewebsites.net
echo.

pause
