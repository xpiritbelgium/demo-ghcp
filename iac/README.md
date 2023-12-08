# Setting up IaC in Azure

## Prerequisite 
- [Download Azure cli](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-windows?tabs=azure-cli) or use the shell in the Azure portal

## Creating a service principal
1. Log in into Azure using the az cli
    ```bash
    az login
    ```
1. Get the subscription id
    ```bash
    subscriptionid=$(az account show --query id --output tsv)
    ```
1. Create the service principal
    ```bash
    #set the name for the principal
    principalname=GitHubActions_Dev

    az ad sp create-for-rbac --name $principalname --role Contributor --scopes /subscriptions/$subscriptionid --sdk-auth

    #Copy the json string
    ```

## Creating the AZURE_CREDENTIALS secret in Github
1. Go to your repo in GitHub
1. Click on Environments and create a new environment called "DEV"
1. Add an environment secret called "AZURE_CREDENTIALS" and past the json string from the bash command