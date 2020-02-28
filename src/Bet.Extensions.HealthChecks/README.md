# Bet.Extensions.HealthChecks

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.HealthChecks.svg)](https://www.nuget.org/packages?q=Bet.Extensions.HealthChecks)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.Extensions.HealthChecks/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.Extensions.HealthChecks/latest/download)

The collection of the HealthChecks functionality for `IHost` that can be used within the `Worker` in Kubernetes.

Pre-release packages are distributed via feedz.io `https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json`.

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
        .AddSigtermCheck("Sigterm_shutdown_check")
        .AddSocketListener(8080);
```

## Helm configuration for Kubernetes

```yaml
    spec:
      containers:
        - name: {{ .Chart.Name }}
          image: "{{ .Values.image.repository }}:{{ .Values.image.tag }}"
          imagePullPolicy: {{ .Values.image.pullPolicy }}
          ports:
            - name: health
              containerPort: 8080
              protocol: TCP
          livenessProbe:
            tcpSocket:
              path: /
              port: health
          readinessProbe:
            tcpSocket:
              path: /
              port: health
```