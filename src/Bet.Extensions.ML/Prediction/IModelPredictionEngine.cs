using Microsoft.ML;

namespace Bet.Extensions.ML.Prediction
{
    /// <summary>
    /// Machine Learning Prediction extension framework.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TPrediction"></typeparam>
    public interface IModelPredictionEngine<TInput, TPrediction>
        where TInput : class
        where TPrediction : class, new()
    {
        /// <summary>
        /// Gets <see cref="MLContext"/> instance.
        /// </summary>
        MLContext MLContext { get; }

        /// <summary>
        /// The transformer holds the Machine Learning predictive model data.
        /// </summary>
        ITransformer GetModel();

        /// <summary>
        /// Gets Machine Learning Model.
        /// </summary>
        /// <param name="modelName">The name of the model besides the default one.</param>
        /// <returns></returns>
        ITransformer GetModel(string modelName);

        /// <summary>
        /// Get <see cref="PredictionEngine{TSrc, TDst}"/>.
        /// </summary>
        /// <returns></returns>
        PredictionEngine<TInput, TPrediction> GetPredictionEngine();

        /// <summary>
        /// Get <see cref="PredictionEngine{TSrc, TDst}"/>.
        /// </summary>
        /// <param name="modelName">The name of the model besides the default one.</param>
        /// <returns></returns>
        PredictionEngine<TInput, TPrediction> GetPredictionEngine(string modelName);

        /// <summary>
        /// Returns <see cref="PredictionEngine{TSrc, TDst}"/> back to the object pool.
        /// </summary>
        /// <param name="engine"></param>
        void ReturnPredictionEngine(PredictionEngine<TInput, TPrediction> engine);

        /// <summary>
        /// Returns <see cref="PredictionEngine{TSrc, TDst}"/> back to the object pool with specified model name.
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="engine"></param>
        void ReturnPredictionEngine(string modelName, PredictionEngine<TInput, TPrediction> engine);
    }
}
