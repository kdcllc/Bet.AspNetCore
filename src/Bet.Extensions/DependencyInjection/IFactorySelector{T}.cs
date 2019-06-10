namespace Microsoft.Extensions.DependencyInjection
{
    public interface IFactorySelector<TSelector, TService> where TService : class
    {
        TService Create(TSelector selctor);
    }
}
