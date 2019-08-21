using Bet.Extensions.ML.Data;
using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.ML;

namespace Bet.Extensions.ML.Spam
{
    /// <summary>
    /// https://github.com/dotnet/machinelearning-samples/blob/0ba74327e843c30eb02f01ca5d5d31ce77e84442/samples/csharp/getting-started/BinaryClassification_SpamDetection/SpamDetectionConsoleApp/Program.cs#L55.
    /// </summary>
    public class SpamModelBuilder
        : ModelCreationBuilder<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult>
    {
        public SpamModelBuilder(MLContext context) : base(context)
        {
        }

        public override IModelCreationBuilder<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult> LoadDefaultData()
        {
            Records = LoadFromEmbededResource.GetRecords<SpamInput>("Content.SpamDetectionData.csv", delimiter: ",");

            // Records = LoadFromEmbededResource.GetRecords<SpamInput>("Content.SMSSpamCollection.txt", delimiter: "\t", hasHeaderRecord: false);
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
                              .Append(MLContext.Transforms.Text.FeaturizeText(
                                 "FeaturesText",
                                 new Microsoft.ML.Transforms.Text.TextFeaturizingEstimator.Options
                                 {
                                     WordFeatureExtractor = new Microsoft.ML.Transforms.Text.WordBagEstimator.Options { NgramLength = 2, UseAllLengths = true },
                                     CharFeatureExtractor = new Microsoft.ML.Transforms.Text.WordBagEstimator.Options { NgramLength = 3, UseAllLengths = false }
                                 },
                                 "Message"))
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
