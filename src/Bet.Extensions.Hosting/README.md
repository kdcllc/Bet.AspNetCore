# Bet.Extensions.Hosting

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.Hosting.svg)](https://www.nuget.org/packages?q=Bet.Extensions.Hosting)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.Extensions.Hosting/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.Extensions.Hosting/latest/download)

The collection of the IHost related functionality used with GenericHost.

Pre-release packages are distributed via feedz.io `https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json`.

## Configuration Validations for `IHost`

```bash
    dotnet add package Bet.Extensions.Hosting
    dotnet add package Bet.Extensions.Options
```

[Usage Bet.Extensions.Options](../../src/Bet.Extensions.Options/README.md).

To enable validation use `UseOptionValidation` in `Program.cs`

```csharp
        var host = new HostBuilder()
                .UseOptionValidation().Build();
```

Usage of `ConfigureWithDataAnnotationsValidation` or `ConfigureWithValidation` the same as in `IWebHost`

## `IHostStartupFilter`

To enable registration of other services on the start up use `UseStartupFilters` in `Program.cs`

By implementing and registering this interface with DI it is possible to trigger startup jobs for `IHost`.

## `ITimedHostedService`

The simple implementation of the service that must be run at an interval specified.

1. Add `MyHostedService` to the DI.

```csharp
    services.AddTimedHostedService(
        "MachineLearningService",
        options =>
        {
            options.Interval = TimeSpan.FromMinutes(30);

            options.FailMode = FailMode.LogAndRetry;
            options.RetryInterval = TimeSpan.FromSeconds(30);

            options.TaskToExecuteAsync = async (sp, cancellationToken) =>
            {
                var job = sp.GetRequiredService<IModelCreationService>();
                var logger = sp.GetRequiredService<ILogger<TimedHostedService>>();

                try
                {
                    await job.BuildModelsAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError("{serviceName} failed with exception: {message}", nameof(TimedHostedService), ex.Message);
                }
            };
        });
```
