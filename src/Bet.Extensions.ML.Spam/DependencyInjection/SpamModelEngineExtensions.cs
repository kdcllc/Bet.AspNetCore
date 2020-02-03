using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.DataLoaders.ModelLoaders;
using Bet.Extensions.ML.DataLoaders.SourceLoaders;
using Bet.Extensions.ML.ModelCreation;
using Bet.Extensions.ML.ModelCreation.Results;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SpamModelEngineExtensions
    {
        public static IServiceCollection AddSpamModelCreationService<TModelLoader>(
            this IServiceCollection services,
            string modelName = "SpamModel",
            double testSlipFraction = 0.1)
                where TModelLoader : ModelLoader
        {
            // create builder for the named model
            var builder = services.AddModelCreationService<SpamInput, MulticlassClassificationFoldsAverageMetricsResult>(modelName);

            // 1. adds 2 embedded sources to source loader interface
            builder.AddSourceLoader<SpamInput, MulticlassClassificationFoldsAverageMetricsResult, EmbeddedSourceLoader<SpamInput>>(options =>
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

            // 2. adds model loader for saving the model and model results
            builder.AddModelLoader<SpamInput, MulticlassClassificationFoldsAverageMetricsResult, TModelLoader>();

            // 3. adds model creation configuration
            builder.ConfigureModelDefinition<SpamInput, MulticlassClassificationFoldsAverageMetricsResult>(
                testSlipFraction,
                options =>
                {
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

            // 4. adds model creation engine for testing as well.
            builder.ConfigureService<SpamInput, MulticlassClassificationFoldsAverageMetricsResult>(options =>
            {
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

            return builder.Services;
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
