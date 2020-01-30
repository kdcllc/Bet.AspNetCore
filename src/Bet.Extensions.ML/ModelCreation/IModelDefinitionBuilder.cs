using System.Collections.Generic;
using System.IO;

using Bet.Extensions.ML.ModelCreation.Results;

using Microsoft.ML;

namespace Bet.Extensions.ML.ModelCreation
{
    /// <summary>
    /// ML.NET model definition interface for generic processes of the Machine Learning.
    /// </summary>
    /// <typeparam name="TInput">The type of the input data.</typeparam>
    /// <typeparam name="TResult">The time of of the prediction result.</typeparam>
    public interface IModelDefinitionBuilder<TInput, TResult>
        where TInput : class
        where TResult : MetricsResult
    {
        /// <summary>
        /// ML.NET Named Model.
        /// </summary>
        string ModelName { get; }

        /// <summary>
        /// ML.NET Model that was produced.
        /// </summary>
        ITransformer? Model { get; }

        /// <summary>
        /// ML.NET Context to be used for Model Generation.
        /// </summary>
        MLContext MLContext { get; }

        /// <summary>
        /// Loads raw ML.NET model dataset from <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="records"></param>
        void LoadData(IEnumerable<TInput> records);

        /// <summary>
        /// Builds <see cref="IDataView"/> for ML.NET consumption.
        /// </summary>
        void BuildDataView();

        /// <summary>
        /// Builds ML/NET training pipeline.
        /// </summary>
        /// <returns></returns>
        TrainingPipelineResult BuildTrainingPipeline();

        /// <summary>
        /// Evaluates ML.NET model and returns model generation statistical results.
        /// </summary>
        /// <returns></returns>
        TResult Evaluate();

        /// <summary>
        /// Trains ML.NET Model.
        /// </summary>
        /// <returns></returns>
        TrainModelResult TrainModel();

        /// <summary>
        /// Saves ML.net model into <see cref="Stream"/>.
        /// </summary>
        /// <returns></returns>
        Stream GetModelStream();
    }
}
