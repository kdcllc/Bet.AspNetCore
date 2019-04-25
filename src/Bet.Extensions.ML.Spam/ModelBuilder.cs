using System.Collections.Generic;
using System.Diagnostics;
using Bet.Extensions.ML.Data;
using Bet.Extensions.ML.Helpers;

using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;

namespace Bet.Extensions.ML.Spam
{
    public  class ModelBuilder<TInput, TOutput, TResult> : ModelCreationBuilder<TInput, TOutput, TResult>
        where TInput : class
        where TOutput : class
        where TResult : class
    {
        public TransformerChain<TransformerChain<KeyToValueMappingTransformer>> Model;

        public ModelBuilder(
            MLContext context = null,
            IEnumerable<TInput> inputs = null,
            ILogger logger = null) : base(context,inputs,logger)
        { }

        public override void LoadData()
        {
            _records = LoadFromEmbededResource.GetRecords<TInput>("Content.SpamDetectionData.csv", delimiter: ",");

            var smsRecords = LoadFromEmbededResource.GetRecords<TInput>("Content.SMSSpamCollection.txt", delimiter: "\t", hasHeaderRecord: false);

            _records.AddRange(smsRecords);
        }

        public override TResult Train()
        {
            //Measure training time
            var watch = Stopwatch.StartNew();

            var data = MlContext.Data.LoadFromEnumerable(_records);

            TrainingSchema = data.Schema;

            // Create the estimator which converts the text label to boolean,
            // then featurizes the text, and adds a linear trainer.
            // Data process configuration with pipeline data transformations

            var dataProcessPipeline = MlContext.Transforms.Conversion.MapValueToKey("Label", "Label")
                          .Append(MlContext.Transforms.Text.FeaturizeText("FeaturesText", new Microsoft.ML.Transforms.Text.TextFeaturizingEstimator.Options
                          {
                              WordFeatureExtractor = new Microsoft.ML.Transforms.Text.WordBagEstimator.Options { NgramLength = 2, UseAllLengths = true },
                              CharFeatureExtractor = new Microsoft.ML.Transforms.Text.WordBagEstimator.Options { NgramLength = 3, UseAllLengths = false },
                          }, "Message"))
                          .Append(MlContext.Transforms.CopyColumns("Features", "FeaturesText"))
                          .Append(MlContext.Transforms.NormalizeLpNorm("Features", "Features"))
                          .AppendCacheCheckpoint(MlContext);

            // Set the training algorithm
            var trainer = MlContext.MulticlassClassification.Trainers
                                    .OneVersusAll(MlContext.BinaryClassification.Trainers.AveragedPerceptron(labelColumnName: "Label", numberOfIterations: 10, featureColumnName: "Features"), labelColumnName: "Label")
                                    .Append(MlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel"));

            var trainingPipeLine = dataProcessPipeline.Append(trainer);

            // Evaluate the model using cross-validation.
            // Cross-validation splits our dataset into 'folds', trains a model on some folds and
            // evaluates it on the remaining fold. We are using 5 folds so we get back 5 sets of scores.
            // Let's compute the average AUC, which should be between 0.5 and 1 (higher is better).
            var crossValidationResults = MlContext.MulticlassClassification.CrossValidate(data: data, estimator: trainingPipeLine, numberOfFolds: 5);
            var crossTrainingResults = new MulticlassClassificationFoldsAverageMetrics(trainer.ToString(), crossValidationResults);

            // Now let's train a model on the full dataset to help us get better results
            Model = trainingPipeLine.Fit(data);

            watch.Stop();

            crossTrainingResults.ElapsedMilliseconds = watch.ElapsedMilliseconds;

            return crossTrainingResults as TResult;
        }
    }
}
