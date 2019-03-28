using Microsoft.ML;

namespace Bet.Extensions.ML.Prediction
{
    public interface IModelPredictionEngine<TData, TPrediction> where TData : class where TPrediction : class
    {
        /// <summary>
        /// The transformer holds the Machine Learning predictive model data.
        /// </summary>
        ITransformer Model { get; }

        /// <summary>
        /// Predict based on <see cref="Model"/> that was loaded.
        /// </summary>
        /// <param name="dataSample">The data sample to be predicted on.</param>
        /// <returns></returns>
        TPrediction Predict(TData dataSample);
    }
}
