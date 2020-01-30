using System;

using Bet.Extensions.ML.DataLoaders.ModelLoaders;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.ML;

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
        /// The instance of the service provider.
        /// </summary>
        public IServiceProvider? ServiceProvider { get; set; } = default;

        /// <summary>
        /// This is set up by DI process. The default value is empty string.
        /// </summary>
        public string ModelName { get; set; } = string.Empty;

        /// <summary>
        /// Will contain the input schema for the model. If the model was saved without any
        /// description of the input, there will be no input schema. In this case this can be null.
        /// </summary>
        public DataViewSchema? InputSchema { get; set; } = default;

        /// <summary>
        /// The number of maximum pools allowed for Object Pool. The default value is -1 which sets Environment.ProcessorCount * 2.
        /// </summary>
        public int MaximumObjectsRetained { get; set; } = -1;

        /// <summary>
        /// The entry point to configure and load the actual Machine Learning Model.
        /// </summary>
        public Func<MLContext, ITransformer>? CreateModel { get; set; }

        public ModelLoader? ModelLoader { get; set; }

        /// <summary>
        /// The logging level for the <see cref="MLContext"/> for this instance of options.The default is <see cref="LogLevel.Trace"/>.
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Trace;

        /// <summary>
        /// Raises an event when the options have changed.
        /// </summary>
        /// <returns></returns>
        public IChangeToken GetReloadToken()
        {
            if (ModelLoader == null)
            {
                throw new NullReferenceException($"{nameof(ModelLoader)} must be set");
            }

            return ModelLoader.GetReloadToken();
        }
    }
}
