param location string = resourceGroup().location
param sku string = 'S1'

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
