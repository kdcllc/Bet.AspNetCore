using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.Data;
using Bet.Extensions.ML.ModelCreation;
using Bet.Extensions.ML.ModelCreation.DataLoaders;
using Bet.Extensions.ML.ModelCreation.Results;
using Bet.Extensions.ML.ModelCreation.Services;
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

            services.AddScoped(sp => modelStorageProvider);

            // adds source loader for embedded locations
            services.TryAddTransient<ISourceLoader<SentimentIssue>, EmbeddedSourceLoader<SentimentIssue>>();

            // configures custom logic for retrieving the data from embedded sources.
            services.Configure<EmbeddedSourceLoaderOptions<SentimentIssue>>(options =>
            {
                options.EmbeddedSourcesList.Add(new EmbeddedSources<SentimentIssue>
                {
                    // overrides default loading mechanism
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

            // adds IModelCreator for Sentiment ML.NET model.
            services.TryAddScoped<IModelDefinitionBuilder<SentimentIssue,
                BinaryClassificationMetricsResult>, ModelDefinitionBuilder<SentimentIssue, BinaryClassificationMetricsResult>>();

            // adds Configurations of for IModelCreator for Sentiment ML.NET model
            services.Configure<ModelDefinitionBuilderOptions<BinaryClassificationMetricsResult>>(options =>
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

            services.AddOptions<ModelStorageProviderOptions>(modelName)
                .Configure(x =>
            {
                x.ModelName = modelName;
                x.ModelResultFileName = $"{x.ModelName}.json";
                x.ModelFileName = $"{x.ModelName}.zip";
            });

            services.AddOptions<ModelCreationEngineOptions<SentimentIssue, BinaryClassificationMetricsResult>>()
                .Configure<ISourceLoader<SentimentIssue>>((options, loader) =>
            {
                options.ModelName = modelName;

                options.DataLoader = async (storageProvider, storageOptions, cancellationToken) =>
                {
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
                         SentimentModelEngineExtensions.ClassifyAsync(modelBuilder, "This is a very rude movie", false, logger, cancellationToken),
                         SentimentModelEngineExtensions.ClassifyAsync(modelBuilder, "Hate All Of You're Work", true, logger, cancellationToken)
                    };

                    await Task.WhenAll(tasks);
                };
            });

            services.AddScoped<IModelCreationEngine, ModelCreationEngine<SentimentIssue,
                BinaryClassificationMetricsResult, ModelCreationEngineOptions<SentimentIssue, BinaryClassificationMetricsResult>>>();

            services.TryAddScoped<IMachineLearningService, MachineLearningService>();

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
