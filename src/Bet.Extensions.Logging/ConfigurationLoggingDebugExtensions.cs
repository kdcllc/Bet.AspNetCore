using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Collections.Generic;

namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationLoggingDebugExtensions
    {
        /// <summary>
        /// Displays all of the application configurations based on the Configuration Provider.
        /// </summary>
        /// <param name="config"></param>
        public static void DebugConfigurations(
            this IConfigurationRoot config)
        {
            var logFactory = GetLoggerFactory();

            var logger = logFactory.CreateLogger("Program");
            var allConfigurations = GetAllConfigurations(config);

            foreach (var provider in GetConfigurationsByProvider(config.Providers, allConfigurations))
            {
                logger.LogDebug("Configuration Provider: {name} - Count: {count}", provider.Key, provider.Value.Count);

                foreach (var (path, value) in provider.Value)
                {
                   logger.LogDebug("{provider} - {location} - {value}", provider.Key, path, value);
                }
            }
        }

        /// <summary>
        /// Displays all of the application configurations based on the Configuration Provider.
        /// </summary>
        /// <param name="config"></param>
        public static void DebugConfigurationsWithSerilog(
            this IConfigurationRoot config)
        {
            var logger = new LoggerConfiguration()
                                              .ReadFrom.Configuration(config)
                                              .WriteTo.Console()
                                              .CreateLogger();

            var allConfigurations = GetAllConfigurations(config);

            foreach (var provider in GetConfigurationsByProvider(config.Providers, allConfigurations))
            {
                logger.Debug("Configuration Provider: {name} - Count: {count}", provider.Key, provider.Value.Count);

                foreach (var (path, value) in provider.Value)
                {
                    logger.Debug("{provider} - {location} - {value}", provider.Key, path, value);
                }
            }
        }

        private static Dictionary<string, List<(string path, string value)>> GetConfigurationsByProvider(
            IEnumerable<IConfigurationProvider> providers,
            List<(string path, string value)> allConfigs)
        {
            var dict = new Dictionary<string, List<(string path, string value)>>();

            foreach (var provider in providers)
            {
                var name = provider.GetType().Name;

                var providersConfigs = new List<(string path, string value)>();

                foreach (var (path, value) in allConfigs)
                {
                    if (provider.TryGet(path, out var val))
                    {
                        providersConfigs.Add((path, value));
                    }
                }

                dict.Add($"{name}-{provider.GetHashCode()}", providersConfigs);
            }

            return dict;
        }

        private static List<(string path, string value)> GetAllConfigurations(IConfiguration config)
        {
            var configs = new List<(string path, string value)>();

            foreach (var pair in config.GetChildren())
            {
                configs.Add((pair.Path, pair.Value));
                configs.AddRange(GetAllConfigurations(pair));
            }
            return configs;
        }

        /// <summary>
        ///  Logging migration https://docs.microsoft.com/en-us/aspnet/core/migration/logging-nonaspnetcore?view=aspnetcore-2.2
        /// </summary>
        /// <returns></returns>
        private static ILoggerFactory GetLoggerFactory()
        {
            ILoggerFactory result = null;
#if NETCOREAPP3_0
            result = LoggerFactory.Create(builder =>
            {
                builder.AddDebug();
                builder.AddConsole();
                builder.AddFilter((_) => true);
            });
#else
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.AddConsole()
                  .AddFilter((_) => true);
            });

            result = serviceCollection.BuildServiceProvider()
                    .GetService<ILoggerFactory>();
#endif
            return result;
        }
    }
}
