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
        private readonly ModelPredictionEngineOptions<TData, TPrediction> _options;
        private readonly MLContext _mlContext;
        private readonly ILogger _logger;
        private readonly ObjectPool<PredictionEngine<TData, TPrediction>> _predictionEnginePool;

        public ITransformer Model { get; private set; }

        public ModelPredictionEngineObjectPool(
           Func<ModelPredictionEngineOptions<TData, TPrediction>> options,
           ILoggerFactory loggerFactory)
        {
            _options = options() ?? throw new ArgumentNullException(nameof(options));
            _logger = loggerFactory.CreateLogger(nameof(ModelPredictionEngineObjectPool<TData,TPrediction>)) ?? throw new ArgumentNullException(nameof(loggerFactory));

            // get mlcontext
            _mlContext = _options.MLContext();

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
            var pooledObjectPolicy = new ModelPredictionEnginePooledObjectPolicy<TData, TPrediction>(_mlContext, Model, _options, _logger);

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
