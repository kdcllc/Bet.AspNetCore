using System;

namespace Bet.Extensions.ML.Prediction
{
    public static class ModelPredictionEngineObjectPoolExtensions
    {
        public static TPrediction Predict<TData, TPrediction>(
            this IModelPredictionEngine<TData, TPrediction> modelPredictionEngine,
            TData dataSample) where TData : class where TPrediction : class, new()
        {
            var modelPredictionEnginePool = modelPredictionEngine?.GetPredictionEnginePool() ?? throw new NullReferenceException("GetPredictionEnginePool() returned null");
            var pool = modelPredictionEnginePool.Get();
            try
            {
                return pool.Predict(dataSample);
            }
            finally
            {
                modelPredictionEnginePool.Return(pool);
            }
        }
    }
}
