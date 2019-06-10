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
        private readonly IFactorySelector<ProcessSelector, IProcess> _factory;

        public FactorySelectorProcess(IFactorySelector<ProcessSelector, IProcess> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public void DoWork()
        {
            var instance = _factory.Create(ProcessSelector.A);

            instance.Run();
        }
    }
}
