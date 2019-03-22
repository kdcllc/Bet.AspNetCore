# Bet.AspNetCore solution

This repo contains several projects that provide with extended functionality for `DotNetCore` framework. The projects are split into two category:

1. `Bet.Extensions` generic functionality for `DotNetCore` in general.
    - `Bet.Extensions.Options` - includes Application Options and `Bind()` validations.
    - `Bet.Extensions.Logging` - includes shared/common logging functionality.
    - `Bet.Extensions.Hosting` - includes Generic Hosting functionality.
    - `Bet.Extensions.AzureVault` - includes Azure Vault functionality.
    - `Bet.Extensions` - includes extensions methods for `DotNetCore`.

2. `Bet.AspNetCore` specific functionality for web applications.
    - `Bet.AspNetCore.HealthChecks` contains HealthChecks for most common scenarios of the web application.
    - `Bet.AspNetCore.Logging` contains logging functionality for `AspNetCore` applications.
    - `Bet.AspNetCore` - default location for `AspNetCore`.

## AppAuthenticaion CLI tool

Azure Vault is a great utility but what if your company doesn't allow you to have secret and client id. What if you want to use Docker Containers
on your local machine and be able to authenticate your Azure Vault. This utility provides this functionality.

[AppAuthenticaion CLI tool Readme](./src/AppAuthentication/Readme.md)

## Configuration Validation

`AspNetCore` has very flexible Configuration providers framework. Application can load Configurations from various locations. The last provider overrides the values of the same kind of keys.
Since it has such flexibility was introduces the validation of the Configurations was missing until `2.2`. `AsptNetCore` 2.2 release introduced extension methods on `OptionsBuilder` which if registered will be validated on the first use of the Options, but there are cases where application needs to terminate at startup if the required configurations are missing. This is where the collection of the libraries from this solution come in.

### Configuration Validations for `IWebHost`

```bash
    dotnet add package Bet.AspNetCore
    dotnet add package Bet.Extensions.Options
```

To enable validation add before any Configuration validation are added in `Startup.cs` within `ConfigureServices`

```csharp
    services.AddConfigurationValidation();
```

Then add validation for Configurations as action call back or as data annotations

```csharp
   services.ConfigureWithValidation<FakeOptions>(Configuration, opt =>
      {
          if (opt.Id > 0 && !string.IsNullOrWhiteSpace(opt.Name))
          {
              return true;
          }
          return false;
      }, "This didn't validated.");

   services.ConfigureWithDataAnnotationsValidation<FakeOptionsWithDataAnnotations>(configuration, sectionName: "FakeOptions");
```

### Configuration Validations for `IHost`

```bash
    dotnet add package Bet.Extensions.Hosting
    dotnet add package Bet.Extensions.Options
```

To enable validation use `UseStartupFilter` in `Program.cs`

```csharp
        var host = new HostBuilder()
                .UseStartupFilter().Build();
```

Usage of `ConfigureWithDataAnnotationsValidation` or `ConfigureWithValidation` the same as in `IWebHost`

### Bind Object with Validation

Simply add this package to your application and use `Bind` validation extensions methods

```bash
    dotnet add package Bet.Extensions.Options
```

```csharp
    var options = config.Bind<MyOptionsWithDatatAnnotations>(options);
```

## Azure Key Vault Configuration Provider

```csharp
    dotnet add package Bet.Extensions.AzureVault
```

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
   configBuilder.AddAzureKeyVault(hostingEnviromentName:hostContext.HostingEnvironment.EnvironmentName);
```

## Logging Configurations for debugging

Add the following to the project

```csharp
    dotnet add package Bet.Extensions.Logging
```

Sometimes we need to have tracing information enable to see where the configuration details are loaded from.

```csharp

    new HostBuilder()
          .ConfigureAppConfiguration((context, configBuilder) =>
          {
               configBuilder.Build().DebugConfigurations();
          }
```

## Logging Extension with Serilog

Add the following to the project

```csharp
    dotnet add package Bet.AspNetCore.Logging
```

To enable Azure `ApplicationInsights` and/or `LogAnalytics` Seriglog sinks add the following in `Program.cs`:

```csharp
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    loggerConfiguration
                          .ReadFrom.Configuration(hostingContext.Configuration)
                          .Enrich.FromLogContext()
                          .WriteTo.Console()
                          .AddApplicationInsights(hostingContext.Configuration)
                          .AddAzureLogAnalytics(hostingContext.Configuration);
                })
```

Make sure that the following Options values are supplied.

```json
  "ApplicationInsights": {
    "InstrumentationKey": "",
    "EnableEvents": true,
    "EnableTraces": true
  },

  "AzureLogAnalytics": {
    "WorkspaceId": "",
    "AuthenticationId": ""
  }
```
