using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.Data;
using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.ModelCreation;
using Bet.Extensions.ML.ModelCreation.DataLoaders;
using Bet.Extensions.ML.ModelStorageProviders;
using Bet.Extensions.ML.Sentiment.Models;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SentimentModelEngineExtensions
    {
        public static IServiceCollection AddSentimentModelEngine(
            this IServiceCollection services,
            string modelName = "SentimentModel",
            IModelStorageProvider? modelStorageProvider = default)
        {
            services.TryAddSingleton(new MLContext());

            if (modelStorageProvider == null)
            {
                modelStorageProvider = new FileModelStorageProvider();
            }

            services.AddScoped<IModelStorageProvider>(sp => modelStorageProvider);

            // adds embedded loader
            services.TryAddTransient<ISourceLoader<SentimentIssue>, EmbeddedSourceLoader<SentimentIssue>>();

            // configure the objects to get the data
            services.Configure<EmbeddedSourceLoaderOptions<SentimentIssue>>(options =>
            {
                options.EmbeddedSourcesList.Add(new EmbeddedSources<SentimentIssue>
                {
                    Overrides = () =>
                    {
                        var inputs = LoadFromEmbededResource
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

            // adds model creation engine
            services.TryAddScoped<IModelBuilder<SentimentIssue,
                Bet.Extensions.ML.ModelCreation.Results.BinaryClassificationMetricsResult>, ModelBuilder<SentimentIssue, Bet.Extensions.ML.ModelCreation.Results.BinaryClassificationMetricsResult>>();

            services.Configure<ModelBuilderOptions<Bet.Extensions.ML.ModelCreation.Results.BinaryClassificationMetricsResult>>(options =>
            {
                options.TrainingPipelineConfigurator = (mlContext) =>
                {
                    // STEP 2: Common data process configuration with pipeline data transformations
                    var dataProcessPipeline = mlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SentimentIssue.Text));

                    // STEP 3: Set the training algorithm, then create and config the modelBuilder
                    var trainer = mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features");
                    var trainingPipeline = dataProcessPipeline.Append(trainer);

                    return new Bet.Extensions.ML.ModelCreation.Results.TrainingPipelineResult(trainingPipeline, trainer.ToString());
                };

                options.EvaluateConfigurator = (mlContext, model, trainerName, dataView, _) =>
                {
                    // STEP 5: Evaluate the model and show accuracy stats
                    var predictions = model.Transform(dataView);
                    var metrics = mlContext.BinaryClassification.Evaluate(data: predictions, labelColumnName: "Label", scoreColumnName: "Score");

                    return new Bet.Extensions.ML.ModelCreation.Results.BinaryClassificationMetricsResult(trainerName, metrics);
                };
            });

            services.AddOptions<ModelStorageProviderOptions>(modelName).Configure(x =>
            {
                x.ModelName = modelName;
                x.ModelResultFileName = $"{x.ModelName}.json";
                x.ModelFileName = $"{x.ModelName}.zip";
            });

            services.Configure<ModelEngineOptions<SentimentIssue, Bet.Extensions.ML.ModelCreation.Results.BinaryClassificationMetricsResult>>(x =>
            {
                x.ModelName = modelName;

                x.TrainModelConfigurator = async (sw, modelBuilder, storageProvider, storageOptions, logger, cancellationToken) =>
                {
                    // 1. load default ML data set
                    logger.LogInformation("[LoadDataset][Started]");

                    modelBuilder.LoadAndBuildDataView();

                    if (modelBuilder?.DataView == null)
                    {
                        throw new NullReferenceException("DataView wasn't loaded");
                    }

                    logger.LogInformation(
                        "[LoadDataset][Count]: {rowsCount} - elapsed time: {elapsed}ms",
                        modelBuilder.DataView.GetRowCount(),
                        sw.GetElapsedTime().Milliseconds);

                    // 2. build training pipeline
                    logger.LogInformation("[BuildTrainingPipeline][Started]");
                    var buildTrainingPipelineResult = modelBuilder.BuildTrainingPipeline();
                    logger.LogInformation("[BuildTrainingPipeline][Ended] elapsed time: {elapsed}ms", buildTrainingPipelineResult.ElapsedMilliseconds);

                    // 3. train the model
                    logger.LogInformation("[TrainModel][Started]");
                    var trainModelResult = modelBuilder.TrainModel();
                    logger.LogInformation("[TrainModel][Ended] elapsed time: {elapsed}ms", trainModelResult.ElapsedMilliseconds);

                    // 4. evaluate quality of the pipeline
                    logger.LogInformation("[Evaluate][Started]");
                    var evaluateResult = modelBuilder.Evaluate();
                    logger.LogInformation("[Evaluate][Ended] elapsed time: {elapsed}ms", evaluateResult.ElapsedMilliseconds);
                    logger.LogInformation(evaluateResult.ToString());

                    // Save Results.
                    await storageProvider.SaveModelResultAsync(evaluateResult, storageOptions.ModelResultFileName, cancellationToken);

                    logger.LogInformation("[TrainModelAsync][Ended] elapsed time: {elapsed}ms", sw.GetElapsedTime().Milliseconds);
                    await Task.CompletedTask;
                };

                x.ClassifyTestConfigurator = async (modelBuilder, logger, cancellationToken) =>
                {
                    // 5. predict on sample data
                    logger.LogInformation("[ClassifyTestAsync][Started]");

                    var sw = ValueStopwatch.StartNew();

                    var predictor = modelBuilder.MLContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(modelBuilder.Model);

                    var tasks = new List<Task>
                    {
                         SentimentModelEngineExtensions.ClassifyAsync(predictor, "This is a very rude movie", false, logger, cancellationToken),
                         SentimentModelEngineExtensions.ClassifyAsync(predictor, "Hate All Of You're Work", true, logger, cancellationToken)
                    };

                    await Task.WhenAll(tasks);

                    logger.LogInformation("[ClassifyTestAsync][Ended] elapsed time: {elapsed} ms", sw.GetElapsedTime().TotalMilliseconds);
                };
            });

            services.AddScoped<IModelBuilderService, ModelEngine<SentimentIssue,
                Bet.Extensions.ML.ModelCreation.Results.BinaryClassificationMetricsResult, ModelEngineOptions<SentimentIssue, Bet.Extensions.ML.ModelCreation.Results.BinaryClassificationMetricsResult>>>();

            return services;
        }

        private static Task ClassifyAsync(
            PredictionEngine<SentimentIssue, SentimentPrediction> predictor,
            string text,
            bool expectedResult,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            return Task.Run(
                () =>
                {
                    var input = new SentimentIssue { Text = text };

                    SentimentPrediction? prediction = null;
                    prediction = predictor.Predict(input);

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
