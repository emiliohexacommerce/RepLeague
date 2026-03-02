# Guía de Despliegue de RepLeague en Azure

## Estado del Despliegue
- ✅ Plan de despliegue creado (`.azure/plan.copilotmd`)
- ✅ Infraestructura as Code generada (Bicep)
- ✅ Configuración de Azure Developer CLI (AZD)
- ✅ Scripts de build y pre-deployment
- ⏳ Falta: Validación y Despliegue Actual

---

## Paso 1: Instalación de Herramientas Requeridas

Antes de desplegar, asegúrate de tener instaladas estas herramientas:

### Windows
```powershell
# Instalar Azure CLI
winget install Microsoft.AzureCLI

# Instalar Azure Developer CLI (azd)
winget install microsoft.azd

# Verificar instalaciones
az --version
azd --version
```

### macOS
```bash
# Instalar Azure CLI
brew install azure-cli

# Instalar Azure Developer CLI
brew install azd

# Verificar instalaciones
az --version
azd --version
```

### Linux
```bash
# Seguir guía oficial en:
# https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-linux
# https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd
```

---

## Paso 2: Autenticación con Azure

```powershell
# Iniciar sesión en Azure
az login

# Configurar suscripción por defecto (si tienes múltiples)
az account set --subscription "TU_ID_SUSCRIPCION"

# Verificar que estás autenticado
az account show
```

---

## Paso 3: Preparación Pre-Despliegue

Ejecuta el script de pre-despliegue para instalar dependencias:

### Windows
```powershell
.\pre-deploy.bat
```

### macOS/Linux
```bash
chmod +x pre-deploy.sh
./pre-deploy.sh
```

---

## Paso 4: Inicializar AZD (Primera vez)

```powershell
# Inicializar el proyecto AZD
azd init

# Cuando te pida valores, usa:
# - Environment name: prod (o dev/staging)
# - Location: eastus (o tu región preferida)
```

---

## Paso 5: Compilar la Aplicación

Compila el backend y frontend antes del despliegue:

### Windows
```powershell
.\build.bat
```

### macOS/Linux
```bash
chmod +x build.sh
./build.sh
```

**Resultado esperado:**
- Backend compilado en: `backend/publish/api`
- Frontend compilado en: `backend/publish/www`

---

## Paso 6: Generar Parámetros de Despliegue

Crea un archivo de parámetros con tus valores específicos:

```powershell
# Crear archivo infra/parameters.bicep.json
@"
{
  "location": {
    "value": "eastus"
  },
  "environment": {
    "value": "prod"
  },
  "projectName": {
    "value": "RepLeague"
  },
  "appServicePlanSku": {
    "value": "S1"
  },
  "sqlAdminUsername": {
    "value": "repleagueadmin"
  },
  "sqlAdminPassword": {
    "value": "TuContraseñaSegura123!@#"
  },
  "storageAccountName": {
    "value": "replague$(Get-Random -Minimum 100000 -Maximum 999999)"
  }
}
"@ | Out-File -FilePath infra/parameters.bicep.json
```

---

## Paso 7: Pre-Validación (Importante)

Antes de desplegar, valida los archivos Bicep:

```powershell
# Validar sintaxis de Bicep
az bicep build --file infra/main.bicep

# Validar plantilla con Azure
az deployment group validate `
  --resource-group "rg-RepLeague" `
  --template-file infra/main.bicep `
  --parameters infra/parameters.bicep.json
```

---

## Paso 8: Despliegue con AZD Up

Este comando provisiona la infraestructura Y despliega la aplicación:

```powershell
# Despliegue completo
azd up

# Cuando se te pida:
# - Selecciona tu suscripción Azure
# - Selecciona/crea un grupo de recursos
# - Confirma la creación de recursos
```

**Este proceso puede tardar 10-20 minutos.**

---

## Paso 9: Verificación Post-Despliegue

Después del despliegue exitoso, verifica que todo funciona:

```powershell
# Obtener outputs del despliegue
azd env get-values

# Verificar recursos creados
az resource list --resource-group "rg-RepLeague"

