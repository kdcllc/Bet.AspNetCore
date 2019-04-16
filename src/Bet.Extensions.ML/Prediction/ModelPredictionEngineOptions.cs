using Microsoft.Extensions.Logging;
using Microsoft.ML;
using System;

namespace Bet.Extensions.ML.Prediction
{
    /// <summary>
    /// The Options for the Machine Learning Model <see cref="IModelPredictionEngine{TData, TPrediction}"/> interface.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TPrediction"></typeparam>
    public class ModelPredictionEngineOptions<TData, TPrediction>
        where TData : class
        where TPrediction : class, new()
    {
        /// <summary>
        /// This is set up by DI process. The default value is <see cref="Constants.MLDefaultModelName"/>.
        /// </summary>
        public string ModelName { get; set; } = Constants.MLDefaultModelName;

        /// <summary>
        /// Machine Learning Model Specific Context. The default value is set to a new instance.
        /// </summary>
        public Func<MLContext> MLContext { get; set; } = () => new MLContext();

        /// <summary>
        /// Will contain the input schema for the model. If the model was saved without any
        /// description of the input, there will be no input schema. In this case this can be null.
        /// </summary>
        public DataViewSchema InputSchema { get; set; }

        /// <summary>
        /// The number of maximum pools allowed for Object Pool. The default value is -1 which sets Environment.ProcessorCount * 2.
        /// </summary>
        public int MaximumObjectsRetained { get; set; } = -1;

        /// <summary>
        /// The entry point to configure and load the actual Machine Learning Model.
        /// </summary>
        public Func<MLContext, ITransformer> CreateModel { get; set; }

        /// <summary>
        /// The logging level for the <see cref="MLContext"/> for this instance of options.The default is <see cref="LogLevel.Trace"/>.
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Trace;
    }
}
