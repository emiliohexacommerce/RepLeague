# ✅ Checklist Despliegue - RepLeague a Web Apps Existentes

**Configuración:**
- Resource Group: `DISCENTRO`
- Backend Web App: `api-repleague`
- Frontend Web App: `replague`

---

## Instalación (Primera Vez - 10 min)

### 1. Verificar Herramientas
```powershell
# Verificar instaladas
az --version          # Azure CLI
dotnet --version      # .NET 9.0
node --version        # Node.js 18+
npm --version         # npm
```

- [ ] Azure CLI instalado
- [ ] .NET 9.0 instalado
- [ ] Node.js 18+ instalado

### 2. Autenticarse con Azure
```powershell
az login
```

- [ ] Login exitoso
- [ ] Grupo `DISCENTRO` visible en Portal
- [ ] Web Apps existentes visibles

---

## Despliegue de Código (10-15 min)

### 3. Ejecutar Script de Despliegue

**Windows:**
```powershell
.\deploy.bat
```

**macOS/Linux:**
```bash
chmod +x deploy.sh
./deploy.sh
```

El script automáticamente:
1. ✓ Compila backend (.NET)
2. ✓ Compila frontend (Angular)
3. ✓ Despliega a `api-repleague`
4. ✓ Despliega a `replague`

**Esperado**: ~10-15 minutos

- [ ] Sin errores en compilación
- [ ] Ambos deployments exitosos
- [ ] Viste mensajes ✓

---

## Verificación Post-Despliegue (5 min)

### 4. Verificar URLs
```
[ ] Backend:  https://api-repleague.azurewebsites.net
[ ] Frontend: https://replague.azurewebsites.net
[ ] Swagger:  https://api-repleague.azurewebsites.net/swagger
```

### 5. Pruebas Rápidas
```
[ ] Frontend carga en https://replague.azurewebsites.net
[ ] Backend responde en /swagger
[ ] Logs sin errores críticos
[ ] Conectividad front-to-back OK
```

---

## Timings

```
INSTALACIÓN:     10 min
DESPLIEGUE:      10-15 min
VERIFICACIÓN:     5 min
─────────────────────────
TOTAL:           25-30 min
```

---

## Comandos Útiles

### Ver Logs
```powershell
# Backend
az webapp log tail --resource-group DISCENTRO --name api-repleague

# Frontend
az webapp log tail --resource-group DISCENTRO --name replague
```

### Reiniciar Web Apps
```powershell
# Backend
az webapp restart --resource-group DISCENTRO --name api-repleague

# Frontend
az webapp restart --resource-group DISCENTRO --name replague
```

### Ver Configuración
```powershell
az webapp config appsettings list --resource-group DISCENTRO --name api-repleague
az webapp config appsettings list --resource-group DISCENTRO --name replague
```

### Troubleshooting: Compilación
Si `deploy.bat` falla:
```powershell
# Limpiar compilaciones previas
rm -r .\backend\publish
rm -r .\frontend\replague\dist

# Reinstalar dependencias
cd .\frontend\replague
npm ci --legacy-peer-deps
cd ..\..\
```

---

## URLs de Acceso

| Servicio | URL |
|----------|-----|
| Frontend | https://replague.azurewebsites.net |
| Backend API | https://api-repleague.azurewebsites.net |
| Swagger | https://api-repleague.azurewebsites.net/swagger |
| Azure Portal | https://portal.azure.com |

---

✅ **¡Listo para desplegar!** Ejecuta `.\deploy.bat` (Windows) o `./deploy.sh` (Mac/Linux)
