using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Xunit;

namespace Bet.Extensions.UnitTest.Hosting
{
    public class HostUnitTests
    {
        [Fact]
        public void Should_Have_Register_Once_StartupFilters()
        {
            var host = new HostBuilder()
                .UseStartupFilters()
                .UseStartupFilters()
                .Build();

            var services = host.Services.GetRequiredService<IHostedService>() as HostStartupService;

            Assert.NotNull(services);
        }
    }
}
