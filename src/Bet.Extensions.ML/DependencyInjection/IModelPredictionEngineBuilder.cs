namespace Microsoft.Extensions.DependencyInjection
{
    public interface IModelPredictionEngineBuilder<TData, TPrediction>
        where TData : class
        where TPrediction : class, new()
    {
        IServiceCollection Services { get; }

        string ModelName { get; }
    }
}
