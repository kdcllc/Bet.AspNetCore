using System;

using Bet.Extensions.ML.Data;
using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Bet.Extensions.ML.Spam
{
    public class SpamModelBuilder
        : ModelCreationBuilder<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult>
    {
        private readonly ILogger<SpamModelBuilder> _logger;

        public SpamModelBuilder(
            MLContext context,
            ILogger<SpamModelBuilder> logger)
        {
            MLContext = context ?? new MLContext();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override IModelCreationBuilder<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult> LoadDefaultData()
        {
            Records = LoadFromEmbededResource.GetRecords<SpamInput>("Content.SpamDetectionData.csv", delimiter: ",");

            var smsRecords = LoadFromEmbededResource.GetRecords<SpamInput>("Content.SMSSpamCollection.txt", delimiter: "\t", hasHeaderRecord: false);

            Records.AddRange(smsRecords);
            return this;
        }

        public override TrainingPipelineResult BuildTrainingPipeline()
        {
            return BuildTrainingPipeline(() =>
            {
                // Create the estimator which converts the text label to boolean,
                // then featurizes the text, and adds a linear trainer.
                // Data process configuration with pipeline data transformations

                var dataProcessPipeline = MLContext.Transforms.Conversion.MapValueToKey("Label", "Label")
                              .Append(MLContext.Transforms.Text.FeaturizeText("FeaturesText", new Microsoft.ML.Transforms.Text.TextFeaturizingEstimator.Options
                              {
                                  WordFeatureExtractor = new Microsoft.ML.Transforms.Text.WordBagEstimator.Options { NgramLength = 2, UseAllLengths = true },
                                  CharFeatureExtractor = new Microsoft.ML.Transforms.Text.WordBagEstimator.Options { NgramLength = 3, UseAllLengths = false },
                              }, "Message"))
                              .Append(MLContext.Transforms.CopyColumns("Features", "FeaturesText"))
                              .Append(MLContext.Transforms.NormalizeLpNorm("Features", "Features"))
                              .AppendCacheCheckpoint(MLContext);

                // Set the training algorithm
                var trainer = MLContext.MulticlassClassification.Trainers
                                        .OneVersusAll(MLContext.BinaryClassification.Trainers.AveragedPerceptron(labelColumnName: "Label", numberOfIterations: 10, featureColumnName: "Features"), labelColumnName: "Label")
                                        .Append(MLContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel"));

                var trainingPipeLine = dataProcessPipeline.Append(trainer);

                // TRAINER NAME???
                return new TrainingPipelineResult(trainingPipeLine, trainer.ToString());
            });
        }

        public override MulticlassClassificationFoldsAverageMetricsResult Evaluate()
        {
            return Evaluate((dataView, trainingPipeLine) =>
            {
                // Evaluate the model using cross-validation.
                // Cross-validation splits our dataset into 'folds', trains a model on some folds and
                // evaluates it on the remaining fold. We are using 5 folds so we get back 5 sets of scores.
                // Let's compute the average AUC, which should be between 0.5 and 1 (higher is better).
                var crossValidationResults = MLContext.MulticlassClassification.CrossValidate(data: dataView, estimator: trainingPipeLine, numberOfFolds: 5);

                return new MulticlassClassificationFoldsAverageMetricsResult(TrainerName, crossValidationResults);
            });
        }
    }
}
