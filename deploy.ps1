# ============================================================
# Script de Despliegue - RepLeague a Web Apps Existentes
# Solo binarios compilados (sin código fuente)
# ============================================================

$ErrorActionPreference = "Stop"

# Configuración
$RESOURCE_GROUP = "DISCENTRO"
$BACKEND_APP = "api-repleague"
$FRONTEND_APP = "replague"
$AZ_CLI = "C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd"
$BACKEND_DIR = ".\backend"
$FRONTEND_DIR = ".\frontend\replague"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RepLeague - Despliegue a Azure Web Apps" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Resource Group: $RESOURCE_GROUP" -ForegroundColor Gray
Write-Host "Backend App:    $BACKEND_APP" -ForegroundColor Gray
Write-Host "Frontend App:   $FRONTEND_APP" -ForegroundColor Gray
Write-Host ""

try {
    # ============================================================
    # 1. Compilar Backend
    # ============================================================
    Write-Host "[1/4] Compilando Backend (.NET)..." -ForegroundColor Yellow
    Push-Location "$BACKEND_DIR"
    & dotnet publish -c Release -o .\publish\api --no-self-contained
    if ($LASTEXITCODE -ne 0) { throw "Error compilando Backend" }
    Write-Host "✓ Backend compilado" -ForegroundColor Green
    Pop-Location

    # ============================================================
    # 2. Compilar Frontend
    # ============================================================
    Write-Host ""
    Write-Host "[2/4] Compilando Frontend (Angular)..." -ForegroundColor Yellow
    Push-Location "$FRONTEND_DIR"
    & npm install --legacy-peer-deps
    if ($LASTEXITCODE -ne 0) { throw "Error en npm install" }
    & npm run build -- --configuration production --base-href /
    if ($LASTEXITCODE -ne 0) { throw "Error compilando Frontend" }
    Write-Host "✓ Frontend compilado" -ForegroundColor Green
    Pop-Location

    # ============================================================
    # 3. Desplegar Backend (solo binarios)
    # ============================================================
    Write-Host ""
    Write-Host "[3/4] Desplegando Backend a $BACKEND_APP..." -ForegroundColor Yellow
    Push-Location "$BACKEND_DIR\publish\api"

    # Crear ZIP con solo binarios (excluir .pdb para reducir tamaño)
    if (Test-Path "api-deploy.zip") { Remove-Item "api-deploy.zip" -Force }
    Get-ChildItem -Path '.' | Compress-Archive -DestinationPath 'api-deploy.zip' -Force

    # Desplegar
    Write-Host "  📤 Subiendo... (esto puede tomar 1-2 minutos)"
    & "$AZ_CLI" webapp deployment source config-zip `
        --resource-group $RESOURCE_GROUP `
        --name $BACKEND_APP `
        --src "api-deploy.zip"
    
    if ($LASTEXITCODE -ne 0) { throw "Error desplegando Backend" }
    Write-Host "✓ Backend desplegado" -ForegroundColor Green
    Pop-Location

    # Esperar a que el backend esté listo
    Write-Host "⏳ Esperando reinicio del backend..." -ForegroundColor Cyan
    Start-Sleep -Seconds 45

    # ============================================================
    # 4. Desplegar Frontend (solo dist)
    # ============================================================
    Write-Host ""
    Write-Host "[4/4] Desplegando Frontend a $FRONTEND_APP..." -ForegroundColor Yellow
    Push-Location "$FRONTEND_DIR\dist"

    # Crear ZIP con solo archivos compilados
    if (Test-Path "frontend-deploy.zip") { Remove-Item "frontend-deploy.zip" -Force }
    Get-ChildItem -Path '.' | Compress-Archive -DestinationPath 'frontend-deploy.zip' -Force

    # Desplegar
    Write-Host "  📤 Subiendo... (esto puede tomar 1-2 minutos)"
    & "$AZ_CLI" webapp deployment source config-zip `
        --resource-group $RESOURCE_GROUP `
        --name $FRONTEND_APP `
        --src "frontend-deploy.zip"
    
    if ($LASTEXITCODE -ne 0) { throw "Error desplegando Frontend" }
    Write-Host "✓ Frontend desplegado" -ForegroundColor Green
    Pop-Location

    # ============================================================
    # 5. Resumen
    # ============================================================
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "✅ Despliegue completado exitosamente" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "URLs de acceso:" -ForegroundColor Cyan
    Write-Host "  Backend:  https://$BACKEND_APP.azurewebsites.net" -ForegroundColor Yellow
    Write-Host "  Frontend: https://$FRONTEND_APP.azurewebsites.net" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Swagger (para probar API):" -ForegroundColor Cyan
    Write-Host "  https://$BACKEND_APP.azurewebsites.net/swagger" -ForegroundColor Yellow
    Write-Host ""

} catch {
    Write-Host ""
    Write-Host "❌ Error: $_" -ForegroundColor Red
    Write-Host ""
    exit 1
}
