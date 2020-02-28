# Bet.Extensions.ML.Sentiment

[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.ML.Sentiment.svg)](https://www.nuget.org/packages?q=Bet.Extensions.ML.Sentiment)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.Extensions.ML.Sentiment/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.Extensions.ML.Sentiment/latest/download)

Pre-release packages are distributed via feedz.io `https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json`.

Add the following to the project

```csharp
    dotnet add package Bet.Extensions.ML.Sentiment
```

This project is self contained ML.NET project to be used within other DotNetCore applications.

## Usage

For complete examples please refer to sample projects:

1. [`Bet.AspNetCore.Sample`](../Bet.AspNetCore.Sample/README.md) - `AspNetCore` Web App with spam prediction models.
2. [`Bet.Hosting.Sample`](../Bet.Hosting.Sample/README.md)


```csharp
    // 1. register
    services.AddSentimentModelBuilder();

    services.AddTimedHostedService<ModelBuilderHostedService>(options =>
    {
        options.Interval = TimeSpan.FromMinutes(30);
        options.FailMode = FailMode.LogAndRetry;
        options.RetryInterval = TimeSpan.FromSeconds(30);
    });

    // 2. build model

    public class ModelBuilderHostedService : TimedHostedService
    {
        private readonly IEnumerable<IModelBuilderService> _modelBuilders;

        public ModelBuilderHostedService(
            IEnumerable<IModelBuilderService> modelBuilders,
            IOptionsMonitor<TimedHostedServiceOptions> options,
            IEnumerable<ITimedHostedLifeCycleHook> lifeCycleHooks,
            ILogger<ITimedHostedService> logger) : base(options, lifeCycleHooks, logger)
        {
            TaskToExecuteAsync = (token) => RunModelGenertorsAsync(token);
            _modelBuilders = modelBuilders ?? throw new ArgumentNullException(nameof(modelBuilders));
        }

        public async Task RunModelGenertorsAsync(CancellationToken cancellationToken)
        {
            foreach (var modelBuilder in _modelBuilders)
            {
                try
                {
                    await modelBuilder.TrainModelAsync(cancellationToken);

                    await modelBuilder.ClassifyTestAsync(cancellationToken);

                    await modelBuilder.SaveModelAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.LogError("{modelBuilder} failed with exception: {message}", modelBuilder.GetType(), ex.Message);
                }
            }
        }
    }

    // 3. predict
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionController : ControllerBase
    {
        private readonly IModelPredictionEngine<SentimentIssue, SentimentPrediction> _sentimentModel;

        public PredictionController(
            IModelPredictionEngine<SentimentIssue, SentimentPrediction> sentimentModel)
        {
            _sentimentModel = sentimentModel ?? throw new ArgumentNullException(nameof(sentimentModel));
        }

        [HttpPost()]
        public ActionResult<SentimentPrediction> GetSentiment(SentimentIssue input)
        {
            return _sentimentModel.Predict(input);
        }
    }
```
