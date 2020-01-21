using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.DataLoaders.ModelLoaders;
using Bet.Extensions.ML.DataLoaders.SourceLoaders;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.ML.ModelCreation
{
    public class ModelCreationEngine<TInput, TResult, TOptions> : IModelCreationEngine
        where TInput : class
        where TResult : MetricsResult
        where TOptions : ModelCreationEngineOptions<TInput, TResult>
    {
        private readonly IEnumerable<IModelDefinitionBuilder<TInput, TResult>> _modelBuilders;
        private readonly ILogger<ModelCreationEngine<TInput, TResult, TOptions>> _logger;
        private readonly IOptionsFactory<SourceLoaderOptions<TInput>> _sourceLoaderOptionsFactory;
        private readonly IOptionsMonitor<TOptions> _engineOptionsMonitor;
        private readonly IOptionsFactory<ModelLoaderOptions> _modelLoaderOptions;

        public ModelCreationEngine(
            IEnumerable<IModelDefinitionBuilder<TInput, TResult>> modelBuilders,
            IOptionsFactory<SourceLoaderOptions<TInput>> sourceLoaderOptionsFactory,
            IOptionsMonitor<TOptions> engineOptionsMonitor,
            IOptionsFactory<ModelLoaderOptions> modelLoaderOptionsFactory,
            ILogger<ModelCreationEngine<TInput, TResult, TOptions>> logger)
        {
            _modelBuilders = modelBuilders ?? throw new ArgumentNullException(nameof(modelBuilders));
            _sourceLoaderOptionsFactory = sourceLoaderOptionsFactory ?? throw new ArgumentNullException(nameof(sourceLoaderOptionsFactory));
            _engineOptionsMonitor = engineOptionsMonitor;
            _modelLoaderOptions = modelLoaderOptionsFactory;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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
            foreach (var modelBuilder in _modelBuilders)
            {
                var sw = ValueStopwatch.StartNew();

                var engineOptions = _engineOptionsMonitor.Get(modelBuilder.ModelName);
                var sourceLoaderOptions = _sourceLoaderOptionsFactory.Create(modelBuilder.ModelName);
                var modelLoaderOptions = _modelLoaderOptions.Create(modelBuilder.ModelName);

                _logger.LogInformation("[{methodName}][Started] Model:{modelName}", nameof(TrainModelAsync), modelBuilder.ModelName);

                var data = await engineOptions.DataLoader(sourceLoaderOptions.SourceLoader, cancellationToken);

                var result = engineOptions.TrainModelConfigurator(modelBuilder, data, _logger);

                await modelLoaderOptions.ModalLoader.SaveModelResultAsync(result, cancellationToken);

                _logger.LogInformation(
                    "[{methodName}][Ended] Model:{modelName} - elapsed time: {elapsed}ms",
                    nameof(TrainModelAsync),
                    modelBuilder.ModelName,
                    sw.GetElapsedTime().TotalMilliseconds);
            }
        }

        public async Task ClassifyTestAsync(CancellationToken cancellationToken)
        {
            foreach (var modelBuilder in _modelBuilders)
            {
                var sw = ValueStopwatch.StartNew();

                var engineOptions = _engineOptionsMonitor.Get(modelBuilder.ModelName);

                _logger.LogInformation("[{methodName}][Started] Model:{modelName}", nameof(ClassifyTestAsync), modelBuilder.ModelName);

                if (engineOptions.ClassifyTestConfigurator != null)
                {
                    await engineOptions.ClassifyTestConfigurator(modelBuilder, _logger, cancellationToken);
                }

                _logger.LogInformation(
                    "[{methodName}][Ended] Model:{modelName} - elapsed time: {elapsed}ms",
                    nameof(ClassifyTestAsync),
                    modelBuilder.ModelName,
                    sw.GetElapsedTime().TotalMilliseconds);
            }

            await Task.CompletedTask;
        }

        public async Task SaveModelAsync(CancellationToken cancellationToken)
        {
            // 6. save to the file
            foreach (var modelBuilder in _modelBuilders)
            {
                var sw = ValueStopwatch.StartNew();

                var modelLoaderOptions = _modelLoaderOptions.Create(modelBuilder.ModelName);

                _logger.LogInformation("[{methodName}][Started] Model:{modelName}", nameof(SaveModelAsync), modelBuilder.ModelName);

                var readStream = modelBuilder.GetModelStream();
                await modelLoaderOptions.ModalLoader.SaveModelAsync(readStream, cancellationToken);

                _logger.LogInformation(
                    "[{methodName}][Ended] Model:{modelName} - elapsed time: {elapsed}ms",
                    nameof(SaveModelAsync),
                    modelBuilder.ModelName,
                    sw.GetElapsedTime().TotalMilliseconds);
            }
        }
    }
}
