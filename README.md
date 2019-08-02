# Bet.AspNetCore solution

This repo contains several projects that provide with extended functionality for `DotNetCore` framework. The projects are split into two category:

1. `Bet.Extensions` generic functionality for `DotNetCore` in general.
    - `Bet.Extensions.Options` - includes Application Options and `Bind()` validations.
    - `Bet.Extensions.Logging` - includes shared/common logging functionality.
    - `Bet.Extensions.Hosting` - includes Generic Hosting functionality.
    - `Bet.Extensions.AzureVault` - includes Azure Vault functionality.
    - `Bet.Extensions` - includes extensions methods for `DotNetCore`.
    - [`Bet.Extensions.AzureStorage` -includes MSI and regular access to Azure Storage Blob or Queue.](./src/Bet.Extensions.AzureStorage/README.md)

2. `Bet.AspNetCore` specific functionality for web applications.
    - `Bet.AspNetCore.HealthChecks` contains HealthChecks for most common scenarios of the web application.
    - `Bet.AspNetCore.Logging` contains logging functionality for `AspNetCore` applications.
    - `Bet.AspNetCore` - default location for `AspNetCore`.
    - [`Bet.AspNetCore.ReCapture` - ability to verify users submissions.](./src/Bet.AspNetCore.ReCapture/README.md)

