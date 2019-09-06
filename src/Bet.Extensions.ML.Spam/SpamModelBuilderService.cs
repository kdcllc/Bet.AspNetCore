using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.ModelStorageProviders;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Bet.Extensions.ML.Spam
{
    public class SpamModelBuilderService : ModelBuilderService<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult>
    {
        private readonly ILogger _logger;
        private readonly IModelCreationBuilder<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult> _modelBuilder;
        private readonly IModelStorageProvider _storageProvider;
        private readonly object _lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="SpamModelBuilderService"/> class.
        /// </summary>
        /// <param name="spamModelBuilder"></param>
        /// <param name="storageProvider"></param>
        /// <param name="logger"></param>
        public SpamModelBuilderService(
            IModelCreationBuilder<SpamInput, SpamPrediction, MulticlassClassificationFoldsAverageMetricsResult> spamModelBuilder,
            IModelStorageProvider storageProvider,
            ILogger logger) : base(spamModelBuilder, storageProvider, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modelBuilder = spamModelBuilder ?? throw new ArgumentNullException(nameof(spamModelBuilder));
            _storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));

            Name = nameof(SpamModelBuilderService);
        }

        public override string Name { get; set; }

        public override async Task ClassifyTestAsync(CancellationToken cancellationToken)
        {
            // 5. predict on sample data
            _logger.LogInformation("[ClassifyTestAsync][Started]");

            var sw = ValueStopwatch.StartNew();

            var predictor = _modelBuilder.MLContext.Model.CreatePredictionEngine<SpamInput, SpamPrediction>(_modelBuilder.Model);

            var tasks = new List<Task>
            {
                ClassifyAsync(predictor, "That's a great idea. It should work.", "ham", cancellationToken),
                ClassifyAsync(predictor, "free medicine winner! congratulations", "spam", cancellationToken),
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
            return Task.Run(
                () =>
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
                        _logger.LogInformation("[ClassifyAsync][Predict] result: '{0}' is {1}", input.Message, result);
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
