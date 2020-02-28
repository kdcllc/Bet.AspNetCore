# Bet.AspNetCore Libraries

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.AspNetCore.svg)](https://www.nuget.org/packages?q=Bet.AspNetCore)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.AspNetCore/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.AspNetCore/latest/download)

This repo contains several projects that provide with extended functionality for `DotNetCore` framework. The projects are split into two category:

Pre-release packages are distributed via feedz.io `https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json`.

## Machine Learning (ML.NET) functionality

1. [`Bet.Extensions.ML`](./src/Bet.Extensions.ML/) - includes Machine Learning library around [ML.NET](https://github.com/dotnet/machinelearning).
2. [`Bet.Extensions.ML.Sentiment`](./src/Bet.Extensions.ML.Sentiment/) - Sentiment self-contained prediction engine to be used with any DotNetCore applications.
3. [`Bet.Extensions.ML.Spam`](./src/Bet.Extensions.ML.Spam/) - Spam self-contained prediction engine to be used with any DotNetCore applications.
4. [`Bet.AspNetCore.Sample`](./src/Bet.AspNetCore.Sample/) - `AspNetCore` Web App with spam and sentiment prediction models.
5. [`Bet.Extensions.HealthChecks.ML`](./src/Bet.Extensions.HealthChecks.ML/) provides with HealthChecks ML.NET Models.
6. [`Bet.Extensions.ML.Azure`](./src/Bet.Extensions.ML.Azure/) provides with extensions methods for Azure Blob Storage Model monitoring and reload.

## DotNet CLI global tools

1. [AppAuthentication](./src/AppAuthentication/) - enables Azure Vault access from Docker Container in Local development.

## Generic functionally

1. [`Bet.Extensions`](./src/Bet.Extensions/) extends many DotNetCore classes.
2. [`Bet.Extensions.Options`](./src/Bet.Extensions.Options/) - extends Options with common functionality such as `Bind()` validations.
3. [`Bet.Extensions.Logging`](./src/Bet.Extensions.Logging/) - includes shared/common logging functionality.
4. [`Bet.Extensions.Hosting`](./src/Bet.Extensions.Hosting/) - extends generic functionality for `IHost`.
5. [`Bet.Extensions.AzureVault`](./src/Bet.Extensions.AzureVault/) - includes Azure Vault functionality.
6. [`Bet.Extensions.AzureStorage`](./src/Bet.Extensions.AzureStorage/) - includes MSI and regular access to Azure Storage Blob or Queue.
7. [`Bet.Extensions.HealthChecks`](./src/Bet.Extensions.HealthChecks/) - many useful HealChecks for Kubernetes.
8. [`Bet.Extensions.HealthChecks.AzureStorage`](./src/Bet.Extensions.HealthChecks.AzureStorage/) provides with HealthChecks Azure Storage

## AspNetCore specific functionality

1. [`Bet.AspNetCore`](./src/Bet.AspNetCore/) specific functionality for web applications.
2. [`Bet.AspNetCore.HealthChecks`](./src/Bet.Extensions.Hosting/) provides with HealthChecks for most common scenarios of the web application.
3. [`Bet.AspNetCore.Logging`](./src/Bet.AspNetCore.Logging/) contains logging functionality for `AspNetCore` applications such as azure analyzer and AppInsight extends `Serilog`.
4. [`Bet.AspNetCore.ReCapture`](./src/Bet.AspNetCore.ReCapture/) - a package for Google ReCapture.
5. [`Bet.AspNetCore.LetsEncrypt`](./src/Bet.Extensions.Hosting/) - enables SSL inside of docker container i.e. hosted in Azure Containers.
6. [`Bet.AspNetCore.Middleware`](./src/Bet.AspNetCore.Middleware/)

## Sample Applications

All of the sample applications are deployable to local Kubernetes Cluster.

1. [`Bet.AspNetCore.Sample`](./src/Bet.AspNetCore.Sample/) - `AspNetCore` Web App with spam and sentiment prediction models.
2. [`Bet.Hosting.Sample`](./src/Bet.Hosting.Sample/) - DotNetCore Worker App that can run in Kubernetes as CronJob.

## Domain Driven Development - `Clean Architecture`

1. [Bet.CleanArchitecture.Core](./src/Bet.CleanArchitecture.Core/)

