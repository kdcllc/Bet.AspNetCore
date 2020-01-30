using System;
using System.Threading.Tasks;

using Bet.AspNetCore.Sample.Data;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace Bet.AspNetCore.Sample
{
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            await host.RunStartupJobsAync();

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.UseShutdownTimeout(TimeSpan.FromSeconds(20));

                            webBuilder.ConfigureAppConfiguration((hostingContext, configBuilder) =>
                            {
                                // based on environment Development = dev; Production = prod prefix in Azure Vault.
                                var envName = hostingContext.HostingEnvironment.EnvironmentName;

                                var configuration = configBuilder.AddAzureKeyVault(
                                    hostingEnviromentName: envName,
                                    usePrefix: false,
                                    reloadInterval: null);

                                // helpful to see what was retrieved from all of the configuration providers.
                                if (hostingContext.HostingEnvironment.IsDevelopment())
                                {
                                    configuration.DebugConfigurations();
                                }
                            });

                            webBuilder.UseSerilog((hostingContext, loggerConfiguration) =>
                            {
                                var applicationName = $"BetWebSample-{hostingContext.HostingEnvironment.EnvironmentName}";
                                loggerConfiguration
                                        .ReadFrom.Configuration(hostingContext.Configuration)
                                        .Enrich.FromLogContext()
                                        .WriteTo.Console()
                                        .AddApplicationInsights(hostingContext.Configuration)
                                        .AddAzureLogAnalytics(hostingContext.Configuration, applicationName: applicationName);
                            });

                            webBuilder.ConfigureServices(services =>
                            {
                                // commented it out if model building to be done on apps load.
                                // services.AddStartupJob<ModelBuilderJob>();
                                services.AddStartupJob<SeedDatabaseStartupJob>();
                            });

                            webBuilder.UseStartup<Startup>();
                        });
        }
    }
}
