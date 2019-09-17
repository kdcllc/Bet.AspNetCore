using System;

using Microsoft.Extensions.DependencyInjection;

namespace Bet.Extensions.ML.Prediction
{
    public class DefaultModelPredictionEngineBuilder<TData, TPrediction> : IModelPredictionEngineBuilder<TData, TPrediction>
        where TData : class
        where TPrediction : class, new()
    {
        public DefaultModelPredictionEngineBuilder(IServiceCollection services, string modelName)
        {
            Services = services ?? throw new ArgumentException("Can't be null", nameof(services));
            ModelName = modelName;
        }

        public IServiceCollection Services { get; }

        public string ModelName { get; }
    }
}
