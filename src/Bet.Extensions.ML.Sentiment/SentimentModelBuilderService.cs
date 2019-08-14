using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.Sentiment.Models;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Bet.Extensions.ML.Sentiment
{
    public class SentimentModelBuilderService : IModelBuilderService
    {
        private readonly IModelCreationBuilder<SentimentIssue, SentimentPrediction, BinaryClassificationMetricsResult> _modelBuilder;
        private readonly IModelStorageProvider _storageProvider;
        private readonly ILogger<SentimentModelBuilderService> _logger;
        private readonly object _lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="SentimentModelBuilderService"/> class.
        /// </summary>
        /// <param name="sentimentModelBuilder"></param>
        /// <param name="storageProvider"></param>
        /// <param name="logger"></param>
        public SentimentModelBuilderService(
            IModelCreationBuilder<SentimentIssue, SentimentPrediction, BinaryClassificationMetricsResult> sentimentModelBuilder,
            IModelStorageProvider storageProvider,
            ILogger<SentimentModelBuilderService> logger)
        {
            _modelBuilder = sentimentModelBuilder ?? throw new ArgumentNullException(nameof(sentimentModelBuilder));
            _storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Name = nameof(SentimentModelBuilderService);
        }

        public string Name { get; set; }

        public async Task TrainModelAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TrainModelAsync][Started]");

            var sw = ValueStopwatch.StartNew();

            // 1. load default ML data set
            _logger.LogInformation("[LoadDataset][Started]");
            _modelBuilder.LoadDefaultData().BuiltDataView();
            _logger.LogInformation(
                "[LoadDataset][Count]: {rowsCount} - elapsed time: {elapsed}",
                _modelBuilder.DataView.GetRowCount(),
                sw.GetElapsedTime().Milliseconds);

            // 2. build training pipeline
            _logger.LogInformation("[BuildTrainingPipeline][Started]");
            var buildTrainingPipelineResult = _modelBuilder.BuildTrainingPipeline();
            _logger.LogInformation("[BuildTrainingPipeline][Ended] elapsed time: {elapsed}", buildTrainingPipelineResult.ElapsedMilliseconds);

            // 3. train the model
            _logger.LogInformation("[TrainModel][Started]");
            var trainModelResult = _modelBuilder.TrainModel();
            _logger.LogInformation("[TrainModel][Ended] elapsed time: {elapsed}", trainModelResult.ElapsedMilliseconds);

            // 4. evaluate quality of the pipeline
            _logger.LogInformation("[Evaluate][Started]");
            var evaluateResult = _modelBuilder.Evaluate();
            _logger.LogInformation("[Evaluate][Ended] elapsed time: {elapsed}", evaluateResult.ElapsedMilliseconds);
            _logger.LogInformation(evaluateResult.ToString());

            await _storageProvider.SaveModelResultAsync(evaluateResult, Name, cancellationToken);

            _logger.LogInformation("[TrainModelAsync][Ended] elapsed time: {elapsed}", sw.GetElapsedTime().Milliseconds);

            await Task.CompletedTask;
        }

        public async Task SaveModelAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[SaveModelAsync][Started]");

            var sw = ValueStopwatch.StartNew();

            var readStream = _modelBuilder.GetModelStream();

            await _storageProvider.SaveModelAsync(Name, readStream, cancellationToken);

            _logger.LogInformation("[SaveModelAsync][Ended] elapsed time: {elapsed}", sw.GetElapsedTime().TotalMilliseconds);
        }

        public async Task ClassifyTestAsync(CancellationToken cancellationToken)
        {
            // 5. predict on sample data
            _logger.LogInformation("[ClassifyTestAsync][Started]");

            var sw = ValueStopwatch.StartNew();

            var predictor = _modelBuilder.MLContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(_modelBuilder.Model);

            var tasks = new List<Task>
            {
                 ClassifyAsync(predictor, "This is a very rude movie", false, cancellationToken),
                 ClassifyAsync(predictor, "Hate All Of You're Work", true, cancellationToken)
            };

            await Task.WhenAll(tasks);

            _logger.LogInformation("[ClassifyTestAsync][Ended] elapsed time: {elapsed}", sw.GetElapsedTime().TotalMilliseconds);
        }

        private Task ClassifyAsync(
            PredictionEngine<SentimentIssue, SentimentPrediction> predictor,
            string text,
            bool expectedResult,
            CancellationToken cancellationToken)
        {
            return Task.Run(
                () =>
                {
                    var input = new SentimentIssue { Text = text };

                    SentimentPrediction prediction = null;

                    lock (_lockObject)
                    {
                        prediction = predictor.Predict(input);
                    }

                    var result = prediction.Prediction ? "Toxic" : "Non Toxic";

                    if (prediction.Prediction == expectedResult)
                    {
                        _logger.LogInformation(
                            "[ClassifyAsync][Predict] result: '{0}' is {1} Probability of being toxic: {2}",
                            input.Text,
                            result,
                            prediction.Probability);
                    }
                    else
                    {
                        _logger.LogWarning(
                           "[ClassifyAsync][Predict] result: '{0}' is {1} Probability of being toxic: {2}",
                           input.Text,
                           result,
                           prediction.Probability);
                    }
                },
                cancellationToken);
        }
    }
}
