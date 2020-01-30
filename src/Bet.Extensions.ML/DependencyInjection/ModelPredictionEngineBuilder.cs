using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public class ModelPredictionEngineBuilder<TInput, TPrediction> : IModelPredictionEngineBuilder<TInput, TPrediction>
        where TInput : class
        where TPrediction : class, new()
    {
        public ModelPredictionEngineBuilder(IServiceCollection services, string modelName)
        {
            if (string.IsNullOrEmpty(modelName))
            {
                throw new ArgumentException("message", nameof(modelName));
            }

            Services = services ?? throw new ArgumentNullException(nameof(services));
            ModelName = modelName;
        }

        public IServiceCollection Services { get; }

        public string ModelName { get; }
    }
}
