using System.Collections.Generic;

using Microsoft.Extensions.Configuration;

using Xunit;

namespace Bet.Extensions.UnitTest.AzureAppConfig
{
    public class AppConfigTests
    {
        [RunnableInDebugOnly]
        public void Configure_Azure_App_Config_With_Defaults()
        {
            var dic = new Dictionary<string, string>
            {
                { "Environments:Development", "dev" },
                { "Environments:Staging", "qa" },
                { "Environments:Production", "prod" },
                { "AzureAppConfig:Endpoint", "https://featuremanagementworkshop.azconfig.io" },
                { "AzureAppConfig:MaxRetries", "7" },
                { "AzureAppConfig:Filters:CacheExpirationTime", "2" },
                { "AzureAppConfig:Features:CacheExpirationTime", "3" }
            };

            var currentEnvironment = "Development";

            var config = new ConfigurationBuilder()
                            .AddInMemoryCollection(dic)
                            .AddAzureAppConfiguration(
                            currentEnvironment,
                            (connect, filters, features) =>
                            {
                                filters.Sections.Add("WorkerApp:WorkerOptions*");
                                filters.RefresSections.Add("WorkerApp:WorkerOptions:Flag");
                                features.Label = currentEnvironment;
                            })
                            .Build();

            Assert.NotNull(config);
        }
    }
}
