namespace Microsoft.Extensions.DependencyInjection
{
    public interface IFactory<TService> where TService : class
    {
        TService Create();
    }
}
