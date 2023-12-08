param resourceGroupName string
param location string = 'westeurope'

targetScope = 'subscription'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: resourceGroupName
  location: location
}
