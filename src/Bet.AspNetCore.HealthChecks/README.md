# Bet.AspNetCore.HealthChecks

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.AspNetCore.HealthChecks.svg)](https://www.nuget.org/packages?q=Bet.AspNetCore.HealthChecks)

The collection of the HealthChecks functionality for AspNetCore.

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
