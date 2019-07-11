using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Bet.AspNetCore.UnitTest.Factory
{
    public class FactoryTests
    {
        public enum ProcessKey
        {
            A,
            B
        }

        [Fact]
        public void Test_Trainsient_Factory_Registration()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransientFactory<IProcess, ProcessB>();

            serviceCollection.AddTransient<FactoryProcess<string>>();

            var services = serviceCollection.BuildServiceProvider();

            var process1 = services.GetRequiredService<FactoryProcess<string>>();

            process1.DoWork();

            var process2 = services.GetRequiredService<FactoryProcess<string>>();

            process2.DoWork();

            Assert.NotSame(process1, process2);
        }

        [Fact]
        public void Test_Throw_ArgumentException()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransientFactory<IProcess, ProcessB>();

            Assert.Throws<ArgumentException>(() => serviceCollection.AddTransientFactory<IProcess, ProcessA>());
        }

        [Fact]
        public void Test_Trainsient_FactorySelector_Registration()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransientKeyFactory<IProcess, ProcessKey>(
                (sp, key) =>
                {
                    switch (key)
                    {
                        case ProcessKey.A:
                            return sp.GetRequiredService<ProcessA>();
                        case ProcessKey.B:
                            return sp.GetRequiredService<ProcessB>();
                        default:
                            throw new KeyNotFoundException();
                    }
                },
                typeof(ProcessA),
                typeof(ProcessB));

            serviceCollection.AddTransient<KeyFactoryProcess>();

            var services = serviceCollection.BuildServiceProvider();

            var process1 = services.GetRequiredService<KeyFactoryProcess>();

            Assert.NotNull(process1);

            process1.DoWork();
        }
    }
}
