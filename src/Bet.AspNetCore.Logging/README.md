# Bet.AspNetCore.Logging

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.AspNetCore.Logging.svg)](https://www.nuget.org/packages?q=Bet.AspNetCore.Logging)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.AspNetCore.Logging/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.AspNetCore.Logging/latest/download)

The collection of AspNetCore functionality for logging with Azure Appinsight and Azure Log Analyzer extends `Serilog`.

Pre-release packages are distributed via feedz.io `https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json`.

## Usage

Add the following to the project

```csharp
    dotnet add package Bet.AspNetCore.Logging
```

To enable Azure `ApplicationInsights` and/or `LogAnalytics` Seriglog sinks add the following in `Program.cs`:

```csharp
    .UseSerilog((hostingContext, loggerConfiguration) =>
    {
        var applicationName = $"myapp.-{hostingContext.HostingEnvironment.EnvironmentName}";
        loggerConfiguration
                .ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .AddApplicationInsights(hostingContext.Configuration)
                .AddAzureLogAnalytics(hostingContext.Configuration, applicationName: applicationName);
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
