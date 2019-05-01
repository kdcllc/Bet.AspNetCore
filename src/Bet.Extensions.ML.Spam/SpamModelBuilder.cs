using System;
using System.Collections.Generic;
using System.Diagnostics;

using Bet.Extensions.ML.Data;
using Bet.Extensions.ML.ModelBuilder;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Bet.Extensions.ML.Spam
{
    public  class SpamModelBuilder<TInput, TOutput, TResult> : IModelCreationBuilder<TInput, TOutput, TResult>
        where TInput : class
        where TOutput : class
        where TResult : class
    {
        private readonly ILogger<SpamModelBuilder<TInput, TOutput, TResult>> _logger;
        private IEstimator<ITransformer> _trainingPipeLine;
        private string _trainerName;

        public SpamModelBuilder(MLContext context, ILogger<SpamModelBuilder<TInput, TOutput, TResult>> logger)
        {
            MLContext = context ?? new MLContext();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Records = new List<TInput>();
        }

        public List<TInput> Records { get; set; }

        public ITransformer Model { get; set; }

        public MLContext MLContext { get; set; }

        private IDataView _dataView;

        public DataViewSchema TrainingSchema { get; set; }

        public IModelCreationBuilder<TInput, TOutput, TResult> LoadDefaultData()
        {
            Records = LoadFromEmbededResource.GetRecords<TInput>("Content.SpamDetectionData.csv", delimiter: ",");

            var smsRecords = LoadFromEmbededResource.GetRecords<TInput>("Content.SMSSpamCollection.txt", delimiter: "\t", hasHeaderRecord: false);

            Records.AddRange(smsRecords);
            return this;
        }

        public IModelCreationBuilder<TInput, TOutput, TResult> LoadData(IEnumerable<TInput> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Records.AddRange(data);

            return this;
        }

        public IModelCreationBuilder<TInput, TOutput, TResult> BuiltDataView()
        {
            if (Records.Count > 0)
            {
                _dataView = MLContext.Data.LoadFromEnumerable(Records);
                TrainingSchema = _dataView.Schema;
            }
            else
            {
                throw new ArgumentException($"{nameof(_dataView)} doesn't have any records.");
            }

            return this;
        }

        public TrainingPipelineResult BuildTrainingPipeline()
        {
            return BuildTrainingPipeline(() =>
            {
                var sw = Stopwatch.StartNew();
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
                sw.Stop();
                return new TrainingPipelineResult(trainingPipeLine, sw.ElapsedMilliseconds, trainer.ToString());
            });
        }

        public TrainingPipelineResult BuildTrainingPipeline(Func<TrainingPipelineResult> builder)
        {
            var result = builder();

            _trainingPipeLine = result.TrainingPipeLine;
            _trainerName = result.TrainerName;
            return result;
        }

        public TrainModelResult TrainModel()
        {
            return TrainModel((dataView) =>
            {
                var sw = Stopwatch.StartNew();

                var model = _trainingPipeLine.Fit(_dataView);
                sw.Stop();

                return new TrainModelResult(model, sw.ElapsedMilliseconds);
            });
        }

        public TrainModelResult TrainModel(Func<IDataView,TrainModelResult> builder)
        {
            var result = builder(_dataView);

            Model = result.Model;

            return result;
        }

        public TResult Evaluate()
        {
            return Evaluate((dataView, train) =>
            {
                var sw = Stopwatch.StartNew();
                // Evaluate the model using cross-validation.
                // Cross-validation splits our dataset into 'folds', trains a model on some folds and
                // evaluates it on the remaining fold. We are using 5 folds so we get back 5 sets of scores.
                // Let's compute the average AUC, which should be between 0.5 and 1 (higher is better).
                var crossValidationResults = MLContext.MulticlassClassification.CrossValidate(data: _dataView, estimator: _trainingPipeLine, numberOfFolds: 5);
                var crossTrainingResults = new MulticlassClassificationFoldsAverageMetricsResult(_trainerName, crossValidationResults);

                sw.Stop();

                crossTrainingResults.ElapsedMilliseconds = sw.ElapsedMilliseconds;

                return crossTrainingResults as TResult;
            });
        }

        public TResult Evaluate(Func<IDataView, IEstimator<ITransformer>, TResult> builder)
        {
            return builder(_dataView, _trainingPipeLine);
        }
    }
}
