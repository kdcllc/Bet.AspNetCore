using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.ML;

namespace Bet.Extensions.ML.Prediction
{
    public class ModelPredictionEngineObjectPool<TData, TPrediction>
        : IModelPredictionEngine<TData, TPrediction>
        where TData : class
        where TPrediction : class, new()
    {
        private readonly ModelPredictionEngineOptions _options;
        private readonly MLContext _mlContext;
        private readonly ILogger _logger;
        private readonly ObjectPool<PredictionEngine<TData, TPrediction>> _predictionEnginePool;

        public ITransformer Model { get; private set; }

        public ModelPredictionEngineObjectPool(
           ModelPredictionEngineOptions options,
           ILogger logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // get mlcontext
            _mlContext = options.MLContext();

            // get prediction model
            Model = _options.CreateModel(_mlContext);

            // create PredictionEngine Object Pool
            _predictionEnginePool = CreatePredictionEngineObjectPool();
        }

        public TPrediction Predict(TData dataSample)
        {
            // get instance of PredictionEngine from the object pool

            var predictionEngine = _predictionEnginePool.Get();

            try
            {
                return predictionEngine.Predict(dataSample);
            }
            catch (Exception ex)
            {
                _logger.LogError("Predict failed: {ex}", ex.ToString());
            }
            finally
            {
                // release used PredictionEngine object into the Object pool.
                _predictionEnginePool.Return(predictionEngine);
            }

            // all other cases return null prediction.
            return null;
        }

        private ObjectPool<PredictionEngine<TData,TPrediction>> CreatePredictionEngineObjectPool()
        {
            var pooledObjectPolicy = new PredictionEnginePooledObjectPolicy<TData, TPrediction>(_mlContext, Model, _logger);

            DefaultObjectPool<PredictionEngine<TData, TPrediction>> pool;

            if (_options.MaximumObjectsRetained != -1)
            {
                pool = new DefaultObjectPool<PredictionEngine<TData, TPrediction>>(pooledObjectPolicy, _options.MaximumObjectsRetained);
            }
            else
            {
                //default maximumRetained is Environment.ProcessorCount * 2, if not explicitly provided
                pool = new DefaultObjectPool<PredictionEngine<TData, TPrediction>>(pooledObjectPolicy);
            }

            return pool;
        }
    }
}
