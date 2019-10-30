using System;
using System.Threading.Tasks;

using Bet.ML.WebApi.Sample.Jobs;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace Bet.ML.WebApi.Sample
{
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // begin start up jobs.
            await host.RunStartupJobsAync();

            // start the server.
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.ConfigureAppConfiguration((hostingContext, configBuilder) =>
                        {
                            // based on environment Development = dev; Production = prod prefix in Azure Vault.
                            var envName = hostingContext.HostingEnvironment.EnvironmentName;

                            var configuration = configBuilder.AddAzureKeyVault(
                                hostingEnviromentName: envName,
                                usePrefix: false,
                                reloadInterval: TimeSpan.FromSeconds(10));

                            // helpful to see what was retrieved from all of the configuration providers.
                            if (hostingContext.HostingEnvironment.IsDevelopment())
                            {
                                configuration.DebugConfigurations();
                            }
                        });

                        webBuilder.UseSerilog((hostingContext, loggerConfiguration) =>
                        {
                            var applicationName = $"BetWebApiSample-{hostingContext.HostingEnvironment.EnvironmentName}";
                            loggerConfiguration
                                    .ReadFrom.Configuration(hostingContext.Configuration)
                                    .Enrich.FromLogContext()
                                    .WriteTo.Console()
                                    .AddApplicationInsights(hostingContext.Configuration)
                                    .AddAzureLogAnalytics(hostingContext.Configuration, applicationName: applicationName);
                        });

                        webBuilder.ConfigureServices(services =>
                        {
                            services.AddStartupJob<RebuildMLModelScheduledJob>();
                        });

                        webBuilder.UseStartup<Startup>();
                    });
        }
    }
}
