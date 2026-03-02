#!/bin/bash

# ============================================================
# Script de Despliegue - RepLeague a Web Apps Existentes
# ============================================================

# Configuración
RESOURCE_GROUP="DISCENTRO"
BACKEND_APP="api-repleague"
FRONTEND_APP="replague"
BACKEND_DIR="./backend"
FRONTEND_DIR="./frontend/replague"

echo ""
echo "========================================"
echo "RepLeague - Despliegue a Azure Web Apps"
echo "========================================"
echo "Resource Group: $RESOURCE_GROUP"
echo "Backend App:    $BACKEND_APP"
echo "Frontend App:   $FRONTEND_APP"
echo ""

# ============================================================
# 1. Compilar Backend
# ============================================================
echo "[1/4] Compilando Backend (.NET)..."
cd "$BACKEND_DIR"
dotnet publish -c Release -o ./publish/api
if [ $? -ne 0 ]; then
    echo "❌ Error compilando Backend"
    exit 1
fi
echo "✓ Backend compilado"
cd ../../

# ============================================================
# 2. Compilar Frontend
# ============================================================
echo ""
echo "[2/4] Compilando Frontend (Angular)..."
cd "$FRONTEND_DIR"
npm ci
if [ $? -ne 0 ]; then
    echo "❌ Error en npm ci del Frontend"
    exit 1
fi
npm run build -- --configuration production --base-href /
if [ $? -ne 0 ]; then
    echo "❌ Error compilando Frontend"
    exit 1
fi
echo "✓ Frontend compilado"
cd ../../..

# ============================================================
# 3. Desplegar Backend
# ============================================================
echo ""
echo "[3/4] Desplegando Backend a $BACKEND_APP..."
cd "$BACKEND_DIR/publish/api"

# Crear ZIP para despliegue
rm -f ../api-deploy.zip
cd ..
zip -r api-deploy.zip api/
cd ../..

# Desplegar
az webapp deployment source config-zip --resource-group "$RESOURCE_GROUP" --name "$BACKEND_APP" --src "$BACKEND_DIR/publish/api-deploy.zip"
if [ $? -ne 0 ]; then
    echo "❌ Error desplegando Backend"
    exit 1
fi
echo "✓ Backend desplegado"
cd ../../

# ============================================================
# 4. Desplegar Frontend
# ============================================================
echo ""
echo "[4/4] Desplegando Frontend a $FRONTEND_APP..."
cd "$FRONTEND_DIR/dist"

# Crear ZIP para despliegue
cd ..
rm -f frontend-deploy.zip
zip -r frontend-deploy.zip dist/
cd ../..

# Desplegar
az webapp deployment source config-zip --resource-group "$RESOURCE_GROUP" --name "$FRONTEND_APP" --src "$FRONTEND_DIR/frontend-deploy.zip"
if [ $? -ne 0 ]; then
    echo "❌ Error desplegando Frontend"
    exit 1
fi
echo "✓ Frontend desplegado"
cd ../

# ============================================================
# 5. URLs de Acceso
# ============================================================
echo ""
echo "========================================"
echo "✅ Despliegue completado exitosamente"
echo "========================================"
echo ""
echo "URLs de acceso:"
echo "  Backend:  https://$BACKEND_APP.azurewebsites.net"
echo "  Frontend: https://$FRONTEND_APP.azurewebsites.net"
echo ""
echo "Swagger (para probar API):"
echo "  https://$BACKEND_APP.azurewebsites.net/swagger"
echo ""