# Consultar URL de la aplicación
az webapp show \
  --resource-group "rg-RepLeague" \
  --name "RepLeague-prod-app" \
  --query "defaultHostName"
```

---

## Paso 10: Configurar Secretos en Key Vault

Después del despliegue, configura los secretos necesarios en Key Vault:

```powershell
# Configurar secretos
$kvName = "repleague-prod-kv"

# Conexión a Base de Datos SQL
az keyvault secret set --vault-name $kvName `
  --name "ConnectionStrings--DefaultConnection" `
  --value "Server=tcp:YOUR_SQL_SERVER.database.windows.net,1433;Initial Catalog=RepLeagueDB;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Default;"

# Clave de Almacenamiento
az keyvault secret set --vault-name $kvName `
  --name "AzureStorage--ConnectionString" `
  --value "DefaultEndpointsProtocol=https;AccountName=YOUR_STORAGE;AccountKey=YOUR_KEY;EndpointSuffix=core.windows.net"

# Clave JWT
az keyvault secret set --vault-name $kvName `
  --name "JWT-SecretKey" `
  --value "TuClaveSecreSuperSegura32CaractereMinimoOK"

# Clave SendGrid
az keyvault secret set --vault-name $kvName `
  --name "SendGrid--ApiKey" `
  --value "SG.XXXXXXXXX"
```

---

## Paso 11: Verificar Logs

Revisa los logs de la aplicación para asegurar que no hay errores:

```powershell
# Ver logs de la aplicación
az webapp log tail --resource-group "rg-RepLeague" --name "RepLeague-prod-app"

# Ver logs en Application Insights (Azure Portal)
# Portal -> Resource Group -> Application Insights -> Logs
```

---

## Paso 12: Configurar Dominio Personalizado (Opcional)

Si tienes un dominio personalizado:

```powershell
# Agregar dominio personalizado
az webapp config hostname add \
  --resource-group "rg-RepLeague" \
  --webapp-name "RepLeague-prod-app" \
  --hostname "tudominio.com"

# Crear certificado SSL gestionado
az webapp config ssl bind \
  --resource-group "rg-RepLeague" \
  --name "RepLeague-prod-app" \
  --certificate-thumbprint "YOUR_CERT_THUMBPRINT"
```

---

## Troubleshooting

### Error: "Resource group not found"
```powershell
# Crear grupo de recursos manualmente
az group create --name "rg-RepLeague" --location "eastus"
```

### Error: "Bicep validation failed"
```powershell
# Verificar y arreglar sintaxis
az bicep build --file infra/main.bicep
```

### Error: "SQL connection failed"
- Verificar que Key Vault tiene los secretos correctos
- Verificar que Web App tiene permiso para acceder a Key Vault
- Verificar que SQL Server tiene firewall configurado

### Error: "Frontend not loading"
- Verificar que Angular build se completó correctamente
- Verificar que `wwwroot` contiene los archivos compilados
- Revisar web.config para reescribir URLs

---

## Monitoreo Continuo

Después del despliegue, monitorea tu aplicación:

```powershell
# Ver métricas en Application Insights
az monitor metrics list \
  --resource "/subscriptions/YOUR_SUB_ID/resourceGroups/rg-RepLeague/providers/Microsoft.Insights/components/RepLeague-prod-ai"

# Ver alertas
az monitor alert list --resource-group "rg-RepLeague"
```

---

## Siguientes Pasos

1. ✅ Verificar que la aplicación está accesible
2. ✅ Probar las API endpoints
3. ✅ Probar el formulario de login
4. ✅ Verificar uploads de archivos
5. ✅ Revisar logs de errores
6. 📝 Configurar CI/CD con GitHub Actions (opcional)
7. 📝 Configurar backups de Base de Datos (importante)
8. 📝 Configurar escalado automático (si es necesario)

---

## Recursos Útiles

- [Azure Developer CLI Docs](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/)
- [Bicep Language Reference](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/file)
- [Azure SQL Security Best Practices](https://learn.microsoft.com/en-us/azure/azure-sql/database/security-best-practices)
- [App Service Documentation](https://learn.microsoft.com/en-us/azure/app-service/)

---

**Última actualización**: 2 de marzo de 2026
**Versión**: 1.0
