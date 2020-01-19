using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.ModelStorageProviders;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.ML.ModelCreation
{
    public class ModelCreationEngine<TInput, TResult, TOptions> : IModelCreationEngine
        where TInput : class
        where TResult : MetricsResult
        where TOptions : ModelCreationEngineOptions<TInput, TResult>
    {
        private readonly IModelDefinitionBuilder<TInput, TResult> _modelBuilder;
        private readonly IModelStorageProvider _storageProvider;
        private readonly ILogger<ModelCreationEngine<TInput, TResult, TOptions>> _logger;

        private TOptions _options;
        private ModelStorageProviderOptions _storageOptions;

        public ModelCreationEngine(
            IModelDefinitionBuilder<TInput, TResult> modelBuilder,
            IModelStorageProvider storageProvider,
            IOptionsMonitor<TOptions> optionsMonitor,
            IOptionsMonitor<ModelStorageProviderOptions> optionsStorageMonitor,
            ILogger<ModelCreationEngine<TInput, TResult, TOptions>> logger)
        {
            _modelBuilder = modelBuilder ?? throw new ArgumentNullException(nameof(modelBuilder));
            _storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _options = optionsMonitor.CurrentValue;
            optionsMonitor.OnChange(x => _options = x);

            _storageOptions = optionsStorageMonitor.Get(_options.ModelName);
        }

        public string Name => _options.ModelName;

        /// <summary>
        /// The following steps are executed in the pipeline:
        /// 1. LoadDefaultData().BuiltDataView()
        /// 2. BuildTrainingPipeline()
        /// 3. Evaluate()
        /// 4. SaveModelResultAsync()
        /// 5. TrainModel().
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task TrainModelAsync(CancellationToken cancellationToken)
        {
            var sw = ValueStopwatch.StartNew();

            _logger.LogInformation("[methodName][Started]", nameof(TrainModelAsync));

            var data = await _options.DataLoader(_storageProvider, _storageOptions, cancellationToken);

            var result = _options.TrainModelConfigurator(_modelBuilder, data, _logger);

            await _storageProvider.SaveModelResultAsync(result, _storageOptions.ModelResultFileName, cancellationToken);

            _logger.LogInformation("[methodName][Ended] elapsed time: {elapsed}ms", nameof(TrainModelAsync), sw.GetElapsedTime().TotalMilliseconds);
        }

        public async Task ClassifyTestAsync(CancellationToken cancellationToken)
        {
            var sw = ValueStopwatch.StartNew();

            _logger.LogInformation("[methodName][Started]", nameof(ClassifyTestAsync));

            if (_options.ClassifyTestConfigurator != null)
            {
                await _options.ClassifyTestConfigurator(_modelBuilder, _logger, cancellationToken);
            }

            _logger.LogInformation("[methodName][Ended] elapsed time: {elapsed}ms", nameof(ClassifyTestAsync), sw.GetElapsedTime().TotalMilliseconds);

            await Task.CompletedTask;
        }

        public async Task SaveModelAsync(CancellationToken cancellationToken)
        {
            // 6. save to the file
            var sw = ValueStopwatch.StartNew();

            _logger.LogInformation("[methodName][Started]", nameof(SaveModelAsync));

            var readStream = _modelBuilder.GetModelStream();
            await _storageProvider.SaveModelAsync(_storageOptions.ModelFileName, readStream, cancellationToken);

            _logger.LogInformation("[methodName][Ended] elapsed time: {elapsed}ms", nameof(SaveModelAsync), sw.GetElapsedTime().TotalMilliseconds);
        }
    }
}
