namespace Microsoft.Extensions.DependencyInjection
{
    public interface IModelCreationServiceBuilder
    {
        IServiceCollection Services { get; }

        string ModelName { get; }
    }
}
