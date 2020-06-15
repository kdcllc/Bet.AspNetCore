# Bet.AspNetCore.Logging

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.AspNetCore/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.AspNetCore.Logging.svg)](https://www.nuget.org/packages?q=Bet.AspNetCore.Logging)
![Nuget](https://img.shields.io/nuget/dt/Bet.AspNetCore.Logging)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.AspNetCore.Logging/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.AspNetCore.Logging/latest/download)

*Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json).*

The collection of AspNetCore functionality for logging with Azure AppInsight and Azure Log Analyzer extends `Serilog`.

## Install

```csharp
    dotnet add package Bet.AspNetCore.Logging
```

## Usage

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
