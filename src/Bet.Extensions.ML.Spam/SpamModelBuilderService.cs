using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Bet.Hosting.Sample.Services
{
    public class SpamModelBuilderService : IModelBuilderService
    {
        private readonly ILogger<SpamModelBuilderService> _logger;
        private readonly IModelCreationBuilder<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult> _modelBuilder;
        private readonly IModelStorageProvider _storageProvider;
        private readonly object _lockObject = new object();

        public SpamModelBuilderService(
            IModelCreationBuilder<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult> spamModelBuilder,
            IModelStorageProvider storageProvider,
            ILogger<SpamModelBuilderService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modelBuilder = spamModelBuilder ?? throw new ArgumentNullException(nameof(spamModelBuilder));
            _storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
        }

        public async Task TrainModelAsync(CancellationToken cancellationToken)
        {
                _logger.LogInformation("TrainModelAsync][Started]");

                var sw = ValueStopwatch.StartNew();

                // 1. load default ML data set
                _logger.LogInformation("[LoadDataset][Started]");
                _modelBuilder.LoadDefaultData().BuiltDataView();
                _logger.LogInformation("[LoadDataset][Count]: {rowsCount} - elapsed time: {elapsed}",
                    _modelBuilder.DataView.GetRowCount(),
                    sw.GetElapsedTime().TotalMilliseconds);

                // 2. build training pipeline
                _logger.LogInformation("[BuildTrainingPipeline][Started]");
                var buildTrainingPipelineResult = _modelBuilder.BuildTrainingPipeline();
                _logger.LogInformation("[BuildTrainingPipeline][Ended] elapsed time: {elapsed}", buildTrainingPipelineResult.ElapsedMilliseconds);

                // 3. evaluate quality of the pipeline
                _logger.LogInformation("[Evaluate][Started]");
                var evaluateResult = _modelBuilder.Evaluate();
                _logger.LogInformation("[Evaluate][Ended] elapsed time: {elapsed}", evaluateResult.ElapsedMilliseconds);
                _logger.LogInformation(evaluateResult.ToString());

                var fileLocation = FileHelper.GetAbsolutePath($"{DateTime.UtcNow.Ticks}-spam-results.json");
                await _storageProvider.SaveResultsAsync(evaluateResult, fileLocation, cancellationToken);

                // 4. train the model
                _logger.LogInformation("[TrainModel][Started]");
                var trainModelResult = _modelBuilder.TrainModel();
                _logger.LogInformation("[TrainModel][Ended] elapsed time: {elapsed}", trainModelResult.ElapsedMilliseconds);

                _logger.LogInformation("[TrainModelAsync][Ended] elapsed time: {elapsed}", sw.GetElapsedTime().TotalMilliseconds);

            await Task.CompletedTask;
        }

        public async Task SaveModelAsync(CancellationToken cancellationToken)
        {
            // 6. save to the file
            _logger.LogInformation("[SaveModelAsync][Started]");

            var sw = ValueStopwatch.StartNew();

            var fileLocation = FileHelper.GetAbsolutePath("SpamModel.zip");

            var readStream = _modelBuilder.GetModelStream();

            await _storageProvider.SaveModelAsync(fileLocation, readStream, cancellationToken);

            _logger.LogInformation("[SaveModelAsync][Ended] elapsed time: {elapsed}", sw.GetElapsedTime().TotalMilliseconds);
        }

        public async Task ClassifyTestAsync(CancellationToken cancellationToken)
        {
            // 5. predict on sample data
            _logger.LogInformation("[ClassifyTestAsync][Started]");

            var sw = ValueStopwatch.StartNew();

            var predictor = _modelBuilder.MLContext.Model.CreatePredictionEngine<SpamInput, SpamPrediction>(_modelBuilder.Model);

            var tasks = new List<Task>
            {
                ClassifyAsync(predictor, "That's a great idea. It should work.","ham", cancellationToken),
                ClassifyAsync(predictor, "free medicine winner! congratulations","spam", cancellationToken),
                ClassifyAsync(predictor, "Yes we should meet over the weekend!", "ham", cancellationToken),
                ClassifyAsync(predictor, "you win pills and free entry vouchers", "spam", cancellationToken)
            };

            await Task.WhenAll(tasks);

            _logger.LogInformation("[ClassifyTestAsync][Ended] elapsed time: {elapsed}", sw.GetElapsedTime().TotalMilliseconds);
        }

        private Task ClassifyAsync(
            PredictionEngine<SpamInput, SpamPrediction> predictor,
            string text,
            string expectedResult,
            CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var input = new SpamInput { Message = text };

                SpamPrediction prediction = null;
                lock (_lockObject)
                {
                    prediction = predictor.Predict(input);
                }

                var result = prediction.IsSpam == "spam" ? "spam" : "not spam";

                if (prediction.IsSpam == expectedResult)
                {
                    _logger.LogInformation("[ClassifyAsync][Predict] result: '{0}' is {1}",  input.Message, result);
                }
                else
                {
                    _logger.LogWarning("[ClassifyAsync][Predict] result: '{0}' is {1}", input.Message, result);
                }
            },
            cancellationToken);
        }
    }
}
