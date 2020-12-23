# Bet.Extensions.HealthChecks.ML

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.AspNetCore/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.HealthChecks.ML.svg)](https://www.nuget.org/packages?q=Bet.Extensions.HealthChecks.ML)
![Nuget](https://img.shields.io/nuget/dt/Bet.Extensions.HealthChecks.ML)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.Extensions.HealthChecks.ML/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.Extensions.HealthChecks.ML/latest/download)

> The second letter in the Hebrew alphabet is the ב bet/beit. Its meaning is "house". In the ancient pictographic Hebrew it was a symbol resembling a tent on a landscape.

*Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json).*

## Summary

This goal of this repo is to create ML.NET `HealChecks` for Application Context. This is useful feature that makes the state of the Models to be checked.

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Install

```csharp
    dotnet add package Bet.Extensions.HealthChecks.ML
```

## Usage

For complete examples please refer to sample projects:

1. [`Bet.AspNetCore.Sample`](../Bet.AspNetCore.Sample/) - `AspNetCore` Web App with spam prediction models.
2. [`Bet.Hosting.Sample`](../Bet.Hosting.Sample/) - `DotNetCore` Worker based scheduled job for generating ML.NET Models.

In order for the healtheck to work please add `HealthCheck` configuration in `ConfigureServices`.

```csharp
  services.AddHealthChecks()
                .AddMemoryHealthCheck()
                .AddMachineLearningModelCheck<SpamInput, SpamPrediction>("spam_check")
                .AddMachineLearningModelCheck<SentimentIssue, SentimentPrediction>("sentiment_check")
                .AddSigtermCheck("sigterm_check")
                .AddLoggerPublisher(new List<string> { "sigterm_check" });
```
