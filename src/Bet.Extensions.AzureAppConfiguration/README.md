# Bet.Extensions.AzureAppConfiguration

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.AspNetCore/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.AzureAppConfiguration.svg)](https://www.nuget.org/packages?q=Bet.Extensions.AzureAppConfiguration)
![Nuget](https://img.shields.io/nuget/dt/Bet.Extensions.AzureAppConfiguration)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.Extensions.AzureAppConfiguration/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.Extensions.AzureAppConfiguration/latest/download)

> The second letter in the Hebrew alphabet is the ב bet/beit. Its meaning is "house". In the ancient pictographic Hebrew it was a symbol resembling a tent on a landscape.

*Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json).*

## Summary

The purpose of this project is to provide with extension method to access Azure AppConfiguration and FeatureManagement for the application.

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Install

```csharp
    dotnet add package Bet.Extensions.AzureAppConfiguration
```

## Usage

For best practices Microsoft Managed Identity must be used for Endpoint to Azure App Configuration which in turn doesn't work with local development for this reason, please refer to usage of [`AppAuthentication`](https://github.com/kdcllc/AppAuthentication) dotnet cli tool.
**NOTE: this tool is only used on the local development machine. In Azure Cloud MSI Identity is provided thru Environment variables.**
```bash

    # install the tool
    dotnet tool install --global appauthentication

    # run this first before opening vs.net or vscode;
    # this it will create proper environments for you.
    appauthentication run -l --verbose:debug
```
