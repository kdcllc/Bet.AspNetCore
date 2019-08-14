using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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
                            webBuilder.UseStartup<Startup>();

                            webBuilder.ConfigureAppConfiguration((hostingContext, configBuilder) =>
                            {
                                // based on environment Development = dev; Production = prod prefix in Azure Vault.
                                var envName = hostingContext.HostingEnvironment.EnvironmentName;

                                var configuration = configBuilder.AddAzureKeyVault(hostingEnviromentName: envName, usePrefix: true);

                                // helpful to see what was retrieved from all of the configuration providers.
                                if (hostingContext.HostingEnvironment.IsDevelopment())
                                {
                                    configuration.DebugConfigurations();
                                }
                            });
                        });
        }
    }
}
