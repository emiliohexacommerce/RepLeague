@description('The location for all resources')
param location string = resourceGroup().location

@description('Environment name (dev, staging, prod)')
param environment string = 'prod'

@description('Project name - used to derive resource names')
param projectName string = 'RepLeague'

@description('App Service Plan SKU tier')
@allowed([
  'B1'
  'B2'
  'B3'
  'S1'
  'S2'
  'S3'
])
param appServicePlanSku string = 'B2'

output location string = location
output environment string = environment
output projectName string = projectName
output appServicePlanSku string = appServicePlanSku
