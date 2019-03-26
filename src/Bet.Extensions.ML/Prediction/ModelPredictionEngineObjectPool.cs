using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace Bet.Extensions.ML.Prediction
{
    public class ModelPredictionEngineObjectPool<TData, TPrediction>
        : IModelPredictionEngine<TData, TPrediction>
        where TData : class
        where TPrediction : class, new()
    {
        private readonly MLContext _mlContext;
        private readonly ILogger _logger;
        private readonly int _maximumObjectsRetained;
        private readonly ObjectPool<PredictionEngine<TData, TPrediction>> _predictionEnginePool;

        public ITransformer MLModel { get; private set; }

        public ModelPredictionEngineObjectPool(
            IOptionsMonitor<MLContextOptions> options,
            string modelFilePathName,
            ILogger logger,
            int maximumObjectsRetained = -1)
        {
            //Create the MLContext object to use under the scope of this class
            _mlContext = new MLContext();

            _logger = logger;
            //Load the ProductSalesForecast model from the .ZIP file
            using (var fileStream = File.OpenRead(modelFilePathName))
            {
                MLModel = _mlContext.Model.Load(fileStream);
            }

            _maximumObjectsRetained = maximumObjectsRetained;

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
                _logger.LogError("PredictionEngine failed: {ex}", ex.ToString());
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
            var pooledObjectPolicy = new PredictionEnginePooledObjectPolicy<TData, TPrediction>(_mlContext, MLModel, _logger);

            DefaultObjectPool<PredictionEngine<TData, TPrediction>> pool;

            if (_maximumObjectsRetained != -1)
            {
                pool = new DefaultObjectPool<PredictionEngine<TData, TPrediction>>(pooledObjectPolicy, _maximumObjectsRetained);
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
