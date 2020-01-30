using System;
using System.Collections.Generic;

using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace Bet.Extensions.ML.Prediction
{
    public class ModelPredictionEngine<TInput, TPrediction> : IModelPredictionEngine<TInput, TPrediction>, IDisposable
        where TInput : class
        where TPrediction : class, new()
    {
        private readonly IOptionsFactory<ModelPredictionEngineOptions<TInput, TPrediction>> _optionsFactory;
        private readonly Dictionary<string, ModelPoolLoader<TInput, TPrediction>> _namedPools;
        private readonly ModelPoolLoader<TInput, TPrediction>? _defaultPool;

        private bool _disposed = false;

        public ModelPredictionEngine(
            IOptionsFactory<ModelPredictionEngineOptions<TInput, TPrediction>> optionsFactory,
            IOptions<MLContextOptions> mlContextOptions)
        {
            _optionsFactory = optionsFactory ?? throw new ArgumentNullException(nameof(optionsFactory));

            MLContext = mlContextOptions.Value.MLContext ?? throw new ArgumentNullException(nameof(mlContextOptions));

            var defaultOptions = _optionsFactory.Create(string.Empty);
            if (defaultOptions.CreateModel != null)
            {
                _defaultPool = new ModelPoolLoader<TInput, TPrediction>(defaultOptions);
            }

            _namedPools = new Dictionary<string, ModelPoolLoader<TInput, TPrediction>>();
        }

        public MLContext MLContext { get; }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        public ITransformer GetModel()
        {
            if (_defaultPool == null)
            {
                throw new NullReferenceException("Default Model wasn't added");
            }

            return _defaultPool.GetModel();
        }

        public ITransformer GetModel(string modelName)
        {
            return _namedPools[modelName].GetModel();
        }

        public PredictionEngine<TInput, TPrediction> GetPredictionEngine()
        {
            return GetPredictionEngine(string.Empty);
        }

        public PredictionEngine<TInput, TPrediction> GetPredictionEngine(string modelName)
        {
            if (_namedPools.ContainsKey(modelName))
            {
                return _namedPools[modelName].PredictionEnginePool.Get();
            }

            if (string.IsNullOrEmpty(modelName))
            {
                if (_defaultPool == null)
                {
                    throw new ArgumentException("You need to configure a default, not named, model before you use this method.");
                }

                return _defaultPool.PredictionEnginePool.Get();
            }

            var options = _optionsFactory.Create(modelName);

            var pool = new ModelPoolLoader<TInput, TPrediction>(options);

            _namedPools.Add(modelName, pool);

            return pool.PredictionEnginePool.Get();
        }

        public void ReturnPredictionEngine(PredictionEngine<TInput, TPrediction> engine)
        {
            ReturnPredictionEngine(string.Empty, engine);
        }

        public void ReturnPredictionEngine(string modelName, PredictionEngine<TInput, TPrediction> engine)
        {
            if (engine == null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            if (string.IsNullOrEmpty(modelName)
                && _defaultPool != null)
            {
                _defaultPool?.PredictionEnginePool?.Return(engine);
            }
            else
            {
                _namedPools[modelName]?.PredictionEnginePool?.Return(engine);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _defaultPool?.Dispose();

                foreach (var pool in _namedPools)
                {
                    pool.Value?.Dispose();
                }
            }

            _disposed = true;
        }
    }
}
