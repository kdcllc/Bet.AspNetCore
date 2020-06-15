# Bet.Extensions.HealthChecks.AzureStorage

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.AspNetCore/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.HealthChecks.AzureStorage.svg)](https://www.nuget.org/packages?q=Bet.Extensions.HealthChecks.AzureStorage)
![Nuget](https://img.shields.io/nuget/dt/Bet.Extensions.HealthChecks.AzureStorage)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.Extensions.HealthChecks.AzureStorage/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.Extensions.HealthChecks.AzureStorage/latest/download)

*Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json).*

The healthcheck for Azure Storage.

## Install

```bash
    dotnet add package Bet.Extensions.HealthChecks.AzureStorage
```

## Usage

1. Add `HealthCheck` configuration in `ConfigureServices`.

```csharp
  services.AddHealthChecks()
                .AddMemoryHealthCheck()
                .AddMachineLearningModelCheck<SpamInput, SpamPrediction>("spam_check")
                .AddMachineLearningModelCheck<SentimentIssue, SentimentPrediction>("sentiment_check")
                .AddSigtermCheck("sigterm_check")
                .AddLoggerPublisher(new List<string> { "sigterm_check" });
```
