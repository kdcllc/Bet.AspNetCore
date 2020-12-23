# Bet.AspNetCore

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.AspNetCore.svg)](https://www.nuget.org/packages?q=Bet.AspNetCore)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.AspNetCore/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.AspNetCore/latest/download)

> The second letter in the Hebrew alphabet is the ב bet/beit. Its meaning is "house". In the ancient pictographic Hebrew it was a symbol resembling a tent on a landscape.

*Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json).*

## Summary

The collection of the functionality for AspNetCore WebHost.
`AspNetCore` has very flexible Configuration providers framework.
Application can load Configurations from various locations.
The last provider overrides the values of the same kind of keys.

Since it has such flexibility was introduces the validation of the Configurations was missing until `2.2`. `AsptNetCore` 2.2 release introduced extension methods on `OptionsBuilder` which if registered will be validated on the first use of the Options, but there are cases where application needs to terminate at startup if the required configurations are missing. This is where the collection of the libraries from this solution come in.


[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Configuration Validations for `IWebHost`

```bash
    dotnet add package Bet.AspNetCore
    dotnet add package Bet.Extensions.Options
```

[Usage Bet.Extensions.Options](./src/Bet.Extensions.Options/README.md).

To enable validation add before any Configuration validation are added in `Startup.cs` within `ConfigureServices`

```csharp
    services.AddConfigurationValidation();
```

Then add validation for Configurations as action call back or as data annotations

```csharp
   services.ConfigureWithValidation<FakeOptions>(Configuration, opt =>
      {
          if (opt.Id > 0 && !string.IsNullOrWhiteSpace(opt.Name))
          {
              return true;
          }
          return false;
      }, "This didn't validated.");

   services.ConfigureWithDataAnnotationsValidation<FakeOptionsWithDataAnnotations>(configuration, sectionName: "FakeOptions");
```

### Simplify configuration for OpenApi / Swagger UI, Document Generation

```csharp

public void ConfigureServices(IServiceCollection services)
{
  services.AddSwaggerGenWithApiVersion<Startup>();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}
```
