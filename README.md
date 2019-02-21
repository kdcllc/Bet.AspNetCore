# Bet.AspNetCore Library

dotnThe commonly used AspNetCore functionality to be utilized across multiple projects.

Contains the following functionality:

- Azure Key Vault Configuration Provider.
- Options Validations Methods.

## Azure Key Valut Configuration Provider

This provider requires the following configurations to be present in any other configuration providers.

```json
"AzureVault": {
    "BaseUrl": "https://kdcllc.vault.azure.net/",
    "ClientId": "",
    "ClientSecret": ""
  }
```
At minimum this provider requires to have BaseUrl and will authenticated based on the Visual Studio.Net 
Credentials. For `Docker` Containers and other environments where Microsoft MSI can't be used provide `ClientId` and `ClientSecret`.

### Usage

```c#
   configBuilder.AddAzureKeyVault(hostingEnviromentName:hostContext.HostingEnvironment.EnvironmentName);
```
