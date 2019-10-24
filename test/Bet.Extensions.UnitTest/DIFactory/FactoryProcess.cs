using System;

using Microsoft.Extensions.DependencyInjection;

namespace Bet.Extensions.UnitTest.DIFactory
{
    public class FactoryProcess<T>
    {
        private readonly IFactory<IProcess> _factory;

        public FactoryProcess(IFactory<IProcess> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public void DoWork()
        {
            var instance = _factory.Create();

            instance.Run();
        }
    }
}
