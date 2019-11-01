# Bet.Extensions.HealthChecks.ML

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.HealthChecks.ML.svg)](https://www.nuget.org/packages?q=Bet.Extensions.HealthChecks.ML)
[![MyGet](https://img.shields.io/myget/kdcllc/v/Bet.Extensions.HealthChecks.ML.svg?label=myget)](https://www.myget.org/F/kdcllc/api/v2)

The healthcheck for Machine Learning model that is loaded into the Application Context.

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
