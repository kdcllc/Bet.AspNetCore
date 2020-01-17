using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.ModelStorageProviders;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.ML.ModelCreation
{
    public class ModelEngine<TInput, TResult, TOptions> : IModelBuilderService
        where TInput : class
        where TResult : MetricsResult
        where TOptions : ModelEngineOptions<TInput, TResult>
    {
        private readonly IModelBuilder<TInput, TResult> _modelBuilder;
        private readonly IModelStorageProvider _storageProvider;
        private readonly ILogger<ModelEngine<TInput, TResult, TOptions>> _logger;

        private TOptions _options;
        private ModelStorageProviderOptions _storageOptions;

        public ModelEngine(
            IModelBuilder<TInput, TResult> modelBuilder,
            IModelStorageProvider storageProvider,
            IOptionsMonitor<TOptions> optionsMonitor,
            IOptionsMonitor<ModelStorageProviderOptions> optionsStorageMonitor,
            ILogger<ModelEngine<TInput, TResult, TOptions>> logger)
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
            _logger.LogInformation("[TrainModelAsync][Started]");
            var sw = ValueStopwatch.StartNew();

            await _options.TrainModelConfigurator(sw, _modelBuilder, _storageProvider, _storageOptions, _logger, cancellationToken);
        }

        public async Task ClassifyTestAsync(CancellationToken cancellationToken)
        {
            if (_options.ClassifyTestConfigurator != null)
            {
                await _options.ClassifyTestConfigurator(_modelBuilder, _logger, cancellationToken);
            }

            await Task.CompletedTask;
        }

        public virtual async Task SaveModelAsync(CancellationToken cancellationToken)
        {
            // 6. save to the file
            _logger.LogInformation("[SaveModelAsync][Started]");
            var sw = ValueStopwatch.StartNew();

            var readStream = _modelBuilder.GetModelStream();
            await _storageProvider.SaveModelAsync(_storageOptions.ModelFileName, readStream, cancellationToken);

            _logger.LogInformation("[SaveModelAsync][Ended] elapsed time: {elapsed} ms", sw.GetElapsedTime().TotalMilliseconds);
        }
    }
}
