using System;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.ML;

namespace Bet.Extensions.ML.Prediction
{
    public sealed class ModelPoolLoader<TData, TPrediction> : IDisposable
        where TData : class
        where TPrediction : class, new()
    {
        private readonly ILogger _logger;
        private readonly ModelPredictionEngineOptions<TData, TPrediction> _options;
        private readonly MLContext _mlContext;
        private readonly IDisposable _changeToken;

        private DefaultObjectPool<PredictionEngine<TData, TPrediction>> _pool;
        private ITransformer? _model;

        public ModelPoolLoader(ModelPredictionEngineOptions<TData, TPrediction> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _mlContext = _options.ServiceProvider.GetRequiredService<IOptions<MLContextOptions>>().Value.MLContext
                ?? throw new ArgumentNullException("MLContext is missing");

            _logger = _options.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(ModelPoolLoader<TData, TPrediction>));

            LoadPool();

            _changeToken = ChangeToken.OnChange(
                () => _options.GetReloadToken(),
                () => LoadPool());
        }

        public ObjectPool<PredictionEngine<TData, TPrediction>> PredictionEnginePool => _pool;

        public void Dispose()
        {
            _changeToken?.Dispose();
        }

        public ITransformer GetModel()
        {
            if (_model == null)
            {
                throw new NullReferenceException("Model wasn't created");
            }

            return _model;
        }

        private void LoadPool()
        {
            if (_options.CreateModel == null)
            {
                throw new NullReferenceException("CreateModel wasn't provided...");
            }

            Interlocked.Exchange(ref _model, _options.CreateModel(_mlContext));

            if (_model == null)
            {
                throw new NullReferenceException("Model wasn't created");
            }

            var pooledObjectPolicy = new ModelPredictionEnginePoolPolicy<TData, TPrediction>(_mlContext, _model);

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
                nameof(ModelPoolLoader<TData, TPrediction>),
                nameof(LoadPool),
                _options.ModelName);
        }
    }
}
