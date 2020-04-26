# Let's Encrypt Web Api sample

This AspNetCore project is an example usage for `Bet.AspNetCore.LetsEncrypt` with Azure Container Instances.

This sample project is hosted in the Azure Container Services and accessible:

http://betacme.kingdavidconsulting.com/weatherforecast

https://betacme.kingdavidconsulting.com/weatherforecast

The required Azure Compoenent

- Azure Blob Storage - Used to store Acme Account and Certificate
- Azure Key Vault - Used to store secrets
- [Managed Identity](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/how-to-manage-ua-identity-cli) is used to access resources from Azure Container Services


## Azure Container Services Deployment Sample Template

```yaml
#------------------------------------------------------------------------------------------------------------------
#
# SOURCE: https://docs.microsoft.com/en-gb/azure/container-instances/container-instances-multi-container-yaml
#
# DEPLOY: az container create --resource-group {resourceGroupName} --file acme-deploy.yml
#
# VIEW DEPLOY STATE:  az container show --resource-group {resourceGroupName} --name betAcmeContainerGroup --output table
#
# DELETE DEPLOYMENT:  az container delete --resource-group {resourceGroupName} --name betAcmeContainerGroup
#------------------------------------------------------------------------------------------------------------------
apiVersion: 2018-10-01
location: centralus
name: betAcmeContainerGroup
properties:
  containers:
  - name: bet-acme
    properties:
      environmentVariables: []
      image: kdcllc/bet:letsencrypt
      resources:
        requests:
          cpu: 1
          memoryInGb: 0.2
      ports:
      - port: 80
      - port: 443
      environmentVariables:
      - name: ASPNETCORE_URLS
        value: "http://+:80;https://+:443"
      - name: ASPNETCORE_ENVIRONMENT
        value: "Production"
  osType: Linux

  ipAddress:
    type: Public
    ports:
    - protocol: tcp
      port: '80'
    - protocol: tcp
      port: '443'
    dnsNameLabel: betacme

identity:
    type: UserAssigned
    userAssignedIdentities:
      {'/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/{managedIdentityName}':{}}
tags: null

type: Microsoft.ContainerInstance/containerGroups


```
