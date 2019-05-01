using System;
using System.IO;
using System.Threading.Tasks;

using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.Sentiment.Models;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Bet.Hosting.Sample.Services
{
    public class SentimentModelGeneratorService
    {
        private readonly IModelCreationBuilder<SentimentIssue, SentimentPrediction, BinaryClassificationMetricsResult> _modelBuilder;
        private readonly ModelPathService _pathService;
        private readonly ILogger<SentimentModelGeneratorService> _logger;

        public SentimentModelGeneratorService(
            IModelCreationBuilder<SentimentIssue, SentimentPrediction, BinaryClassificationMetricsResult> sentimentModelBuilder,
            ModelPathService pathService,
            ILogger<SentimentModelGeneratorService> logger)
        {
            _modelBuilder = sentimentModelBuilder ?? throw new ArgumentNullException(nameof(sentimentModelBuilder));
            _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public Task GenerateModel()
        {
            // 1. load default ML data set
            _logger.LogInformation("=============== Loading data===============");
            _modelBuilder.LoadDefaultData()
                .BuiltDataView();

            // 2. build training pipeline
            _logger.LogInformation("=============== BuildTrainingPipeline ===============");
            var buildTrainingPipelineResult = _modelBuilder.BuildTrainingPipeline();
            _logger.LogInformation("BuildTrainingPipeline ran for: {BuildTrainingPipelineTime}", buildTrainingPipelineResult.ElapsedMilliseconds);

            // 3. evaluate quality of the pipeline
            _logger.LogInformation("=============== Evaluate ===============");
            var evaluateResult = _modelBuilder.Evaluate();
            _logger.LogInformation("Evaluate ran for {EvaluateTime}", evaluateResult.ElapsedMilliseconds);
            _logger.LogInformation(evaluateResult.ToString());

            // 4. train the model
            _logger.LogInformation("=============== TrainModel ===============");
            var trainModelResult = _modelBuilder.TrainModel();
            _logger.LogInformation("TrainModel ran for {TrainModelTime}", trainModelResult.ElapsedMilliseconds);

            // 5. predict on sample data
            _logger.LogInformation("=============== Predictions for below data===============");
            var predictor = _modelBuilder.MLContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(_modelBuilder.Model);

            ClassifySentimentText(predictor, "This is a very rude movie");
            ClassifySentimentText(predictor, "Hate All Of You're Work");

            Console.WriteLine("=================== Saving Model to Disk ============================ ");

            using (var fs = new FileStream(_pathService.SpamModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                _modelBuilder.MLContext.Model.Save(_modelBuilder.Model, _modelBuilder.TrainingSchema, fs);
            }

            Console.WriteLine("======================= Creating Model Completed ================== ");

            return Task.CompletedTask;
        }

        public void ClassifySentimentText(PredictionEngine<SentimentIssue, SentimentPrediction> predictor, string text)
        {
            var input = new SentimentIssue { Text = text };
            var prediction = predictor.Predict(input);
            _logger.LogInformation("The text '{0}' is {1} Probability of being toxic: {2}", input.Text, Convert.ToBoolean(prediction.Prediction) ? "Toxic" : "Non Toxic", prediction.Probability);
        }
    }
}
