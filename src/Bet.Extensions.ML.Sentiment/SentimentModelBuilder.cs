using System;
using System.Collections.Generic;

using Bet.Extensions.ML.Data;
using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.Sentiment.Models;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Bet.Extensions.ML.Sentiment
{
    public class SentimentModelBuilder
        : ModelCreationBuilder<SentimentIssue, SentimentPrediction, BinaryClassificationMetricsResult>
    {
        private readonly ILogger<SentimentModelBuilder> _logger;

        public SentimentModelBuilder(
            MLContext context,
            ILogger<SentimentModelBuilder> logger)
        {
            MLContext = context ?? new MLContext();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override IModelCreationBuilder<SentimentIssue, SentimentPrediction, BinaryClassificationMetricsResult> LoadDefaultData()
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

            Records.AddRange(result);
            return this;
        }

        public override TrainingPipelineResult BuildTrainingPipeline()
        {
            return BuildTrainingPipeline(() =>
            {
                 // STEP 2: Common data process configuration with pipeline data transformations
                var dataProcessPipeline = MLContext.Transforms.Text.FeaturizeText(outputColumnName: "Features", inputColumnName: nameof(SentimentIssue.Text));

                // STEP 3: Set the training algorithm, then create and config the modelBuilder
                var trainer = MLContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features");
                var trainingPipeline = dataProcessPipeline.Append(trainer);

                return new TrainingPipelineResult(trainingPipeline, trainer.ToString());
            });
        }

        public override BinaryClassificationMetricsResult Evaluate()
        {
            return Evaluate((dataView, train) =>
            {
                if (Model == null)
                {
                    throw new ArgumentNullException(nameof(Model));
                }

                // STEP 5: Evaluate the model and show accuracy stats
                var predictions = Model.Transform(dataView);
                var metrics = MLContext.BinaryClassification.Evaluate(data: predictions, labelColumnName: "Label", scoreColumnName: "Score");

                return new BinaryClassificationMetricsResult(TrainerName, metrics);
            });
        }

        public override TrainModelResult TrainModel()
        {
            return TrainModel((dataView) =>
            {
                var model = TrainingPipeLine.Fit(dataView);

                return new TrainModelResult(model);
            });
        }
    }
}
