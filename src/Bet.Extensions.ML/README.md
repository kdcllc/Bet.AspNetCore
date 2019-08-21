# Bet.Extensions.ML

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.ML.svg)](https://www.nuget.org/packages?q=Bet.Extensions.ML)

Add the following to the project

```csharp
    dotnet add package Bet.Extensions.ML
```

A library that provides production ready extensions to [ML.NET library](https://github.com/dotnet/machinelearning).
It contains two major functionality:

1. ML.NET ModelBuilder

- ML.NET models builder based on ML.NET AutoML generation idea

2. ML.NET Web Api hosting with caching of ML models

- Utilizing `ObjectPool` similar to  [Extensions.ML](https://github.com/glennc/Extensions.ML)

## Usage

For complete examples please refer to sample projects:

1. [`Bet.AspNetCore.Sample`](../Bet.AspNetCore.Sample/README.md) - `AspNetCore` Web App with spam prediction models.
2. [`Bet.ML.WebApi.Sample`](../Bet.ML.WebApi.Sample/README.md) - AspNetCore Web Api application with predictive engine enabled.
3. [`Bet.Hosting.Sample`](../Bet.Hosting.Sample/README.md)

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
