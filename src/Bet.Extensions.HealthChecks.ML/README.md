# Bet.Extensions.HealthChecks.ML

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.HealthChecks.ML.svg)](https://www.nuget.org/packages?q=Bet.Extensions.HealthChecks.ML)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.Extensions.HealthChecks.ML/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.Extensions.HealthChecks.ML/latest/download)

The healthcheck for Machine Learning model that is loaded into the Application Context.

Pre-release packages are distributed via feedz.io `https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json`.

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
