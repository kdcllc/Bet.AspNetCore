using System.Collections.Generic;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Xunit;

namespace Bet.AspNetCore.UnitTest
{
    public class AzureKeyVaultTests
    {
        [Fact(Skip = "Integration Test")]

        // [Fact]
        public void TestGenericHost()
        {
            var dic = new Dictionary<string, string>
            {
                { "AzureVault:BaseUrl", "https://kdcllc.vault.azure.net/" },
                { "AzureVault:ClientId", string.Empty },
                { "AzureVault:ClientSecret", string.Empty },
            };

            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                {
                    configBuilder.AddInMemoryCollection(dic);
                    configBuilder.AddAzureKeyVault(hostingEnviromentName: hostingContext.HostingEnvironment.EnvironmentName);

                    // print out the environment
                    var config = configBuilder.Build();
                    config.DebugConfigurations();
                })
                .ConfigureLogging((hostContext, logging) =>
                {
                })
                .Build();

            var services = host.Services;
        }
    }
}
