using System;
using System.Threading;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Primitives;
using Microsoft.ML;

namespace Bet.Extensions.ML.Prediction
{
    public sealed class ModelPredictionEngineObjectPool<TData, TPrediction>
        : IModelPredictionEngine<TData, TPrediction>, IDisposable
        where TData : class
        where TPrediction : class, new()
    {
        private readonly Func<ModelPredictionEngineOptions<TData, TPrediction>> _funcOptions;
        private readonly ILogger _logger;
        private readonly ModelPredictionEngineOptions<TData, TPrediction> _options;
        private readonly IDisposable _changeToken;

        private DefaultObjectPool<PredictionEngine<TData, TPrediction>>? _pool;
        private ITransformer? _model;

        public ModelPredictionEngineObjectPool(
           Func<ModelPredictionEngineOptions<TData, TPrediction>> options,
           ILoggerFactory loggerFactory)
        {
            _funcOptions = options ?? throw new ArgumentNullException(nameof(options));

            _logger = loggerFactory.CreateLogger(nameof(ModelPredictionEngineObjectPool<TData, TPrediction>))
                ?? throw new ArgumentNullException(nameof(loggerFactory));

            _options = _funcOptions();

            LoadPool();

            _changeToken = ChangeToken.OnChange(
                () => _options.GetReloadToken(),
                () => LoadPool());
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
            _changeToken?.Dispose();
        }

        public ITransformer? GetModel()
        {
            return _model;
        }

        public DefaultObjectPool<PredictionEngine<TData, TPrediction>>? GetPredictionEnginePool()
        {
            if (_pool == null)
            {
                LoadPool();
            }

            return _pool;
        }

        private void LoadPool()
        {
            var mlContext = _options.MLContext();

            if (_options?.CreateModel == null)
            {
                throw new NullReferenceException("CreateModel wasn't provided...");
            }

            Interlocked.Exchange(ref _model, _options.CreateModel(mlContext));

            var pooledObjectPolicy = new ModelPredictionEnginePooledObjectPolicy<TData, TPrediction>(mlContext, _model!, _options, _logger);

            if (_options.MaximumObjectsRetained != -1)
            {
                Interlocked.Exchange(ref _pool, new DefaultObjectPool<PredictionEngine<TData, TPrediction>>(pooledObjectPolicy, _options.MaximumObjectsRetained));
            }
            else
            {
                // default maximumRetained is Environment.ProcessorCount * 2, if not explicitly provided
                Interlocked.Exchange(ref _pool, new DefaultObjectPool<PredictionEngine<TData, TPrediction>>(pooledObjectPolicy));
            }

            _logger.LogDebug(
                "[{className}][{methodName}] ML.NET Model name: {modelName}",
                nameof(ModelPredictionEngineObjectPool<TData, TPrediction>),
                nameof(LoadPool),
                _options.ModelName);
        }
    }
}
