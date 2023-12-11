param resourceGroupName string ='rg_cleanarchitecture_dev'
param location string = 'westeurope'

targetScope = 'subscription'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: resourceGroupName
  location: location
}
