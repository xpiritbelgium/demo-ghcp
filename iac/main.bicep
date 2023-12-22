param location string = resourceGroup().location
param sku string = 'S1'
param environment string = 'dev'
@secure()
param sqlpassword string
// param sqladminid string
// param sqladminname string
param sqlDomain string
param storageDomain string
param communciationservicemailsenderroleid string

var environment_lower = toLower(environment)

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-12-01-preview' = {
  name: 'law-cbn-${environment_lower}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 90
    workspaceCapping: {
      dailyQuotaGb: 1
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-cbn-${environment_lower}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'    
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: 'asp-cbn-${environment_lower}'
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
  name: 'app-cbn-${environment_lower}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|6.0'
      alwaysOn: true
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'Blobs__ServiceUri'
          value: 'https://${storageaccount.name}.${storageDomain}/'
        }
        {
          name: 'Email__Endpoint'
          value: communicationService.properties.hostName
        }
        {
          name: 'Email__Sender'
          value: '${senderUserNameAzureDomain.properties.username}@${emailServiceAzureDomain.properties.fromSenderDomain}'
        }
      ]
      connectionStrings: [
        {
          name: 'CleanArchitectureConnectionsString'
          connectionString: 'Server=tcp:${sqlServer.name}.${sqlDomain},1433;Initial Catalog=${sqlServerDatabase.name};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication="Active Directory Default";'
          type: 'SQLAzure'
        }
      ]
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

output webAppIdentity string = webApp.name

resource storageaccount 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: 'stcbn${environment_lower}'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_ZRS'
  }
  properties:{
    allowBlobPublicAccess: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2019-06-01' = {
  name: 'default'
  parent: storageaccount
}

resource blobContainerinternal 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: 'sa-mgdid-test'
  parent: blobService
  properties: {
    publicAccess: 'None'
    metadata: {}
  }
}

resource blobContainerexternal 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: 'sa-mgdid-published-test'
  parent: blobService
  properties: {
    publicAccess: 'Blob'
    metadata: {}
  }
}

resource roleAssignment_storageaccountcontributor 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  scope: storageaccount
  name: guid(storageaccount.id, '17d1049b-9a84-46fb-8f53-869881c3d3ab', '${environment_lower}')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '17d1049b-9a84-46fb-8f53-869881c3d3ab')
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource roleAssignment_storageblobdatacontributor 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  scope: storageaccount
  name: guid(storageaccount.id, 'ba92f5b4-2d11-453d-a403-e96b0029c9fe', '${environment_lower}')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' ={
  name: 'sql-cbn-${environment_lower}'
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

// resource roleAssignment_sqlserverdirectoryreader 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
//   scope: tenant()
//   name: guid(sqlServer.id, '88d8e3e3-8f55-4a1e-953a-9b9898b8876b', '${environment_lower}')
//   properties:{
//     roleDefinitionId: tenantResourceId('Microsoft.Authorization/roleDefinitions', '88d8e3e3-8f55-4a1e-953a-9b9898b8876b')
//     principalId: sqlServer.identity.principalId
//     principalType: 'ServicePrincipal'
//   }
// }

// resource sqlServerAdministrator 'Microsoft.Sql/servers/administrators@2022-05-01-preview' = {
//   name: 'ActiveDirectory'
//   parent: sqlServer
//   properties: {
//     administratorType: 'ActiveDirectory'
//     login: sqladminname
//     sid: sqladminid
//     tenantId: tenant().tenantId
//   }
// }

resource sqlServerDatabase 'Microsoft.Sql/servers/databases@2023-02-01-preview' = {
  parent: sqlServer
  name: 'sqldb-cbn-${environment_lower}'
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

output sqlConnectionString string = 'Server=tcp:${sqlServer.name}.${sqlDomain},1433;Initial Catalog=${sqlServerDatabase.name};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication="Active Directory Default";'


resource emailService 'Microsoft.Communication/emailServices@2023-06-01-preview' = {
  name: 'email-cbn-${environment_lower}'
  location: 'global'
  properties: {
    dataLocation: 'Europe'
  }
}

resource emailServiceAzureDomain 'Microsoft.Communication/emailServices/domains@2023-03-31' = {
  parent: emailService
  name: 'AzureManagedDomain'
  location: 'global'
  properties: {
    domainManagement: 'AzureManaged'
    userEngagementTracking: 'Disabled'
  }
}

resource senderUserNameAzureDomain 'Microsoft.Communication/emailServices/domains/senderUsernames@2023-03-31' = {
  parent: emailServiceAzureDomain
  name: 'donotreply'
  properties: {
    username: 'DoNotReply'
    displayName: 'DoNotReply'
  }
}

output sendermail string = '${senderUserNameAzureDomain.properties.username}@${emailServiceAzureDomain.properties.fromSenderDomain}'

resource communicationService 'Microsoft.Communication/communicationServices@2023-06-01-preview' = {
  name: 'acs-cbn-${environment_lower}'
  location: 'global'
  properties: {
    dataLocation: 'Europe'
    linkedDomains: [
      emailServiceAzureDomain.id
    ]
  }
}

resource roleAssignment_communicationservicemailsender 'Microsoft.Authorization/roleAssignments@2020-10-01-preview' = {
  scope: communicationService
  name: guid(storageaccount.id, communciationservicemailsenderroleid, '${environment_lower}')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', communciationservicemailsenderroleid)
    principalId: webApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}
