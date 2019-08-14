using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ML;

namespace Bet.Extensions.ML.ModelBuilder
{
    /// <summary>
    /// ML Model Builder base interface.
    /// </summary>
    /// <typeparam name="TInput">The type of the Input data.</typeparam>
    /// <typeparam name="TOutput">The type of the Output data.</typeparam>
    /// <typeparam name="TResult">The type of the Evaluate function.</typeparam>
    public interface IModelCreationBuilder<TInput, TOutput, TResult>
        where TInput : class
        where TOutput : class, new()
        where TResult : MetricsResult
    {
        IDataView TrainingDataView { get; }

        IDataView TestDataView { get; }

        IEstimator<ITransformer> TrainingPipeLine { get; }

        string TrainerName { get; }

        /// <summary>
        /// ML dataset that was loaded.
        /// </summary>
        IDataView DataView { get; }

        /// <summary>
        /// All of the records.
        /// </summary>
        List<TInput> Records { get; set; }

        /// <summary>
        /// ML Context to be used for Model Generation.
        /// </summary>
        MLContext MLContext { get; set; }

        /// <summary>
        /// Model that was produced.
        /// </summary>
        ITransformer Model { get; }

        /// <summary>
        /// Input Training Schema.
        /// </summary>
        DataViewSchema TrainingSchema { get; set; }

        /// <summary>
        /// Builds DataView object to be used for the training pipeline.
        /// </summary>
        /// <param name="testFraction">The fraction of data to go into the test set.</param>
        /// <returns></returns>
        IModelCreationBuilder<TInput, TOutput, TResult> BuiltDataView(double testFraction = 0.1);

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
        /// Builds ML training pipeline.
        /// </summary>
        /// <returns></returns>
        TrainingPipelineResult BuildTrainingPipeline();

        /// <summary>
        /// Builds ML training pipeline.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
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
        TrainModelResult TrainModel(Func<IDataView, TrainModelResult> builder);

        /// <summary>
        /// Evaluates ML model and returns results.
        /// </summary>
        /// <returns></returns>
        TResult Evaluate();

        /// <summary>
        /// Evaluates ML model and returns results.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        TResult Evaluate(Func<IDataView, IEstimator<ITransformer>, TResult> builder);

        /// <summary>
        /// Saves ML model.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="modelRelativePath"></param>
        void SaveModel(Action<MLContext, ITransformer, string, DataViewSchema> builder, string modelRelativePath);

        /// <summary>
        /// Saves ML model to a disk.
        /// </summary>
        /// <param name="modelRelativePath"></param>
        void SaveModel(string modelRelativePath);

        /// <summary>
        /// Get Binary stream of ML.NET model.
        /// </summary>
        /// <returns></returns>
        MemoryStream GetModelStream();
    }
}
