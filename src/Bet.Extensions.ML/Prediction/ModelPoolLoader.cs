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
    public sealed class ModelPoolLoader<TInput, TPrediction> : IDisposable
        where TInput : class
        where TPrediction : class, new()
    {
        private readonly ILogger _logger;
        private readonly ModelPredictionEngineOptions<TInput, TPrediction> _options;
        private readonly MLContext _mlContext;
        private readonly IDisposable _changeToken;

        private DefaultObjectPool<PredictionEngine<TInput, TPrediction>>? _pool;
        private ITransformer? _model;

        public ModelPoolLoader(ModelPredictionEngineOptions<TInput, TPrediction> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _mlContext = _options.ServiceProvider.GetRequiredService<IOptions<MLContextOptions>>().Value.MLContext
                ?? throw new NullReferenceException("MLContext instance is missing");

            _logger = _options.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(ModelPoolLoader<TInput, TPrediction>));

            LoadPool();

            _changeToken = ChangeToken.OnChange(
                () => _options.GetReloadToken(),
                () => LoadPool());
        }

        public ObjectPool<PredictionEngine<TInput, TPrediction>> PredictionEnginePool => _pool!;

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

            var pooledObjectPolicy = new ModelPredictionEnginePoolPolicy<TInput, TPrediction>(_mlContext, _model);

            if (_options.MaximumObjectsRetained != -1)
            {
                Interlocked.Exchange(ref _pool, new DefaultObjectPool<PredictionEngine<TInput, TPrediction>>(pooledObjectPolicy, _options.MaximumObjectsRetained));
            }
            else
            {
                // default maximumRetained is Environment.ProcessorCount * 2, if not explicitly provided
                Interlocked.Exchange(ref _pool, new DefaultObjectPool<PredictionEngine<TInput, TPrediction>>(pooledObjectPolicy));
            }

            _logger.LogInformation(
                "[{className}][{methodName}] ML.NET Model name: {modelName}",
                nameof(ModelPoolLoader<TInput, TPrediction>),
                nameof(LoadPool),
                _options.ModelName);
        }
    }
}
