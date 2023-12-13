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

#create role assignment for role based access control admin
az role assignment create --role "Role Based Access Control Administrator" --scope /subscriptions/$SP_SUBSCRIPTIONID --assignee-object-id $SP_OBJECTID --assignee-principal-type ServicePrincipal --description "Role assignment to allow GH Actions to do role assignments of service principals"

#create AD group to set sql admins
SP_SQLADMINGROUP_NAME="cbn sql admins"
SP_SQLADMINGROUP_ID=$(az ad group create --display-name "$SP_SQLADMINGROUP_NAME" --mail-nickname "SP_SQLADMINGROUP_NAME" --description "Used to set sql admins" --query id --output tsv)

#add service principal to the sql admins group
az ad group member add --group "cbn sql admins" --member-id $SP_OBJECTID

#create custom role communcation service mail sender
az role definition create --role-definition "{\"Name\": \"Communication Service Mail Sender CBN\", \"IsCustom\": true, \"Description\": \"Minimal set of permissions required to send mail with Azure Communication Service.\", \"Actions\":[\"Microsoft.Communication/CommunicationServices/Read\",\"Microsoft.Communication/CommunicationServices/Write\",\"Microsoft.Communication/EmailServices/read\"],\"NotActions\": [], \"AssignableScopes\": [\"/subscriptions/$SP_SUBSCRIPTIONID\"]}"
 
#print the values needed to add as secrets into github
printf "AZURE_CLIENT_ID: $SP_APPID\nAZURE_TENANT_ID: $SP_TENANTID\nAZURE_SUBSCRIPTION_ID: $SP_SUBSCRIPTIONID\nSP_SQLADMINGROUP_NAME: $SP_SQLADMINGROUP_NAME\nSP_SQLADMINGROUP_ID: $SP_SQLADMINGROUP_ID\n"
```

## Creating the AZURE_CREDENTIALS secret in Github
1. Go to your repo in GitHub
1. Click on Environments and create a new environment called "DEV"
1. Add the AZURE_CLIENT_ID, AZURE_TENANT_ID and AZURE_SUBSCRIPTION_ID as secrets to the environment