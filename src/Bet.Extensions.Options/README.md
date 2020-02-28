# Bet.Extensions.Options

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.Options.svg)](https://www.nuget.org/packages?q=Bet.Extensions.Options)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.Extensions.Options/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.Extensions.Options/latest/download)

The collection of the functionality for Options Validations that can be utilized for AspNetCore or GenericHost.

Pre-release packages are distributed via feedz.io `https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json`.

## Bind Object with Validation

Simply add this package to your application and use `Bind` validation extensions methods

```bash
    dotnet add package Bet.Extensions.Options
```

```csharp
    var options = config.Bind<MyOptionsWithDatatAnnotations>(options);
```
