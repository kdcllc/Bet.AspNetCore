using System.Collections.Generic;

using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Bet.Extensions.ML
{
    public abstract class ModelCreationBuilder<TInput, TOutput, TResult>
        where TInput : class
        where TOutput : class
        where TResult: class
    {
        public MLContext MlContext;
        protected ILogger _logger;

        protected List<TInput> _records = new List<TInput>();

        public virtual DataViewSchema TrainingSchema { get; set; }

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

        public abstract void LoadData();

        public abstract TResult Train();
    }
}
