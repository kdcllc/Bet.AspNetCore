using System.Collections.Generic;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Bet.Extensions.ML
{
    /// <summary>
    /// An Abstract class to support Machine Learning Model Building.
    /// </summary>
    /// <typeparam name="TInput">The object type for the input of the model.</typeparam>
    /// <typeparam name="TOutput">The object type for the output of the model.</typeparam>
    /// <typeparam name="TResult">The training result type for the model.</typeparam>
    public abstract class ModelCreationBuilder<TInput, TOutput, TResult>
        where TInput : class
        where TOutput : class
        where TResult: class
    {
        /// <summary>
        /// Machine Learning context of the model.
        /// </summary>
        public MLContext MlContext;


        protected ILogger _logger;

        protected List<TInput> _records = new List<TInput>();

        /// <summary>
        /// <see cref="DataViewSchema"/> is needed for model saving.
        /// </summary>
        public virtual DataViewSchema TrainingSchema { get; set; }

        /// <summary>
        /// The constructor to create an instance of the builder object.
        /// </summary>
        /// <param name="context">The Machine Leaning Context.</param>
        /// <param name="inputs">The input data besides the one used for the initial model building.</param>
        /// <param name="logger">The logger for the model creation logging.</param>
        public ModelCreationBuilder(
            MLContext context = null,
            IEnumerable<TInput> inputs = null,
            ILogger logger = null)
        {
            MlContext = context ?? new MLContext();
            _logger = logger;
            if (inputs != null)
            {
                _records.AddRange(inputs);
            }
        }

        /// <summary>
        /// Allows to load the default values for this model. If not called the only input values are used to build Machine Learning model is
        /// the input value at the creation of <see cref="ModelCreationBuilder{TInput, TOutput, TResult}"/>
        /// </summary>
        public abstract void LoadData();

        /// <summary>
        /// The methods to allow the model training and creation.
        /// </summary>
        /// <returns></returns>
        public abstract TResult Train();
    }
}
