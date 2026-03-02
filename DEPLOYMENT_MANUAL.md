// Despliegue Manual - RepLeague a Azure Web Apps
// =====================================================

## ✅ Estado Actual
- ✓ Backend compilado: `backend/publish/api`
- ✓ Frontend compilado: `frontend/replague/dist`
- ✓ Web Apps creados: `api-repleague` y `replague` en DISCENTRO

## 📋 Opción 1: Despliegue Manual vía Portal de Azure

### Para Backend:
1. Ir a [Azure Portal](https://portal.azure.com)
2. Buscar recurso: `api-repleague`
3. Ir a **Deployment Center** → **Settings**
4. Seleccionar: **Manual deployment**
5. Ir a **App Service Editor** o **Kudu** (`https://api-repleague.scm.azurewebsites.net`)
6. En Kudu: Ir a **Debug console** → **CMD**
7. Navegar a: `site/wwwroot`
8. **Borrar contenido actual**
9. Subir archivo ZIP de `c:\Users\esala\source\RepLeague\backend\publish\api`
   - Dragging & drop en la consola Kudu
   - O usar FTP

### Para Frontend:
1. Mismo proceso con `replague` Web App
2. Subir ZIP de `c:\Users\esala\source\RepLeague\frontend\replague\dist`

---

## 🔧 Opción 2: Despliegue vía Azure CLI (Paso a paso)

```powershell
# Abre PowerShell y ejecuta los siguientes comandos uno por uno

# 1. Crear ZIP del Backend
cd c:\Users\esala\source\RepLeague\backend\publish\api
$BACKEND_ZIP = "C:\Users\esala\source\RepLeague\backend\api-deploy.zip"
if (Test-Path $BACKEND_ZIP) { Remove-Item $BACKEND_ZIP }
Compress-Archive -Path @("*.dll", "*.exe", "*.json", "web.config", "appsettings*.json", "email-templates", "runtimes") -DestinationPath $BACKEND_ZIP

# 2. Desplegar Backend
$az = "C:\Program Files\Microsoft SDKs\Azure\CLI2\wbin\az.cmd"
& $az webapp deployment source config-zip --resource-group DISCENTRO --name api-repleague --src $BACKEND_ZIP

# Esperar 5-10 minutos, verificar: https://api-repleague.azurewebsites.net/swagger

# 3. Crear ZIP del Frontend
cd c:\Users\esala\source\RepLeague\frontend\replague\dist
$FRONTEND_ZIP = "C:\Users\esala\source\RepLeague\frontend\frontend-deploy.zip"
if (Test-Path $FRONTEND_ZIP) { Remove-Item $FRONTEND_ZIP }
Compress-Archive -Path * -DestinationPath $FRONTEND_ZIP

# 4. Desplegar Frontend
& $az webapp deployment source config-zip --resource-group DISCENTRO --name replague --src $FRONTEND_ZIP

# Esperar 5-10 minutos, verificar: https://replague.azurewebsites.net
```

---

## 🚀 Opción 3: Usar GitHub Actions (Recomendado para futuro)

Crear `.github/workflows/deploy.yml` en tu repositorio:

```yaml
name: Deploy RepLeague

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0'
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '20'
      
      - name: Build Backend
        run: |
          cd backend
          dotnet publish -c Release -o ./publish/api
      
      - name: Build Frontend
        run: |
          cd frontend/replague
          npm install --legacy-peer-deps
          npm run build -- --configuration production --base-href /
      
      - name: Deploy to Azure
        uses: Azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      
      - name: Deploy Backend
        uses: Azure/webapps-deploy@v2
        with:
          app-name: api-repleague
          package: './backend/publish/api'
          resource-group-name: DISCENTRO
      
      - name: Deploy Frontend
        uses: Azure/webapps-deploy@v2
        with:
          app-name: replague
          package: './frontend/replague/dist'
          resource-group-name: DISCENTRO
```

---

## ✔️ Verificación Post-Despliegue

Después de desplegar, verifica:

```bash
# Backend
curl https://api-repleague.azurewebsites.net/swagger

# Frontend
curl https://replague.azurewebsites.net

# Ver logs (si hay errores)
az webapp log tail --resource-group DISCENTRO --name api-repleague
az webapp log tail --resource-group DISCENTRO --name replague
```

---

## 📞 Troubleshooting

| Problema | Solución |
|----------|----------|
| 502 Bad Gateway | Reinicia el Web App desde Portal |
| 404 Not Found | Verifica que los archivos se subieron correctamente en Kudu |
| CORS errors | Configura CORS en Program.cs del backend |
| Frontend no carga | Revisa web.config del frontend Web App |

---

## 🎯 Siguiente Paso

¿Cuál opción prefieres usar?
1. **Portal Azure (más simple, manual)**
2. **CLI (lo completamos ahora)**
3. **GitHub Actions (para futuro)**
