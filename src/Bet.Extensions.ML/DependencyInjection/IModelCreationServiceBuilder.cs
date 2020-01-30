using Bet.Extensions.ML.ModelCreation;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IModelCreationServiceBuilder<TInput, TResult>
            where TInput : class
            where TResult : MetricsResult
    {
        IServiceCollection Services { get; }

        string ModelName { get; }
    }
}
