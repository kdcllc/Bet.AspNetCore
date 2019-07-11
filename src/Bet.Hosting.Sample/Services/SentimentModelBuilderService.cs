using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.Sentiment.Models;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Bet.Hosting.Sample.Services
{
    public class SentimentModelBuilderService : IModelBuilderService
    {
        private readonly IModelCreationBuilder<SentimentIssue, SentimentPrediction, BinaryClassificationMetricsResult> _modelBuilder;
        private readonly ModelPathService _pathService;
        private readonly ILogger<SentimentModelBuilderService> _logger;
        private PredictionEngine<SentimentIssue, SentimentPrediction> _predictor;

        public SentimentModelBuilderService(
            IModelCreationBuilder<SentimentIssue, SentimentPrediction, BinaryClassificationMetricsResult> sentimentModelBuilder,
            ModelPathService pathService,
            ILogger<SentimentModelBuilderService> logger)
        {
            _modelBuilder = sentimentModelBuilder ?? throw new ArgumentNullException(nameof(sentimentModelBuilder));
            _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task TrainModel()
        {
            var sw = ValueStopwatch.StartNew();

            // 1. load default ML data set
            _logger.LogInformation("=============== Loading data===============");
            _modelBuilder.LoadDefaultData().BuiltDataView();

            // 2. build training pipeline
            _logger.LogInformation("=============== BuildTrainingPipeline ===============");
            var buildTrainingPipelineResult = _modelBuilder.BuildTrainingPipeline();
            _logger.LogInformation("BuildTrainingPipeline ran for: {BuildTrainingPipelineTime}", buildTrainingPipelineResult.ElapsedMilliseconds);

            // 3. train the model
            _logger.LogInformation("=============== TrainModel ===============");
            var trainModelResult = _modelBuilder.TrainModel();
            _logger.LogInformation("TrainModel ran for {TrainModelTime}", trainModelResult.ElapsedMilliseconds);

            // 4. evaluate quality of the pipeline
            _logger.LogInformation("=============== Evaluate ===============");
            var evaluateResult = _modelBuilder.Evaluate();
            _logger.LogInformation("Evaluate ran for {EvaluateTime}", evaluateResult.ElapsedMilliseconds);
            _logger.LogInformation(evaluateResult.ToString());

            _logger.LogInformation("Elapsed time {elapsed}", sw.GetElapsedTime());

            return Task.CompletedTask;
        }


        public void ClassifySample()
        {
            // 5. predict on sample data
            _logger.LogInformation("=============== Predictions for below data===============");

            _predictor = _modelBuilder.MLContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(_modelBuilder.Model);

            Classify("This is a very rude movie");
            Classify("Hate All Of You're Work");
        }

        public void SaveModel()
        {
            _logger.LogInformation("=================== Saving Model to Disk ============================ ");

            _modelBuilder.SaveModel(_pathService.SentimentModelPath);

            _logger.LogInformation("======================= Creating Model Completed ================== ");
        }


        private void Classify(string text)
        {
            var input = new SentimentIssue { Text = text };
            var prediction = _predictor.Predict(input);

            _logger.LogInformation(
                "The text '{0}' is {1} Probability of being toxic: {2}",
                input.Text,
                Convert.ToBoolean(prediction.Prediction) ? "Toxic" : "Non Toxic",
                prediction.Probability);
        }
    }
}
