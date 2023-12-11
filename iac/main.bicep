param location string = resourceGroup().location
param sku string = 'S1'

resource appServicePlan 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: 'asp_cbn_dev'
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
  name: 'app_cbn_dev'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      netFrameworkVersion: 'v6.0'
    }
  }
}
