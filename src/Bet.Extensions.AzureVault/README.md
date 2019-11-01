# Bet.Extensions.AzureVault

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.AzureVault.svg)](https://www.nuget.org/packages?q=Bet.Extensions.AzureVault)
[![MyGet](https://img.shields.io/myget/kdcllc/v/Bet.Extensions.AzureVault.svg?label=myget)](https://www.myget.org/F/kdcllc/api/v2)

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

## DotNetCore 3.0 Reload

Next version of the Azure Key Vault Configuration provider supports reloading interval. This is important for configurations that must be updated.

```csharp
     Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostingContext, configBuilder) =>
        {
            // based on environment Development = dev; Production = prod prefix in Azure Vault.
            var envName = hostingContext.HostingEnvironment.EnvironmentName;
            var configuration = configBuilder.AddAzureKeyVault(
                hostingEnviromentName: envName,
                usePrefix: false,
                reloadInterval: TimeSpan.FromSeconds(10));

            // helpful to see what was retrieved from all of the configuration providers.
            if (hostingContext.HostingEnvironment.IsDevelopment())
            {
                configuration.DebugConfigurations();
            }
        })
        .ConfigureServices((hostContext, services) =>
        {
            services.AddOptions<SampleOptions>().Bind(hostContext.Configuration.GetSection("Sample"));

            services.AddHostedService<Worker>();
        });
```

Then

```csharp

    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private SampleOptions _options;

        public Worker(ILogger<Worker> logger, IOptionsMonitor<SampleOptions> options)
        {
            _logger = logger;

            _options = options.CurrentValue;

            options.OnChange((opt) =>
            {
                _options = opt;
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time} - {name}", DateTimeOffset.Now, _options.Name);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
````
