using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bet.Hosting.Sample
{
    /// <summary>
    /// https://github.com/dotnet/machinelearning/blob/master/docs/code/MlNetCookBook.md.
    /// </summary>
    internal sealed class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                {
                    // based on environment Development = dev; Production = prod prefix in Azure Vault.
                    var envName = hostingContext.HostingEnvironment.EnvironmentName;

                    var configuration = configBuilder.AddAzureKeyVault(
                        hostingEnviromentName: envName,
                        usePrefix: false,
                        reloadInterval: TimeSpan.FromSeconds(30));

                    // helpful to see what was retrieved from all of the configuration providers.
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        // var configuration = configBuilder.Build();
                        configuration.DebugConfigurations();
                    }
                })
                .ConfigureLogging((hostingContext, logger) =>
                {
                    logger.AddConfiguration(hostingContext.Configuration);
                    logger.AddConsole();
                    logger.AddDebug();
                })
                .ConfigureServices(services =>
                {
                    services.AddModelBuilderService();
                });
        }
    }
}
