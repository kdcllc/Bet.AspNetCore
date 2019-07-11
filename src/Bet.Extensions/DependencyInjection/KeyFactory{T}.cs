using System;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class FactorySelector<TKey, TService> : IKeyFactory<TKey, TService>
        where TService : class
    {
        private readonly IServiceProvider _provider;
        private readonly Func<IServiceProvider, TKey, TService> _func;

        public FactorySelector(
            IServiceProvider provider,
            Func<IServiceProvider, TKey, TService> func)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public TService Create(TKey resolver)
        {
            return _func(_provider, resolver);
        }
    }
}
