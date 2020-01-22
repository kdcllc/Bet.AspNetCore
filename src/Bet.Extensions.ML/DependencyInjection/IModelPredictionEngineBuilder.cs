namespace Microsoft.Extensions.DependencyInjection
{
    public interface IModelPredictionEngineBuilder<TInput, TPrediction>
        where TInput : class
        where TPrediction : class, new()
    {
        IServiceCollection Services { get; }

        string ModelName { get; }
    }
}
