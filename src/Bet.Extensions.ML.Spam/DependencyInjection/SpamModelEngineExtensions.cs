using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.DataLoaders.ModelLoaders;
using Bet.Extensions.ML.DataLoaders.SourceLoaders;
using Bet.Extensions.ML.DataLoaders.SourceLoaders.Embedded;
using Bet.Extensions.ML.ModelCreation;
using Bet.Extensions.ML.ModelCreation.Results;
using Bet.Extensions.ML.ModelCreation.Services;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SpamModelEngineExtensions
    {
        public static IServiceCollection AddSpamModelEngine(
            this IServiceCollection services,
            string modelName = "SpamModel",
            double testSlipFraction = 0.1)
        {
            services.TryAddSingleton(new MLContext());

            // adds 2 embedded sources to source loader interface
            services
                .Configure<SourceLoaderFileOptions<SpamInput>>(
                modelName,
                options =>
                {
                    options.Sources.Add(new SourceLoaderFile<SpamInput>
                    {
                        FileName = "Content.SpamDetectionData.csv",
                        Delimiter = ",",
                        HasHeaderRecord = true
                    });

                    options.Sources.Add(new SourceLoaderFile<SpamInput>
                    {
                        FileName = "Content.SMSSpamCollection.txt",
                        Delimiter = "\t",
                        HasHeaderRecord = false
                    });
                });

            services.TryAddTransient<EmbeddedSourceLoader<SpamInput>, EmbeddedSourceLoader<SpamInput>>();

            services
                .AddOptions<SourceLoaderOptions<SpamInput>>(modelName)
                .Configure<IServiceProvider, EmbeddedSourceLoader<SpamInput>>(
                (options, sp, loader) =>
                {
                    var setupOptions = sp.GetRequiredService<IOptionsMonitor<SourceLoaderFileOptions<SpamInput>>>().Get(modelName);
                    loader.Setup(setupOptions);
                    options.SourceLoader = loader;
                });

            services.TryAddTransient<FileModelLoader, FileModelLoader>();

            services.Configure<ModelLoderFileOptions>(
                modelName,
                options =>
                {
                    options.ModelName = modelName;
                    options.ModelResultFileName = $"{options.ModelName}.json";
                    options.ModelFileName = $"{options.ModelName}.zip";
                });

            services
                .AddOptions<ModelLoaderOptions>(modelName)
                .Configure<IServiceProvider, FileModelLoader>(
                (options, sp, loader) =>
                {
                    var setupOptions = sp.GetRequiredService<IOptionsMonitor<ModelLoderFileOptions>>().Get(modelName);
                    loader.Setup(setupOptions);

                    options.ModalLoader = loader;
                });

            // adds Configurations for IModelDefinitionBuilder for Spam ML.NET model
            services.Configure<ModelDefinitionBuilderOptions<MulticlassClassificationFoldsAverageMetricsResult>>(
                    modelName,
                    options =>
                    {
                        options.ModelName = modelName;

                        options.TestSlipFraction = testSlipFraction;

                        options.TrainingPipelineConfigurator = (mlContext) =>
                        {
                            // Create the estimator which converts the text label to boolean,
                            // then featurizes the text, and adds a linear trainer.
                            // Data process configuration with pipeline data transformations
                            var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", "Label")
                                          .Append(mlContext.Transforms.Text.FeaturizeText(
                                             "FeaturesText",
                                             new Microsoft.ML.Transforms.Text.TextFeaturizingEstimator.Options
                                             {
                                                 WordFeatureExtractor = new Microsoft.ML.Transforms.Text.WordBagEstimator.Options { NgramLength = 2, UseAllLengths = true },
                                                 CharFeatureExtractor = new Microsoft.ML.Transforms.Text.WordBagEstimator.Options { NgramLength = 3, UseAllLengths = false }
                                             },
                                             "Message"))
                                          .Append(mlContext.Transforms.CopyColumns("Features", "FeaturesText"))
                                          .Append(mlContext.Transforms.NormalizeLpNorm("Features", "Features"))
                                          .AppendCacheCheckpoint(mlContext);

                            // Set the training algorithm
                            var trainer = mlContext.MulticlassClassification.Trainers
                                                    .OneVersusAll(mlContext.BinaryClassification.Trainers.AveragedPerceptron(labelColumnName: "Label", numberOfIterations: 10, featureColumnName: "Features"), labelColumnName: "Label")
                                                    .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel"));

                            var trainingPipeLine = dataProcessPipeline.Append(trainer);

                            // TRAINER NAME???
                            return new TrainingPipelineResult(trainingPipeLine, trainer.ToString());
                        };

                        options.EvaluateConfigurator = (mlContext, model, trainerName, dataView, trainingPipeLine) =>
                        {
                            // Evaluate the model using cross-validation.
                            // Cross-validation splits our dataset into 'folds', trains a model on some folds and
                            // evaluates it on the remaining fold. We are using 5 folds so we get back 5 sets of scores.
                            // Let's compute the average AUC, which should be between 0.5 and 1 (higher is better).
                            var crossValidationResults = mlContext.MulticlassClassification.CrossValidate(data: dataView, estimator: trainingPipeLine, numberOfFolds: 5);

                            return new MulticlassClassificationFoldsAverageMetricsResult(trainerName, crossValidationResults);
                        };
                    });

            // adds IModelDefinitionBuilder for Spam ML.NET model.
            services.AddTransient<IModelDefinitionBuilder<SpamInput, MulticlassClassificationFoldsAverageMetricsResult>>(sp =>
            {
                var mlContext = sp.GetRequiredService<MLContext>();
                var options = sp.GetRequiredService<IOptionsMonitor<ModelDefinitionBuilderOptions<MulticlassClassificationFoldsAverageMetricsResult>>>().Get(modelName);
                var logger = sp.GetRequiredService<ILogger<ModelDefinitionBuilder<SpamInput, MulticlassClassificationFoldsAverageMetricsResult>>>();

                return new ModelDefinitionBuilder<SpamInput, MulticlassClassificationFoldsAverageMetricsResult>(mlContext, options, logger);
            });

            services.AddOptions<ModelCreationEngineOptions<SpamInput, MulticlassClassificationFoldsAverageMetricsResult>>(modelName)
                    .Configure(options =>
                    {
                        options.ModelName = modelName;

                        options.DataLoader = async (loader, cancellationToken) =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            var data = loader.LoadData();
                            return await Task.FromResult(data);
                        };

                        options.ClassifyTestConfigurator = async (modelBuilder, logger, cancellationToken) =>
                        {
                            // var tasks = new List<Task>
                            // {
                            //    SpamModelEngineExtensions.ClassifyAsync(modelBuilder, "That's a great idea. It should work.", "ham", logger, cancellationToken),
                            //    SpamModelEngineExtensions.ClassifyAsync(modelBuilder, "free medicine winner! congratulations", "spam", logger, cancellationToken),
                            //    SpamModelEngineExtensions.ClassifyAsync(modelBuilder, "Yes we should meet over the weekend!", "ham", logger, cancellationToken),
                            //    SpamModelEngineExtensions.ClassifyAsync(modelBuilder, "you win pills and free entry vouchers", "spam", logger, cancellationToken)
                            // };

                            // await Task.WhenAll(tasks);

                            // start: batch predict
                            var batchPrediction = new List<SpamInput>
                            {
                                new SpamInput
                                {
                                    Message = "That's a great idea. It should work",
                                    Label = "ham"
                                },

                                new SpamInput
                                {
                                    Message = "free medicine winner! congratulations",
                                    Label = "spam"
                                },

                                new SpamInput
                                {
                                    Message = "Yes we should meet over the weekend!",
                                    Label = "ham"
                                },
                                new SpamInput
                                {
                                    Message = "you win pills and free entry vouchers",
                                    Label = "spam"
                                }
                            };

                            var data = modelBuilder.MLContext.Data.LoadFromEnumerable(batchPrediction);

                            var batchDataView = modelBuilder.Model!.Transform(data);

                            var predictions = modelBuilder.MLContext.Data.CreateEnumerable<SpamPrediction>(batchDataView, reuseRowObject: false).ToList();

                            for (var i = 0; i < batchPrediction.Count; i++)
                            {
                                var prediction = predictions[i];

                                var result = prediction.IsSpam == "spam" ? "spam" : "not spam";

                                if (prediction.IsSpam == batchPrediction[i].Label)
                                {
                                    logger.LogInformation("[ClassifyAsync][Predict] result: '{0}' is {1}", batchPrediction[i].Message, result);
                                }
                                else
                                {
                                    logger.LogWarning("[ClassifyAsync][Predict] result: '{0}' is {1}", batchPrediction[i].Message, result);
                                }
                            }

                            await Task.CompletedTask;
                        };
                    });

            services.TryAddTransient<IModelCreationEngine, ModelCreationEngine<SpamInput,
                    MulticlassClassificationFoldsAverageMetricsResult, ModelCreationEngineOptions<SpamInput, MulticlassClassificationFoldsAverageMetricsResult>>>();

            services.TryAddScoped<IModelCreationService, ModelCreationService>();

            return services;
        }

        private static Task ClassifyAsync(
            IModelDefinitionBuilder<SpamInput, MulticlassClassificationFoldsAverageMetricsResult> modelBuilder,
            string text,
            string expectedResult,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            return Task.Run(
               () =>
               {
                   var predictor = modelBuilder.MLContext.Model.CreatePredictionEngine<SpamInput, SpamPrediction>(modelBuilder.Model);

                   var input = new SpamInput { Message = text };

                   var prediction = predictor.Predict(input);

                   var result = prediction.IsSpam == "spam" ? "spam" : "not spam";

                   if (prediction.IsSpam == expectedResult)
                   {
                       logger.LogInformation("[ClassifyAsync][Predict] result: '{0}' is {1}", input.Message, result);
                   }
                   else
                   {
                       logger.LogWarning("[ClassifyAsync][Predict] result: '{0}' is {1}", input.Message, result);
                   }
               },
               cancellationToken);
        }
    }
}
