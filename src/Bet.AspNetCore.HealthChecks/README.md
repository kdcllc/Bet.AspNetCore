# Bet.AspNetCore.HealthChecks

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.AspNetCore/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.AspNetCore.HealthChecks.svg)](https://www.nuget.org/packages?q=Bet.AspNetCore.HealthChecks)
![Nuget](https://img.shields.io/nuget/dt/Bet.AspNetCore.HealthChecks)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.AspNetCore.HealthChecks/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.AspNetCore.HealthChecks/latest/download)

*Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json).*

The collection of the HealthChecks functionality for AspNetCore.

## Install

```csharp
    dotnet add package Bet.AspNetCore.HealthChecks
```

## Usage

1. Add `HealthCheck` configuration in `ConfigureServices`.

```csharp

    services.AddHealthChecks()
        // validates ssl certificate
        .AddSslCertificateCheck("localhost", "https://localhost:5001")
        .AddSslCertificateCheck("kdcllc", "https://kingdavidconsulting.com")
        // Checks specific url
        .AddUriHealthCheck("200_check", builder =>
        {
            builder.Add(option =>
            {
                option.AddUri("https://httpstat.us/200")
                        .UseExpectedHttpCode(HttpStatusCode.OK);
            });

            builder.Add(option =>
            {
                option.AddUri("https://httpstat.us/203")
                        .UseExpectedHttpCode(HttpStatusCode.NonAuthoritativeInformation);
            });
        })
        .AddUriHealthCheck("ms_check", uriOptions: (options) =>
        {
            options.AddUri("https://httpstat.us/503").UseExpectedHttpCode(503);
        })
        // used with kubernetes
        .AddSigtermCheck("Sigterm_shutdown_check");
```

2. Add in `Configure`

```csharp
    // returns 200 okay
    // default /liveness
    app.UseLivenessHealthCheck();

    // default /healthy
    // returns healthy if all healthcheks return healthy
    app.UseHealthyHealthCheck();
```