3. `ML.NET - Machine Learning`  
    - [`Bet.Extensions.ML`](./src/Bet.Extensions.ML/README.md) - includes Machine Learning library around [ML.NET](https://github.com/dotnet/machinelearning).
    - [`Bet.Extensions.ML.Sentiment`](./src/Bet.Extensions.ML.Sentiment/README.md) - Sentiment self-contained prediction engine to be used with any DotNetCore applications.
    - [`Bet.Extensions.ML.Spam`](./src/Bet.Extensions.ML.Spam/README.md) - Spam self-contained prediction engine to be used with any DotNetCore applications.
    - [`Bet.ML.WebApi.Sample`](./src/Bet.ML.WebApi.Sample/README.md) - AspNetCore Web Api application with predictive engine enabled.

## Bet.Extensions.ML
This library provides a helper classes for Microsoft C# based Machine Learning Library [ML.NET](https://github.com/dotnet/machinelearning).

To include Machine Learning prediction the following can be added:

```csharp

    services.AddModelPredictionEngine<SpamInput, SpamPrediction>(mlOptions =>
    {
        mlOptions.MLContext = () =>
        {
            var mlContext = new MLContext();
            mlContext.ComponentCatalog.RegisterAssembly(typeof(LabelTransfomer).Assembly);
            mlContext.Transforms.CustomMapping<LabelInput, LabelOutput>(LabelTransfomer.Transform, nameof(LabelTransfomer.Transform));

            return mlContext;
        };

        mlOptions.CreateModel = (mlContext) =>
        {
            using (var fileStream = File.OpenRead("MLContent/SpamModel.zip"))
            {
                return mlContext.Model.Load(fileStream);
            }
        };
    },"SpamModel");
```
Then in the API Controller:

```csharp
[Route("api/[controller]")]
    [ApiController]
    public class PredictionController : ControllerBase
    {
        private readonly IModelPredictionEngine<SentimentObservation, SentimentPrediction> _sentimentModel;
        private readonly IModelPredictionEngine<SpamInput, SpamPrediction> _spamModel;

        public PredictionController(
            IModelPredictionEngine<SentimentObservation, SentimentPrediction> sentimentModel,
            IModelPredictionEngine<SpamInput, SpamPrediction> spamModel)
        {
            _sentimentModel = sentimentModel ?? throw new ArgumentNullException(nameof(sentimentModel));
            _spamModel = spamModel ?? throw new ArgumentNullException(nameof(spamModel));
        }

        [HttpPost()]
        public ActionResult<SentimentPrediction> GetSentiment(SentimentObservation input)
        {
            return _sentimentModel.Predict(input);
        }

        // GET /api/prediction/spam?text=Hello World
        [HttpGet]
        [Route("spam")]
        public ActionResult<SpamPrediction> PredictSpam([FromQuery]string text)
        {
            return _spamModel.Predict(new SpamInput { Message = text});
        }
    }
```

## Bet.Extensions.AzureStorage
This library provides a cohesive object model that deals with authentication and access to Azure Storage Blob and Queues. Please visit the project details
[Bet.Extensions.AzureStorage](./src/Bet.Extensions.AzureStorage/README.md).



## AppAuthenticaion CLI tool

Azure Vault is a great utility but what if your company doesn't allow you to have secret and client id. What if you want to use Docker Containers
on your local machine and be able to authenticate your Azure Vault. This utility provides this functionality.

[AppAuthenticaion CLI tool Readme](./src/AppAuthentication/README.md)

## Bet.AspNetCore.ReCapture

[Bet.AspNetCore.ReCapture](./src/Bet.AspNetCore.ReCapture/README.md)

## Configuration Validation

`AspNetCore` has very flexible Configuration providers framework. Application can load Configurations from various locations. The last provider overrides the values of the same kind of keys.
Since it has such flexibility was introduces the validation of the Configurations was missing until `2.2`. `AsptNetCore` 2.2 release introduced extension methods on `OptionsBuilder` which if registered will be validated on the first use of the Options, but there are cases where application needs to terminate at startup if the required configurations are missing. This is where the collection of the libraries from this solution come in.

### Configuration Validations for `IWebHost`

```bash
    dotnet add package Bet.AspNetCore
    dotnet add package Bet.Extensions.Options
```

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

### Configuration Validations for `IHost`

```bash
    dotnet add package Bet.Extensions.Hosting
    dotnet add package Bet.Extensions.Options
```

To enable validation use `UseStartupFilter` in `Program.cs`

```csharp
        var host = new HostBuilder()
                .UseStartupFilter().Build();
```

Usage of `ConfigureWithDataAnnotationsValidation` or `ConfigureWithValidation` the same as in `IWebHost`

### Bind Object with Validation

Simply add this package to your application and use `Bind` validation extensions methods

```bash
    dotnet add package Bet.Extensions.Options
```

```csharp
    var options = config.Bind<MyOptionsWithDatatAnnotations>(options);
```

## Azure Key Vault Configuration Provider

```csharp
    dotnet add package Bet.Extensions.AzureVault
```

This provider requires the following configurations to be present in any other configuration providers.
The `ClientId` and `ClientSecret` are not needed if used within Visual Studio.Net or in Azure Web Application with MSI enable.

```json
"AzureVault": {
    "BaseUrl": "https://kdcllc.vault.azure.net/",
    "ClientId": "",
    "ClientSecret": ""
  }
```

At minimum this provider requires to have BaseUrl and will authenticated based on the Visual Studio.Net 
Credentials. For `Docker` Containers and other environments where Microsoft MSI can't be used provide `ClientId` and `ClientSecret`.

In order to use Azure Key Vault register it with `IServiceCollection`.

```c#
   configBuilder.AddAzureKeyVault(hostingEnviromentName:hostContext.HostingEnvironment.EnvironmentName);
```

## Logging Configurations for debugging

Add the following to the project

```csharp
    dotnet add package Bet.Extensions.Logging
```

Sometimes we need to have tracing information enable to see where the configuration details are loaded from.

```csharp

    new HostBuilder()
          .ConfigureAppConfiguration((context, configBuilder) =>
          {
               configBuilder.Build().DebugConfigurations();
          }
```

## Logging Extension with Serilog

Add the following to the project

```csharp
    dotnet add package Bet.AspNetCore.Logging
```

To enable Azure `ApplicationInsights` and/or `LogAnalytics` Seriglog sinks add the following in `Program.cs`:

```csharp
                .UseSerilog((hostingContext, loggerConfiguration) =>
                {
                    loggerConfiguration
                          .ReadFrom.Configuration(hostingContext.Configuration)
                          .Enrich.FromLogContext()
                          .WriteTo.Console()
                          .AddApplicationInsights(hostingContext.Configuration)
                          .AddAzureLogAnalytics(hostingContext.Configuration);
                })
```

Make sure that the following Options values are supplied.

```json
  "ApplicationInsights": {
    "InstrumentationKey": "",
    "EnableEvents": true,
    "EnableTraces": true
  },

  "AzureLogAnalytics": {
    "WorkspaceId": "",
    "AuthenticationId": ""
  }
```

## DotNetCore 3.0

- [Migrate from ASP.NET Core 2.2 to 3.0](https://docs.microsoft.com/en-us/aspnet/core/migration/22-to-30?view=aspnetcore-2.2&tabs=visual-studio)
- [Tutorial: Migrate existing code with nullable reference types](https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/upgrade-to-nullable-references#upgrade-the-projects-to-c-8)
