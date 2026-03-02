@description('The location for all resources')
param location string = resourceGroup().location

@description('Environment name (dev, staging, prod)')
param environment string = 'prod'

@description('Project name')
param projectName string = 'RepLeague'

@description('App Service Plan SKU')
param appServicePlanSku string = 'B2'

var resourceNamePrefix = '${projectName}-${environment}'
var appServicePlanName = '${resourceNamePrefix}-plan'
var backendAppName = '${resourceNamePrefix}-backend'
var frontendAppName = '${resourceNamePrefix}-frontend'

// App Service Plan (compartido para backend y frontend)
module appServicePlan 'br/public:avm/res/web/serverfarms:0.4.0' = {
  name: 'appServicePlanDeployment'
  params: {
    name: appServicePlanName
    location: location
    skuName: appServicePlanSku
    kind: 'Windows'
    reserved: false
    tags: {
      environment: environment
      project: projectName
    }
  }
}

// Backend Web App (.NET 9 API)
module backendApp 'br/public:avm/res/web/sites:0.11.0' = {
  name: 'backendAppDeployment'
  params: {
    name: backendAppName
    location: location
    kind: 'app'
    serverFarmResourceId: appServicePlan.outputs.resourceId
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      use32BitWorkerProcess: false
      managedPipelineMode: 'Integrated'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environment == 'prod' ? 'Production' : 'Development'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
    }
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    corsSettings: {
      allowedOrigins: [
        '*'
      ]
    }
    tags: {
      environment: environment
      project: projectName
    }
  }
}

// Frontend Web App (Angular SPA - puede correr en .NET para servir estáticos)
module frontendApp 'br/public:avm/res/web/sites:0.11.0' = {
  name: 'frontendAppDeployment'
  params: {
    name: frontendAppName
    location: location
    kind: 'app'
    serverFarmResourceId: appServicePlan.outputs.resourceId
    siteConfig: {
      netFrameworkVersion: 'v4.7'
      use32BitWorkerProcess: false
      managedPipelineMode: 'Integrated'
      defaultDocuments: [
        'index.html'
        'default.html'
      ]
      appSettings: [
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
    }
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    corsSettings: {
      allowedOrigins: [
        '*'
      ]
    }
    tags: {
      environment: environment
      project: projectName
    }
  }
}

@description('Output the Backend Web App URL')
output backendUrl string = 'https://${backendApp.outputs.defaultHostname}'

@description('Output the Frontend Web App URL')
output frontendUrl string = 'https://${frontendApp.outputs.defaultHostname}'

@description('Output the App Service Plan name')
output appServicePlanName string = appServicePlan.outputs.name
