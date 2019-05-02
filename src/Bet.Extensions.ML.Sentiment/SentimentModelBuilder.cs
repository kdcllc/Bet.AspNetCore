using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Bet.Extensions.ML.Data;
using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.Sentiment.Models;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Bet.Extensions.ML.Sentiment
{
    public class SentimentModelBuilder<TInput, TOutput, TResult> : IModelCreationBuilder<TInput, TOutput, TResult>
        where TInput : class
        where TOutput : class
        where TResult : class
    {
        private readonly ILogger<SentimentModelBuilder<TInput, TOutput, TResult>> _logger;
        private IEstimator<ITransformer> _trainingPipeLine;
        private string _trainerName;

        public List<TInput> Records { get; set; }

        public ITransformer Model { get; set; }

        public MLContext MLContext { get; set; }

        private IDataView _dataView;
        private IDataView _trainingDataView;
        private IDataView _testDataView;

        public DataViewSchema TrainingSchema { get; set; }

        public SentimentModelBuilder(
            MLContext context,
            ILogger<SentimentModelBuilder<TInput, TOutput, TResult>> logger)
        {
            MLContext = context ?? new MLContext();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Records = new List<TInput>();
        }

        public IModelCreationBuilder<TInput, TOutput, TResult> LoadDefaultData()
        {
            var inputs = LoadFromEmbededResource.GetRecords<InputSentimentIssueRow>("Content.wikiDetoxAnnotated40kRows.tsv", delimiter: "\t", hasHeaderRecord: true);

            // convert int to boolean values
            var result = new List<SentimentIssue>();
            foreach (var item in inputs)
            {
                var newItem = new SentimentIssue
                {
                    Label = item.Label == 0 ? false : true,
                    Text = item.comment
                };

                result.Add(newItem);
            }

            Records.AddRange(result as List<TInput>);
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

                var trainTestSplit = MLContext.Data.TrainTestSplit(_dataView, testFraction: 0.2);
                _trainingDataView = trainTestSplit.TrainSet;
                _testDataView = trainTestSplit.TestSet;
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
                 // STEP 2: Common data process configuration with pipeline data transformations
                var dataProcessPipeline = MLContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SentimentIssue.Text));

                // STEP 3: Set the training algorithm, then create and config the modelBuilder
                var trainer = MLContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features");
                var trainingPipeline = dataProcessPipeline.Append(trainer);

                sw.Stop();
                return new TrainingPipelineResult(trainingPipeline, sw.ElapsedMilliseconds, trainer.ToString());
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

                var model = _trainingPipeLine.Fit(dataView);
                sw.Stop();

                return new TrainModelResult(model, sw.ElapsedMilliseconds);
            });
        }

        public TrainModelResult TrainModel(Func<IDataView, TrainModelResult> builder)
        {
            var result = builder(_trainingDataView);

            Model = result.Model;

            return result;
        }

        public TResult Evaluate()
        {
            return Evaluate((dataView, train) =>
            {
                if (Model == null)
                {
                    throw new ArgumentNullException(nameof(Model));
                }

                var sw = Stopwatch.StartNew();
                // STEP 5: Evaluate the model and show accuracy stats
                var predictions = Model.Transform(dataView);
                var metrics = MLContext.BinaryClassification.Evaluate(data: predictions, labelColumnName: "Label", scoreColumnName: "Score");

                var result = new BinaryClassificationMetricsResult(_trainerName, metrics);

                sw.Stop();

                result.ElapsedMilliseconds = sw.ElapsedMilliseconds;

                return result as TResult;
            });
        }

        public TResult Evaluate(Func<IDataView, IEstimator<ITransformer>, TResult> builder)
        {
            return builder(_testDataView, _trainingPipeLine);
        }

        public void SaveModel(string modelRelativePath)
        {
            SaveModel((mlContext,mlModel, path, modelInputSchema) =>
            {
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    mlContext.Model.Save(mlModel, modelInputSchema, fs);
                }
            }, modelRelativePath);
        }

        public void SaveModel(Action<MLContext, ITransformer, string, DataViewSchema> builder, string modelRelativePath)
        {
            builder(MLContext, Model, modelRelativePath, TrainingSchema);
        }
    }
}
