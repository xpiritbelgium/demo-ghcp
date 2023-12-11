# Setting up IaC in Azure

## Prerequisite 
- [Download Azure cli](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-windows?tabs=azure-cli) or use the shell in the Azure portal

## Creating a service principal
```bash
GH_ORGANIZATION=koenluyten #replace with your GitHub organization
GH_REPOSITORY=poc-azure-cleanarchitecture #replace with your GitHub repository
GH_ENVIRONMENT=DEV #replace with your GitHub Environment name

#Get your Azure subscription id and tenant id 
SP_SUBSCRIPTIONID=$(az account show --query id --output tsv)
SP_TENANTID=$(az account show --query tenantId --output tsv)

#Set the principal name to be created
SP_PRINCIPPALNAME=GITHUBACTIONS_$GH_ENVIRONMENT
 
#create the service principal
SP_APPID=$(az ad sp create-for-rbac --name $SP_PRINCIPPALNAME --role Contributor --scopes /subscriptions/$SP_SUBSCRIPTIONID --query appId --output tsv)
 
#get the objectid of the service principal, needed for the creation of the federated credential
SP_OBJECTID=$(az ad app show --id $SP_APPID --query id --output tsv)
 
#create the federated credential
az ad app federated-credential create --id $SP_OBJECTID --parameters "{\"name\":\"GH$GH_ENVIRONMENT\",\"issuer\":\"
https://token.actions.githubusercontent.com\",\"subject\":\"repo:$GH_ORGANIZATION/$GH_REPOSITORY:environment:$GH_ENVIRONMENT\",\"description\":\"credential
for $GH_ENVIRONMENT deployment\",\"audiences\":[\"api://AzureADTokenExchange\"]}"
 
#print the values needed to add as secrets into github
printf "AZURE_CLIENT_ID: $SP_APPID\nAZURE_TENANT_ID: $SP_TENANTID\nAZURE_SUBSCRIPTION_ID: $SP_SUBSCRIPTIONID\n"
```

## Creating the AZURE_CREDENTIALS secret in Github
1. Go to your repo in GitHub
1. Click on Environments and create a new environment called "DEV"
1. Add the AZURE_CLIENT_ID, AZURE_TENANT_ID and AZURE_SUBSCRIPTION_ID as secrets to the environment