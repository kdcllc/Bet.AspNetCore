# Bet.AspNetCore Libraries

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.AspNetCore/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.AspNetCore.svg)](https://www.nuget.org/packages?q=Bet.AspNetCore)
![Nuget](https://img.shields.io/nuget/dt/Bet.AspNetCore)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.AspNetCore/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.AspNetCore/latest/download)

> The second letter in the Hebrew alphabet is the ב bet/beit. Its meaning is "house". In the ancient pictographic Hebrew it was a symbol resembling a tent on a landscape.

_Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json)._

This goal of this repo is to provide with a reusable functionality for developing Microservices with Docker and Kubernetes.
These libraries extend `Microsoft.Extensions` and `Microsoft.AspNetCore` namespaces accordingly.

## Hire me

Please send [email](mailto:kingdavidconsulting@gmail.com) if you consider to **hire me**.

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## AspNetCore specific functionality

1. [`Bet.AspNetCore`](./src/Bet.AspNetCore/) specific functionality for web applications.
2. [`Bet.AspNetCore.HealthChecks`](./src/Bet.Extensions.Hosting/) provides with HealthChecks for most common scenarios of the web application.
3. [`Bet.AspNetCore.Logging`](./src/Bet.AspNetCore.Logging/) contains logging functionality for `AspNetCore` applications such as azure analyzer and AppInsight extends `Serilog`.
4. [`Bet.AspNetCore.ReCapture`](./src/Bet.AspNetCore.ReCapture/) - a package for Google ReCapture.
5. [`Bet.AspNetCore.LetsEncrypt`](./src/Bet.Extensions.Hosting/) - enables SSL inside of docker container i.e. hosted in Azure Containers.
6. [`Bet.AspNetCore.Middleware`](./src/Bet.AspNetCore.Middleware/)
7. [`Bet.AspNetCore.AzureStorage`](./src/Bet.AspNetCore.AzureStorage/) - extends `AspNetCore` and enables usage of Azure Blob Storage, Queues, Tables with MSI identity or token.
8. [`Bet.AspNetCore.Jwt`](./src/Bet.AspNetCore.Jwt/) - Provides a simple and a quick way to get started with JWT authentication for your app.
9. [`Bet.AspNetCore.ApiKeyAuthentication`](./src/Bet.AspNetCore.ApiKeyAuthentication/) Provides with custom authentication for Web Apis with Query Url parameter or Header.

## Sample Applications

All of the sample applications are deployable to local Kubernetes Cluster.
1. [`Bet.AspNetCore.Sample`](./src/Bet.AspNetCore.Sample/) - `AspNetCore` Web App with spam and sentiment prediction models based on ([ML.NET](https://github.com/dotnet/machinelearning)). Utiizes the following libraries
    - [`Bet.Extensions.ML`](https://github.com/kdcllc/Bet.Extensions/tree/master/src/Bet.Extensions.ML/) - includes Machine Learning library around [ML.NET](https://github.com/dotnet/machinelearning).
    - [`Bet.Extensions.ML.Sentiment`](https://github.com/kdcllc/Bet.Extensions/tree/master/src/Bet.Extensions.ML.Sentiment/) - Sentiment self-contained prediction engine to be used with any DotNetCore applications.
    - [`Bet.Extensions.ML.Spam`](https://github.com/kdcllc/Bet.Extensions/tree/master/src/Bet.Extensions.ML.Spam/) - Spam self-contained prediction engine to be used with any DotNetCore applications.
    - [`Bet.Extensions.HealthChecks.ML`](https://github.com/kdcllc/Bet.Extensions/tree/master/src/Bet.Extensions.HealthChecks.ML/) provides with HealthChecks ML.NET Models.
    - [`Bet.Extensions.ML.Azure`](https://github.com/kdcllc/Bet.Extensions/tree/master/src/Bet.Extensions.ML.Azure/) provides with extensions methods for Azure Blob Storage Model monitoring and reload.


2. [`Bet.K8s.Web.Sample`](./src/Bet.K8s.Web.Sample/) - `AspNetCore` Web App with spam and sentiment prediction models.
3. [`LetsEncryptWeb`](./src/LetsEncryptWeb/) - `AspNetCore` Web App with spam and sentiment prediction models.

## `DotNetCore` CLI global tools

1. [AppAuthentication](https://github.com/kdcllc/AppAuthentication) - enables Microsoft Managed Identity (MSI) testing `Azure Key Vault` or `Azure Blob Storage` access from Docker Container in Local development based on token authentication.
   ![AppAuthentication](./img/appauthentication-docker-debug.gif)

```bash
    # adds local tool manifest file
    dotnet new tool-manifest

    # install appauthentication local version
    dotnet tool install appauthentication
```

## `Bet.Extensions` libraries that build on `Microsoft.Extensions`:

1. [`Bet.Extensions`](https://github.com/kdcllc/Bet.Extensions/tree/master/src/Bet.Extensions/) - extends many DotNetCore classes.
2. [`Bet.Extensions.Options`](https://github.com/kdcllc/Bet.Extensions/tree/master/src/Bet.Extensions.Options/) - extends Options with common functionality such as `Bind()` validations.
3. [`Bet.Extensions.Logging`](https://github.com/kdcllc/Bet.Extensions/tree/master/src/Bet.Extensions.Logging/) - extends shared/common logging functionality.
4. [`Bet.Extensions.Hosting`](https://github.com/kdcllc/Bet.Extensions/tree/master/src/Bet.Extensions.Hosting/) - extends generic functionality for `IHost`.
5. [`Bet.Extensions.AzureVault`](https://github.com/kdcllc/Bet.Extensions/tree/master/src/Bet.Extensions.AzureVault/) - extends Azure Vault functionality.
6. [`Bet.Extensions.AzureStorage`](https://github.com/kdcllc/Bet.Extensions/tree/master/src/Bet.Extensions.AzureStorage/) - extends MSI and regular access to Azure Storage Blob or Queue.
7. [`Bet.Extensions.HealthChecks`](https://github.com/kdcllc/Bet.Extensions/tree/master/src/Bet.Extensions.HealthChecks/) - extends useful HealChecks for Kubernetes, including `Worker` tcp based healthchecks.
8. [`Bet.Extensions.HealthChecks.AzureStorage`](https://github.com/kdcllc/Bet.Extensions/tree/master/src/Bet.Extensions.HealthChecks.AzureStorage/) - provides with HealthChecks Azure Storage
9. [`Bet.Extensions.DataProtection`](https://github.com/kdcllc/Bet.Extensions/tree/master/src/Bet.Extensions.DataProtection/) - extends `DataProtection` to store encryption keys on Azure Storage Blob.


## About Docker Images

This repo is utilizing [King David Consulting LLC Docker Images](https://hub.docker.com/u/kdcllc):

- [kdcllc/dotnet-sdk:3.1](https://hub.docker.com/r/kdcllc/dotnet-sdk-vscode): - the docker image for templated `DotNetCore` build of the sample web application.

- [kdcllc/dotnet-sdk-vscode:3.1](https://hub.docker.com/r/kdcllc/dotnet-sdk/tags): the docker image for the Visual Studio Code In container development.

- [Docker Github repo](https://github.com/kdcllc/docker/blob/master/dotnet/dotnet-docker.md)
