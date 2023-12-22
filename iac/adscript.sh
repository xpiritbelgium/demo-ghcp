# Create the Azure AD application.
application=$(az ad app create --display-name $AzureADApplicationName --app-roles ./roles.json --sign-in-audience AzureADMyOrg --web-redirect-uris "https://app-$AzureADApplicationName.azurewebsites.net/.auth/login/aad/callback")
applicationObjectId=$(jq -r '.id' <<< "$application")
applicationClientId=$(jq -r '.appId' <<< "$application")

# Create client secret
secret=$(az ad app credential reset --id $applicationObjectId --display-name $AzureADApplicationName)
secretPassword=$(jq -r '.id' <<< "$secret")

# Create a service principal for the application.
servicePrincipal=$(az ad sp create --id $applicationObjectId)
servicePrincipalObjectId=$(jq -r '.id' <<< "$servicePrincipal")

outputJson=$(jq --null-input \
  --arg applicationObjectId "$applicationObjectId" \
  --arg applicationClientId "$applicationClientId" \
  --arg servicePrincipalObjectId "$servicePrincipalObjectId" \
  --arg clientSecret "$secretPassword" \
  '{"applicationObjectId": $applicationObjectId, "applicationClientId": $applicationClientId, "servicePrincipalObjectId": $servicePrincipalObjectId, "secret": $clientSecret}')
echo $outputJson > $AZ_SCRIPTS_OUTPUT_PATH
