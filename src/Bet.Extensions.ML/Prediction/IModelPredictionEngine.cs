using Microsoft.Extensions.ObjectPool;
using Microsoft.ML;

namespace Bet.Extensions.ML.Prediction
{
    public interface IModelPredictionEngine<TData, TPrediction>
        where TData : class where TPrediction : class, new()
    {
        /// <summary>
        /// The transformer holds the Machine Learning predictive model data.
        /// </summary>
        ITransformer? GetModel();

        DefaultObjectPool<PredictionEngine<TData, TPrediction>>? GetPredictionEnginePool();
    }
}
