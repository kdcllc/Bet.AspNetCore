# Bet.Extensions.ML

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.AspNetCore/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/fo9rakj7s7uhs3ij?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.ML.svg)](https://www.nuget.org/packages?q=Bet.Extensions.ML)
![Nuget](https://img.shields.io/nuget/dt/Bet.Extensions.ML)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-aspnetcore/shield/Bet.Extensions.ML/latest)](https://f.feedz.io/kdcllc/bet-aspnetcore/packages/Bet.Extensions.ML/latest/download)

> The second letter in the Hebrew alphabet is the ב bet/beit. Its meaning is "house". In the ancient pictographic Hebrew it was a symbol resembling a tent on a landscape.

*Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/bet-aspnetcore/nuget/index.json).*

## Summary

The goal of this repo is to provide with production ready extensions to [ML.NET library](https://github.com/dotnet/machinelearning).

It contains two major functionality:

1. ML.NET ModelBuilder Pipeline with ability to load from different sources i.e. Azure Blob Storage.

2. ML.NET Web Api hosting with caching of ML models to improve performance. This library utilizes `ObjectPool` similar to  [Extensions.ML](https://github.com/glennc/Extensions.ML)

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!


## Install

```csharp
    dotnet add package Bet.Extensions.ML
```

## Usage

For complete examples please refer to sample projects:

1. [`Bet.AspNetCore.Sample`](../Bet.AspNetCore.Sample/) - `AspNetCore` Web App with spam prediction models.
2. [`Bet.Hosting.Sample`](../Bet.Hosting.Sample/) - `DotNetCore` Worker based scheduled job for generating ML.NET Models.

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

## Basics of ML.NET Presentation

### 1. ML.NET Preparing Data Apis

![ML.NET Preparing Data Apis](../../img/ml.net/01-ml.net-preparing-data-apis.png)

### 2. ML.NET Preparing and Filtering Data Apis

![ML.NET Preparing and Filtering Data Apis](../../img/ml.net/02-ml.net-preparing-filtering-data-apis.png)

### 3. ML.NET Regression Basics

![ML.NET Regression Basics](../../img/ml.net/03-ml.net-regression-basics.png)

### 4. ML.NET Regression Trainers

![ML.NET Regression Trainers](../../img/ml.net/04-ml.net-regression-trainers.png)

### 5. ML.NET OSL Regression

![ML.NET OSL Regression](../../img/ml.net/05-ml.net-basic-of-osl-regression.png)

### 6. ML.NET Basics of Decision Trees and Random Forests

![ML.NET Basics of Decision Trees and Random Forests](../../img/ml.net/06-ml.net-basic-of-random-tree-forests.png)

### 7. ML.NET Regression with Random Forests

![ML.NET Regression with Random Forests](../../img/ml.net/07-ml.net-regression-with-random-forsets.png)

### 8. ML.NET Trainer Options

![ML.NET Trainer Options](../../img/ml.net/08-ml.net-trainer-options-object.png)

### 9. ML.NET Scoring Regression Models

![ML.NET Scoring Regression Models](../../img/ml.net/09-ml.net-scoring-regression-models.png)

### 10. ML.NET Cross Validating Regression Models

![ML.NET Cross Validating Regression Models](../../img/ml.net/10-ml.net-cross-validating-regression-models.png)

### 11. ML.NET Basics of `One-Hot-Encoding`

![ML.NET Basics of `One-Hot-Encoding`](../../img/ml.net/11-ml.net-basics-of-one-hot-encoding.png)

### 12. ML.NET Binary Classifications

![ML.NET Binary Classifications](../../img/ml.net/12-ml.net-classifications.png)

### 13. Binary Classification Trainers

![ML.NET Binary Classification Trainers](../../img/ml.net/13-ml.net-binary-classification-trainers.png)

### 14. 1ML.NET Scoring Binary Classification Models

![ML.NET Scoring Binary Classification Models](../../img/ml.net/14-ml.net-scoring-binary-classification-models.png)

### 15. ML.NET Cross Validating Binary Classification Models

![ML.NET Cross Validating Binary Classification Models](../../img/ml.net/15-ml.net-cross-validating-binary-classification-models.png)

### 16. ML.NET Vectorizing Text Functionality

![ML.NET Vectorizing Text Functionality](../../img/ml.net/16-ml.net-vectorizing-text.png)

### 17. ML.NET Specifying Text Vectorization Options

![ML.NET Specifying Text Vectorization Options](../../img/ml.net/17-ml.net-specifying-text-vectorization-options.png)

### 18. ML.NET Multi Classification Trainers

![ML.NET Multi Classification Trainers](../../img/ml.net/18-ml.net-multi-class-classification-trainers.png)

### 19. ML.NET Scoring Multi Classification Models

![ML.NET Scoring Multi Classification Models](../../img/ml.net/19-ml.net-scoring-multi-classification-models.png)

### 20. ML.NET Classifying Images

![ML.NET Classifying Images](../../img/ml.net/20-ml.net-classifying-images.png)

### 21.ML.NET Convolutional Neutral Networks

![ML.NET Convolutional Neutral Networks](../../img/ml.net/21-ml.net-convolutional-neutral-networks.png)

### 22. ML.NET Transfer Learning

![ML.NET Transfer Learning](../../img/ml.net/22-ml.net-transfer-learning.png)

### 23. ML.NET Pre-trained Convolutional Neutral Networks

![ML.NET Pre-trained Convolutional Neutral Networks](../../img/ml.net/23-ml.net-pretrained-convolutional-neutral-networks.png)

### 24. ML.NET AutoML

![ML.NET AutoML ](../../img/ml.net/24-ml.net-automl.png)

### 25. ML.NET AutoML Regression Model Training

![ML.NET AutoML Regression Model Training](../../img/ml.net/25-ml.net-automl-regression-model.png)

## References

- [Jeff Prosise Presentation](https://github.com/jeffprosise/ML.NET)
