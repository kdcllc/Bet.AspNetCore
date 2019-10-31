using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;

namespace Bet.AspNetCore.Sample
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
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

                            webBuilder.UseStartup<Startup>();
                        });
        }
    }
}
