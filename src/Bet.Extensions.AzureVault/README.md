# Bet.Extensions.AzureVault

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.AzureVault.svg)](https://www.nuget.org/packages?q=Bet.Extensions.AzureVault)

Add the following to the project

```csharp
    dotnet add package Bet.Extensions.AzureVault
```

## Usage

This provider requires the following configurations to be present in any other configuration providers.
The `ClientId` and `ClientSecret` are not needed if used within Visual Studio.Net or in Azure Web Application with MSI enable.

```json
"AzureVault": {
    "BaseUrl": "https://kdcllc.vault.azure.net/",
    "ClientId": "",
    "ClientSecret": ""
  }
```

At minimum this provider requires to have BaseUrl and will authenticated based on the Visual Studio.Net 
Credentials. For `Docker` Containers and other environments where Microsoft MSI can't be used provide `ClientId` and `ClientSecret`.

In order to use Azure Key Vault register it with `IServiceCollection`.

```c#
     .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();

        webBuilder.ConfigureAppConfiguration((hostingContext, configBuilder) =>
        {
            // based on environment Development = dev; Production = prod prefix in Azure Vault.
            var envName = hostingContext.HostingEnvironment.EnvironmentName;

            var configuration = configBuilder.AddAzureKeyVault(hostingEnviromentName: envName, usePrefix: true);
        });
    });
```
