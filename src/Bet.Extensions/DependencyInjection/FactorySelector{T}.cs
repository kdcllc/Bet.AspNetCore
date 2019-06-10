using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public class FactorySelector<TSelector, TService> : IFactorySelector<TSelector, TService>
        where TService : class
    {
        private readonly IServiceProvider _provider;
        private readonly Func<IServiceProvider, TSelector, TService> _func;

        public FactorySelector(
            IServiceProvider provider,
            Func<IServiceProvider, TSelector, TService> func)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public TService Create(TSelector resolver)
        {
            return _func(_provider, resolver);
        }
    }
}
