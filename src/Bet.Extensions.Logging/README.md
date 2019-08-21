# Bet.Extensions.Logging

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.Logging.svg)](https://www.nuget.org/packages?q=Bet.Extensions.Logging)

Add the following to the project

```csharp
    dotnet add package Bet.Extensions.Logging
```

## Usage

Sometimes we need to have tracing information enable to see where the configuration details are loaded from.

```csharp

    new HostBuilder()
          .ConfigureAppConfiguration((context, configBuilder) =>
          {
               configBuilder.Build().DebugConfigurations();
          }
```
