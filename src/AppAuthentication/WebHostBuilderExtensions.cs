using System.Drawing;
using System.IO;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Console = Colorful.Console;

namespace AppAuthentication
{
    internal static class WebHostBuilderExtensions
    {
        internal static WebHostBuilder CreateDefaultBuilder(WebHostBuilderOptions options)
        {
            var builder = new WebHostBuilder();

            builder.UseEnvironment(options.HostingEnvironment);
            var fullPath = Directory.GetCurrentDirectory();

            if (!string.IsNullOrWhiteSpace(Path.GetDirectoryName(options.ConfigFile)))
            {
                fullPath = Path.GetDirectoryName(options.ConfigFile);
            }

            builder.UseContentRoot(fullPath);

            var defaultConfigName = !string.IsNullOrWhiteSpace(options.ConfigFile) ? Path.GetFileName(options.ConfigFile) : "appsettings.json";

            if (options.Verbose)
            {
                Console.WriteLine($"ContentRoot:{fullPath}", color: Color.Green);
            }

            var config = new ConfigurationBuilder();

            config.AddEnvironmentVariables(prefix: "ASPNETCORE_");

            // appsettings file or others
            config.AddJsonFile(Path.Combine(fullPath, $"{defaultConfigName.Split(".")[0]}.json"), optional: true)
                  .AddJsonFile(Path.Combine(fullPath, $"{defaultConfigName.Split(".")[0]}.{options.HostingEnvironment}.json"), optional: true);

            if (options.Arguments != null)
            {
                config.AddCommandLine(options.Arguments);
            }

            if (options.Verbose
                && options.Level == LogLevel.Debug)
            {
                config.Build().DebugConfigurations();
            }

            builder.UseConfiguration(config.Build());

            builder
                .UseKestrel()
                .UseUrls(string.Format(Constants.HostUrl, Constants.MsiLocalhostUrl, options.Port))
                .ConfigureLogging(logger =>
                {
                    if (options.Verbose)
                    {
                        logger.AddConsole();
                        logger.AddDebug();
                    }
                });

            builder
                .ConfigureServices(services =>
                {
                    services.AddSingleton(options);
                    if (options.Verbose)
                    {
                        services.AddLogging(x => x.AddFilter((loglevel) =>
                        {
                            return loglevel == options.Level;
                        }));
                    }

                    services.AddSingleton(PhysicalConsole.Singleton);
                });

            return builder;
        }
    }
}
