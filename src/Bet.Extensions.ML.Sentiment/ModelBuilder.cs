using System.Collections.Generic;
using System.Diagnostics;

using Bet.Extensions.ML.Data;
using Bet.Extensions.ML.Sentiment.Models;

using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Calibrators;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

namespace Bet.Extensions.ML.Sentiment
{
    public class ModelBuilder<TInput, TOutput, TResult> : ModelCreationBuilder<TInput, TOutput, TResult>
        where TInput : class, new()
        where TOutput : class
        where TResult : class
    {
        public ModelBuilder(
            MLContext context = null,
            IEnumerable<TInput> inputs = null,
            ILogger logger = null) : base(context,inputs,logger)
        {
        }

        public TransformerChain<BinaryPredictionTransformer<CalibratedModelParametersBase<LinearBinaryModelParameters, PlattCalibrator>>> Model { get; private set; }

        public override void LoadData()
        {
            var inputs = LoadFromEmbededResource.GetRecords<InputSentimentIssueRow>("Content.wikiDetoxAnnotated40kRows.tsv", delimiter: "\t", hasHeaderRecord: true);

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
            _records.AddRange(result as List<TInput>);
        }

        public override TResult Train()
        {
            //Measure training time
            var watch = Stopwatch.StartNew();

            // STEP 1: Common data loading configuration
            var data = MlContext.Data.LoadFromEnumerable(_records);

            TrainingSchema = data.Schema;

            var trainTestSplit = MlContext.Data.TrainTestSplit(data, testFraction: 0.2);
            var trainingData = trainTestSplit.TrainSet;
            var testData = trainTestSplit.TestSet;

            // STEP 2: Common data process configuration with pipeline data transformations
            var dataProcessPipeline = MlContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SentimentIssue.Text));

            // STEP 3: Set the training algorithm, then create and config the modelBuilder
            var trainer = MlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features");
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // STEP 4: Train the model fitting to the DataSet
            Model = trainingPipeline.Fit(trainingData);

            // STEP 5: Evaluate the model and show accuracy stats
            var predictions = Model.Transform(testData);
            var metrics = MlContext.BinaryClassification.Evaluate(data: predictions, labelColumnName: "Label", scoreColumnName: "Score");

            var result = new Helpers.BinaryClassificationMetrics(trainer.ToString(), metrics);

            watch.Stop();

            result.ElapsedMilliseconds = watch.ElapsedMilliseconds;

            return result as TResult;
        }
    }
}
