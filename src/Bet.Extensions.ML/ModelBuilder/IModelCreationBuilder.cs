using System;
using System.Collections.Generic;

using Microsoft.ML;

namespace Bet.Extensions.ML.ModelBuilder
{
    public interface IModelCreationBuilder<TInput, TOutput, TResult>
        where TInput : class
        where TOutput : class
        where TResult : class
    {
        MLContext MLContext { get; set; }

        ITransformer Model { get; set; }

        DataViewSchema TrainingSchema { get; set; }


        IModelCreationBuilder<TInput, TOutput, TResult> BuiltDataView();

        /// <summary>
        /// Loads dataset from embedded resource of the library.
        /// </summary>
        /// <returns></returns>
        IModelCreationBuilder<TInput, TOutput, TResult> LoadDefaultData();

        /// <summary>
        /// Loads model dataset from <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        IModelCreationBuilder<TInput, TOutput, TResult> LoadData(IEnumerable<TInput> data);

        /// <summary>
        /// Builds ML training Pipeline.
        /// </summary>
        /// <returns></returns>
        TrainingPipelineResult BuildTrainingPipeline();

        TrainingPipelineResult BuildTrainingPipeline(Func<TrainingPipelineResult> builder);

        /// <summary>
        /// Trains ML model.
        /// </summary>
        /// <returns></returns>
        TrainModelResult TrainModel();

        /// <summary>
        /// Trains ML Model.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        TrainModelResult TrainModel(Func<TrainModelResult> builder);

        /// <summary>
        /// Evaluates ML model and returns results
        /// </summary>
        /// <returns></returns>
        TResult Evaluate();
    }
}
