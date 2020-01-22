using System;

using Bet.Extensions.ML.ModelCreation;

namespace Microsoft.Extensions.DependencyInjection
{
    public class ModelCreationServiceBuilder<TInput, TResult> : IModelCreationServiceBuilder<TInput, TResult>
        where TInput : class
        where TResult : MetricsResult
    {
        public ModelCreationServiceBuilder(IServiceCollection services, string modelName)
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
