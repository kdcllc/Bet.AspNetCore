using System;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class Factory<TService> : IFactory<TService>
        where TService : class
    {
        private readonly Func<TService> _func;

        public Factory(Func<TService> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public TService Create()
        {
            return _func();
        }
    }
}
