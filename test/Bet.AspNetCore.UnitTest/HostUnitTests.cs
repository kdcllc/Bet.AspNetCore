using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Xunit;

namespace Bet.AspNetCore.UnitTest
{
    public class HostUnitTests
    {
        [Fact]
        public void Should_have_once_registration()
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
