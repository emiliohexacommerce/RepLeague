@echo off
REM Redeploy del Frontend con web.config
setlocal enabledelayedexpansion

cd .\frontend\replague\dist

echo.
echo Creando ZIP para redeploy del frontend...
if exist "frontend-redeploy.zip" del "frontend-redeploy.zip"
powershell -NoProfile -Command "Get-ChildItem -Path '.' | Compress-Archive -DestinationPath 'frontend-redeploy.zip' -Force"

echo.
echo Desplegando frontend a Azure...
set AZ_CLI=C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd
call "%AZ_CLI%" webapp deployment source config-zip --resource-group DISCENTRO --name repleague --src "frontend-redeploy.zip"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✓ Frontend redesplegado exitosamente
    echo.
    echo Aguardando 30 segundos para que se inicie...
    timeout /t 30 /nobreak
    echo.
    echo ✓ Frontend debe estar disponible en: https://repleague.azurewebsites.net
) else (
    echo.
    echo ❌ Error en el redepliegue
    pause
    exit /b 1
)

cd ..\..\..\

echo.
echo Verificando acceso al frontend...
timeout /t 10 /nobreak
powershell -NoProfile -Command "try { $resp = Invoke-WebRequest -Uri 'https://repleague.azurewebsites.net/' -UseBasicParsing -TimeoutSec 20; Write-Host '✅ Frontend respondió: Status ' $resp.StatusCode } catch { Write-Host '⏳ Frontend aún se está iniciando...' }"

pause
