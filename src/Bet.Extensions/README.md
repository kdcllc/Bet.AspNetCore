# Bet.Extensions

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.AspNetCore/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.svg)](https://www.nuget.org/packages?q=Bet.Extensions)
![Nuget](https://img.shields.io/nuget/dt/Bet.Extensions)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.Extensions/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.Extensions/latest/download)

*Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json).*

The goal of this repo is to provide with reusable `nuget` package that contains commonly used functionality i.e. `ValueStopwatch`.

## Install

```csharp
    dotnet add package Bet.Extensions
```

## Usage

The following classes to support `Async` development:

- `ActionOrAsyncFunc`
- `AsyncExpiringLazy`
- `AsyncFunc`
- `AsyncLock`
- `ReloadToken`
- `ChannelReader`