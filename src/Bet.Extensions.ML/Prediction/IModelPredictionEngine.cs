using Microsoft.ML;

namespace Bet.Extensions.ML.Prediction
{
    public interface IModelPredictionEngine<TData, TPrediction> where TData : class where TPrediction : class
    {
        /// <summary>
        /// The transformer is a component that transforms data. It also supports 'schema
        /// propagation' to answer the question of 'how will the data with this schema look,
        /// after you transform it?'.
        /// </summary>
        ITransformer MLModel { get; }

        /// <summary>
        /// Predict based on <see cref="MLModel"/> that was loaded.
        /// </summary>
        /// <param name="dataSample">The data sample to be predicted on.</param>
        /// <returns></returns>
        TPrediction Predict(TData dataSample);
    }
}
