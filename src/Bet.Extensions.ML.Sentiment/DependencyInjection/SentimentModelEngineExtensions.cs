using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.DataLoaders.ModelLoaders;
using Bet.Extensions.ML.DataLoaders.SourceLoaders;
using Bet.Extensions.ML.Helpers;
using Bet.Extensions.ML.ModelCreation;
using Bet.Extensions.ML.ModelCreation.Results;
using Bet.Extensions.ML.Sentiment.Models;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SentimentModelEngineExtensions
    {
        public static IServiceCollection AddSentimentModelCreationService<TModelLoader>(
            this IServiceCollection services,
            string modelName = "SentimentModel",
            double testSlipFraction = 0.1,
            Action<ModelLoderFileOptions>? configure = null)
            where TModelLoader : ModelLoader
        {
            var builder = services.AddModelCreationService<SentimentIssue, BinaryClassificationMetricsResult>(modelName);

            builder.AddSourceLoader<SentimentIssue, BinaryClassificationMetricsResult, EmbeddedSourceLoader<SentimentIssue>>(options =>
            {
                options.Sources.Add(new SourceLoaderFile<SentimentIssue>
                {
                    // overrides default loading mechanism
                    CustomAction = () =>
                    {
                        var inputs = EmbeddedResourceHelper
                        .GetRecords<InputSentimentIssueRow>("Content.wikiDetoxAnnotated40kRows.tsv", delimiter: "\t", hasHeaderRecord: true);

                        // convert int to boolean values
                        var result = new List<SentimentIssue>();
                        foreach (var item in inputs)
                        {
                            var newItem = new SentimentIssue
                            {
                                Label = item.Label != 0,
                                Text = item.comment
                            };

                            result.Add(newItem);
                        }

                        return result;
                    }
                });
            });

            builder.AddModelLoader<SentimentIssue, BinaryClassificationMetricsResult, TModelLoader>(configure);

            builder.ConfigureModelDefinition<SentimentIssue, BinaryClassificationMetricsResult>(
                testSlipFraction,
                options =>
                {
                    options.TrainingPipelineConfigurator = (mlContext) =>
                    {
                        // STEP 2: Common data process configuration with pipeline data transformations
                        var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SentimentIssue.Text));

                        // STEP 3: Set the training algorithm, then create and config the modelBuilder
                        var trainer = mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features");
                        var trainingPipeline = dataProcessPipeline.Append(trainer);

                        return new TrainingPipelineResult(trainingPipeline, trainer.ToString());
                    };

                    options.EvaluateConfigurator = (mlContext, model, trainerName, dataView, _) =>
                    {
                        // STEP 5: Evaluate the model and show accuracy stats
                        var predictions = model.Transform(dataView);
                        var metrics = mlContext.BinaryClassification.Evaluate(data: predictions, labelColumnName: "Label", scoreColumnName: "Score");

                        return new BinaryClassificationMetricsResult(trainerName, metrics);
                    };
                });

            builder.ConfigureService<SentimentIssue, BinaryClassificationMetricsResult>(
                options =>
                {
                    options.DataLoader = async (loader, cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var data = loader.LoadData();
                        return await Task.FromResult(data);
                    };

                    options.TrainModelConfigurator = (modelBuilder, data, logger) =>
                    {
                        // 1. load ML data set
                        modelBuilder.LoadData(data);

                        // 1. load default ML data set
                        modelBuilder.BuildDataView();

                        // 2. build training pipeline
                        var buildTrainingPipelineResult = modelBuilder.BuildTrainingPipeline();

                        // 3. train the model
                        var trainModelResult = modelBuilder.TrainModel();

                        // 4. evaluate quality of the pipeline
                        var evaluateResult = modelBuilder.Evaluate();
                        logger.LogInformation(evaluateResult.ToString());

                        return evaluateResult;
                    };

                    options.ClassifyTestConfigurator = async (modelBuilder, logger, cancellationToken) =>
                    {
                        // 5. predict on sample data
                        var sw = ValueStopwatch.StartNew();

                        var tasks = new List<Task>
                        {
                             ClassifyAsync(modelBuilder, "This is a very rude movie", false, logger, cancellationToken),
                             ClassifyAsync(modelBuilder, "Hate All Of You're Work", true, logger, cancellationToken)
                        };

                        await Task.WhenAll(tasks);
                    };
                });

            return services;
        }

        private static Task ClassifyAsync(
            IModelDefinitionBuilder<SentimentIssue, BinaryClassificationMetricsResult> modelBuilder,
            string text,
            bool expectedResult,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            return Task.Run(
                () =>
                {
                    var predictor = modelBuilder.MLContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(modelBuilder.Model);

                    var input = new SentimentIssue { Text = text };

                    var prediction = predictor.Predict(input);

                    var result = prediction.Prediction ? "Toxic" : "Non Toxic";

                    if (prediction.Prediction == expectedResult)
                    {
                        logger.LogInformation(
                            "[ClassifyAsync][Predict] result: '{0}' is {1} Probability of being toxic: {2}",
                            input.Text,
                            result,
                            prediction.Probability);
                    }
                    else
                    {
                        logger.LogWarning(
                           "[ClassifyAsync][Predict] result: '{0}' is {1} Probability of being toxic: {2}",
                           input.Text,
                           result,
                           prediction.Probability);
                    }
                },
                cancellationToken);
        }
    }
}
