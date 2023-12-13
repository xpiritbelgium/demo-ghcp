param location string = resourceGroup().location
param sku string = 'S1'
@secure()
param sqlpassword string
param sqladminid string
param sqladminname string

resource appServicePlan 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: 'asp-cbn-dev'
  location: location
  properties: {
    reserved: true
  }
  sku: {
    name: sku
  }
  kind: 'linux'
}

resource webApp 'Microsoft.Web/sites@2021-02-01' = {
  name: 'app-cbn-dev'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|6.0'
      alwaysOn: true
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

output webAppIdentity string = webApp.name

resource storageaccount 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: 'stcbndev'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_ZRS'
  }
}

resource roleAssignment_storageaccountcontributor 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  scope: storageaccount
  name: guid(storageaccount.id, '17d1049b-9a84-46fb-8f53-869881c3d3ab', 'dev')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '17d1049b-9a84-46fb-8f53-869881c3d3ab')
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource roleAssignment_storageblobdatacontributor 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  scope: storageaccount
  name: guid(storageaccount.id, 'ba92f5b4-2d11-453d-a403-e96b0029c9fe', 'dev')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' ={
  name: 'sql-cbn-dev'
  location: location
  properties: {
    administratorLogin: 'sqladmin'
    administratorLoginPassword: sqlpassword
    version: '12.0'
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Disabled'
  }
  identity: {
    type: 'SystemAssigned'    
  }
}

resource symbolicname 'Microsoft.Sql/servers/administrators@2022-05-01-preview' = {
  name: 'ActiveDirectory'
  parent: sqlServer
  properties: {
    administratorType: 'ActiveDirectory'
    login: sqladminname
    sid: sqladminid
    tenantId: tenant().tenantId
  }
}

resource sqlServerDatabase 'Microsoft.Sql/servers/databases@2023-02-01-preview' = {
  parent: sqlServer
  name: 'sqldb-cbn-dev'
  location: location
  sku: {
    name: 'GP_S_Gen5'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 1
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 34359738368
    catalogCollation: 'SQL_Latin1_General_CP1_CI_AS'
    zoneRedundant: true
    readScale: 'Disabled'
    autoPauseDelay: 60
    requestedBackupStorageRedundancy: 'Geo'
    isLedgerOn: false
  }
}
