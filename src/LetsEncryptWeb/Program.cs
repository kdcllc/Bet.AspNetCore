using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Serilog;

namespace LetsEncryptWeb
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
                .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                {
                    // based on environment Development = dev; Production = prod prefix in Azure Vault.
                    var envName = hostingContext.HostingEnvironment.EnvironmentName;

                    var configuration = configBuilder.AddAzureKeyVault(
                        hostingEnviromentName: envName,
                        usePrefix: false,
                        reloadInterval: TimeSpan.FromMinutes(1));

                    // helpful to see what was retrieved from all of the configuration providers.
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        configuration.DebugConfigurationsWithSerilog();
                    }
                })
                .ConfigureLogging((hostingContext, logBuilder) =>
                {
                    logBuilder.ClearProviders();
                    logBuilder.AddFilter((_) => true);

                    logBuilder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logBuilder.AddDebug();
                    logBuilder.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
