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
        private readonly IOptionsFactory<ModelLoaderOptions> _modelLoaderOptionsFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelCreationEngine{TInput, TResult, TOptions}"/> class.
        /// </summary>
        /// <param name="modelBuilders"></param>
        /// <param name="sourceLoaderOptionsFactory"></param>
        /// <param name="engineOptionsMonitor"></param>
        /// <param name="modelLoaderOptionsFactory"></param>
        /// <param name="logger"></param>
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
            _modelLoaderOptionsFactory = modelLoaderOptionsFactory ?? throw new ArgumentNullException(nameof(modelLoaderOptionsFactory));
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
            // usually 1 type per model is registered
            foreach (var modelBuilder in _modelBuilders)
            {
                var sw = ValueStopwatch.StartNew();
                Log.StartProcess(_logger, nameof(TrainModelAsync), modelBuilder.ModelName);

                var engineOptions = _engineOptionsMonitor.Get(modelBuilder.ModelName);

                var sourceLoaderOptions = engineOptions.SourceLoaderOptionsConfigurator(_sourceLoaderOptionsFactory, modelBuilder.ModelName);

                var modelLoaderOptions = engineOptions.ModelLoaderOptionsConfigurator(_modelLoaderOptionsFactory, modelBuilder.ModelName);

                var data = await engineOptions.DataLoader(sourceLoaderOptions.SourceLoader, cancellationToken);

                var result = engineOptions.TrainModelConfigurator(modelBuilder, data, _logger);

                await modelLoaderOptions.ModalLoader.SaveResultAsync(result, cancellationToken);

                Log.EndProcess(_logger, nameof(TrainModelAsync), modelBuilder.ModelName, sw.GetElapsedTime());
            }
        }

        public async Task ClassifyTestAsync(CancellationToken cancellationToken)
        {
            foreach (var modelBuilder in _modelBuilders)
            {
                var sw = ValueStopwatch.StartNew();
                Log.StartProcess(_logger, nameof(ClassifyTestAsync), modelBuilder.ModelName);

                var engineOptions = _engineOptionsMonitor.Get(modelBuilder.ModelName);

                if (engineOptions.ClassifyTestConfigurator != null)
                {
                    await engineOptions.ClassifyTestConfigurator(modelBuilder, _logger, cancellationToken);
                }

                Log.EndProcess(_logger, nameof(ClassifyTestAsync), modelBuilder.ModelName, sw.GetElapsedTime());
            }

            await Task.CompletedTask;
        }

        public async Task SaveModelAsync(CancellationToken cancellationToken)
        {
            // 6. save to the file
            foreach (var modelBuilder in _modelBuilders)
            {
                var sw = ValueStopwatch.StartNew();
                Log.StartProcess(_logger, nameof(SaveModelAsync), modelBuilder.ModelName);

                var engineOptions = _engineOptionsMonitor.Get(modelBuilder.ModelName);

                var modelLoaderOptions = engineOptions.ModelLoaderOptionsConfigurator(_modelLoaderOptionsFactory, modelBuilder.ModelName);

                var readStream = modelBuilder.GetModelStream();
                await modelLoaderOptions.ModalLoader.SaveAsync(readStream, cancellationToken);

                Log.EndProcess(_logger, nameof(SaveModelAsync), modelBuilder.ModelName, sw.GetElapsedTime());
            }
        }

#nullable disable
#pragma warning disable SA1201 // Elements should appear in the correct order
        internal static class Log
        {
            public static class EventIds
            {
                public static readonly EventId StartProcess = new EventId(100, nameof(StartProcess));
                public static readonly EventId EndProcess = new EventId(101, nameof(EndProcess));
            }

            private static readonly Action<ILogger, string, string, Exception> _startProcess = LoggerMessage.Define<string, string>(
                LogLevel.Debug,
                EventIds.StartProcess,
                "[{methodName}][Started] Model: {modelName};");

            private static readonly Action<ILogger, string, string, double, Exception> _endProcess = LoggerMessage.Define<string, string, double>(
                LogLevel.Debug,
                EventIds.EndProcess,
                "[{methodName}][Ended] Model: {modelName}; elapsed time: {elapsed}ms");

            public static void StartProcess(ILogger logger, string methodName, string modelName)
            {
                _startProcess(logger, methodName, modelName, null);
            }

            public static void EndProcess(ILogger logger, string methodName, string modelName, TimeSpan time)
            {
                _endProcess(logger, methodName, modelName, time.TotalMilliseconds, null);
            }
        }
#pragma warning restore SA1201 // Elements should appear in the correct order
#nullable restore

    }
}
