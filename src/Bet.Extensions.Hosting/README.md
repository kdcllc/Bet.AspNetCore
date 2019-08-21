# Bet.Extensions.Hosting

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.Hosting.svg)](https://www.nuget.org/packages?q=Bet.Extensions.Hosting)

The collection of the IHost related functionality used with GenericHost.

## Configuration Validations for `IHost`

```bash
    dotnet add package Bet.Extensions.Hosting
    dotnet add package Bet.Extensions.Options
```

[Usage Bet.Extensions.Options](./src/Bet.Extensions.Options/README.md).

To enable validation use `UseStartupFilter` in `Program.cs`

```csharp
        var host = new HostBuilder()
                .UseStartupFilter().Build();
```

Usage of `ConfigureWithDataAnnotationsValidation` or `ConfigureWithValidation` the same as in `IWebHost`

## `IHostStartupFilter`

By implementing and registering this interface with DI it is possible to trigger startup jobs for `IHost`.

## `ITimedHostedService`

The simple implementation of the service that must be run at an interval specified.

1. Add `MyHostedService` the hosted service.

```csharp
    public class MyHostedService : TimedHostedService
    {
        public ModelBuilderHostedService(
            IOptionsMonitor<TimedHostedServiceOptions> options,
            IEnumerable<ITimedHostedLifeCycleHook> lifeCycleHooks,
            ILogger<ITimedHostedService> logger) : base(options, lifeCycleHooks, logger)
        {
            TaskToExecuteAsync = (token) => RunAsync(token);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            Logger.Information("RunAsync at {timestamp}", DateTime.Now);
        }
    }
```

2. Add `MyHostedService` to the DI.

```csharp
     services.AddTimedHostedService<MyHostedService>(options =>
    {
        options.Interval = TimeSpan.FromMinutes(30);

        options.FailMode = FailMode.LogAndRetry;
        options.RetryInterval = TimeSpan.FromSeconds(30);
    });
```
