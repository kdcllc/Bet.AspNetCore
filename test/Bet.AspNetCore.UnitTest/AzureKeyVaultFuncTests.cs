using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bet.AspNetCore.UnitTest
{
    public class AzureKeyVaultFuncTests
    {
        [Fact(Skip ="Integration Test")]
        public void  TestGenericHost()
        {
            var dic = new Dictionary<string, string>
            {
                {"AzureVault:BaseUrl", "https://{name}.vault.azure.net/" },
                {"AzureVault:ClientId", "" },
                {"AzureVault:ClientSecret", "" },
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
