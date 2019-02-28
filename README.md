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

Since AspNetCore is very flexible with Configuration providers it allows to load options from different providers. It doesn't grantee the validations of the values out of the box.
AspNetCore 2.2 introduced extension methods on `OptionsBuilder` which if registered will be validated on the first use of the Options. 

This project include further ability to validate Options on the application startup before the values are ever used.

Options validation falls into two category:

1. Configuration Extension Method for the `Bind` method which is usually used to bind options with validation `Program.cs` file in `ConfigureAppConfiguration` in `IWebHostBuilder` or `IHostBuilder`.

2. `Configure` Extension method for `Startup.cs` file and `ConfigureServices` method named `ConfigureWithDataAnnotationsValidation` or `ConfigureWithValidation`.


### Usage Within the `Program.cs`

1. `Bind`  or `ConfigureWithDataAnnotationsValidation` or `ConfigureWithValidation` Method extension with a validation delegate

```csharp

   var options = config.Bind<MyOptions>(options, opt =>
        {
            if (opt.Id > 0 && !string.IsNullOrWhiteSpace(opt.Name))
            {
                return true;
            }
            return false;
        }, "Validation Failed");


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

When used with `HostBuilder` you have to register hosted service `services.AddHostedService<HostStartupService>();` or use an extension method.

```csharp
    var host = new HostBuilder()
                .UseStartupFilter()
                .Build();
```

2. `Bind<>` Method with DataAnnotations

```csharp
    var options = config.Bind<MyOptionsWithDatatAnnotations>(options);
```

## Azure Key Vault Configuration Provider

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
