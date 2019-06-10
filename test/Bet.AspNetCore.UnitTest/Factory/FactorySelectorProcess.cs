using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using static Bet.AspNetCore.UnitTest.Factory.FactoryTests;

namespace Bet.AspNetCore.UnitTest.Factory
{
    public class FactorySelectorProcess
    {
        private readonly IKeyFactory<ProcessKey, IProcess> _factory;

        public FactorySelectorProcess(IKeyFactory<ProcessKey, IProcess> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public void DoWork()
        {
            var instance = _factory.Create(ProcessKey.A);

            instance.Run();
        }
    }
}
