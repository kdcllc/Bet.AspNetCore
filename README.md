# Bet.AspNetCore Common Library

This repo contains several projects that provide with extended functionality for AspNetCore platform.

- `Bet.AspNetCore` contains extensions to `Bind()` and `Configure()` methods. AspNetCore is configuration providers are very flexible but with that flexibility comes
the lack of validation. This project builds on AspNetCore 2.2 Validation and extends it.

- `Bet.AspNetCore.HealthChecks` contains HealthChecks for most common scenarios of the web application.

## Bet.AspNetCore 

This project contains the following functionality:

- Options Validations on startup of the application generic host or AspNetCore app.

- Azure Key Vault Configuration Provider.

## Options Validation

Bind options with validation are often used in `ConfigureAppConfiguration` in `IWebHostBuilder` or `IHostBuilder`. 
In some cases you want to prevent from application execution unless all of the required Options are supplied.

1. Bind Method extension with a validation delegate

```csharp

   var options = config.Bind<MyOptions>(options, opt =>
        {
            if (opt.Id > 0 && !string.IsNullOrWhiteSpace(opt.Name))
            {
                return true;
            }
            return false;
        }, "Validation Failed");
```

When used with `HostBuilder` you have to register hosted service `services.AddHostedService<HostStartupService>();`.

2. Bind<> Method with DataAnnotations

```csharp
    var options = config.Bind<MyOptionsWithDatatAnnotations>(options);
```

### Azure Key Vault Configuration Provider

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

Sometimes we need to have tracing information enable to see where the configuration details are loaded from.

```csharp

    new HostBuilder()
          .ConfigureAppConfiguration((context, configBuilder) =>
          {
               configBuilder.Build().DebugConfigurations();
          }
``` 

## Logging Extension with Serilog

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
