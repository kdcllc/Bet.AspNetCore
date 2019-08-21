# Bet.AspNetCore Libraries

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.AspNetCore.svg)](https://www.nuget.org/packages?q=Bet.AspNetCore)
[![MyGet](https://img.shields.io/myget/kdcllc/v/Bet.AspNetCore.svg?label=myget)](https://www.myget.org/F/kdcllc/api/v2)

This repo contains several projects that provide with extended functionality for `DotNetCore` framework. The projects are split into two category:

## Machine Learning (ML.NET) functionality

1. [`Bet.Extensions.ML`](./src/Bet.Extensions.ML/README.md) - includes Machine Learning library around [ML.NET](https://github.com/dotnet/machinelearning).
2. [`Bet.Extensions.ML.Sentiment`](./src/Bet.Extensions.ML.Sentiment/README.md) - Sentiment self-contained prediction engine to be used with any DotNetCore applications.
3. [`Bet.Extensions.ML.Spam`](./src/Bet.Extensions.ML.Spam/README.md) - Spam self-contained prediction engine to be used with any DotNetCore applications.
4. [`Bet.ML.WebApi.Sample`](./src/Bet.ML.WebApi.Sample/README.md) - AspNetCore Web Api application with predictive engine enabled.

## DotNet CLI global tools

1. [AppAuthentication](./src/AppAuthentication/README.md) - enables Azure Vault access from Docker Container in Local development.

## Generic functionally

1. [`Bet.Extensions`](./src/Bet.Extensions/README.md) extends many DotNetCore classes.
2. [`Bet.Extensions.Options`](./src/Bet.Extensions.Options/README.md) - extends Options with common functionality such as `Bind()` validations.
3. [`Bet.Extensions.Logging`](./src/Bet.Extensions.Logging/README.md) - includes shared/common logging functionality.
4. [`Bet.Extensions.Hosting`](./src/Bet.Extensions.Hosting/README.md) - extends generic functionality for `IHost`.
5. [`Bet.Extensions.AzureVault`](./src/Bet.Extensions.AzureVault/README.md) - includes Azure Vault functionality.
6. [`Bet.Extensions.AzureStorage`](./src/Bet.Extensions.AzureStorage/README.md) - includes MSI and regular access to Azure Storage Blob or Queue.

## AspNetCore specific functionality

1. [`Bet.AspNetCore`](./src/Bet.AspNetCore/README.md) specific functionality for web applications.
2. [`Bet.AspNetCore.HealthChecks`](./src/Bet.Extensions.Hosting/README.md) provides with HealthChecks for most common scenarios of the web application.
3. [`Bet.AspNetCore.Logging`](./src/Bet.AspNetCore.Logging/README.md) contains logging functionality for `AspNetCore` applications such as azure analyzer and AppInsight extends `Serilog`.
4. [`Bet.AspNetCore.ReCapture`](./src/Bet.AspNetCore.ReCapture/README.md) - a package for Google ReCapture.
5. [`Bet.AspNetCore.LetsEncrypt`](./src/Bet.Extensions.Hosting/README.md) - enables SSL inside of docker.
6. [`Bet.AspNetCore.Middleware`](./src/Bet.AspNetCore.Middleware/README.md)

## Sample Applications

1. [`Bet.AspNetCore.Sample`](./src/Bet.AspNetCore.Sample/README.md) - `AspNetCore` Web App with spam prediction models.
2. [`Bet.ML.WebApi.Sample`](./src/Bet.ML.WebApi.Sample/README.md) - AspNetCore Web Api application with predictive engine enabled.
3. [`Bet.Hosting.Sample`](./src/Bet.Hosting.Sample/README.md)

## Domain Driven Development - `Clean Architecture`

1. [Bet.CleanArchitecture.Core](./src/Bet.CleanArchitecture.Core/README.md)

## DotNetCore and AspNetCore 3.0 Resources

- [Migrate from ASP.NET Core 2.2 to 3.0](https://docs.microsoft.com/en-us/aspnet/core/migration/22-to-30?view=aspnetcore-2.2&tabs=visual-studio)
- [Tutorial: Migrate existing code with nullable reference types](https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/upgrade-to-nullable-references#upgrade-the-projects-to-c-8)
