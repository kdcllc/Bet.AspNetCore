using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationLoggingDebugExtensions
    {
        /// <summary>
        /// Displays all of the application configurations based on the Configuration Provider.
        /// </summary>
        /// <param name="config"></param>
        public static void DebugConfigurations(this IConfigurationRoot config)
        {
            using (var logFactory = GetLoggerFactory())
            {
                var logger = logFactory.CreateLogger("Program");
                var allConfigurations = config.GetDebugView();
                logger.LogDebug(allConfigurations);
            }
        }

        /// <summary>
        /// Displays all of the application configurations based on the Configuration Provider.
        /// </summary>
        /// <param name="config"></param>
        public static void DebugConfigurationsWithSerilog(this IConfigurationRoot config)
        {
            var logger = new LoggerConfiguration()
                                              .MinimumLevel.Debug()
                                              .WriteTo.Debug()
                                              .WriteTo.Console()
                                              .CreateLogger();

            var allConfigurations = config.GetDebugView();
            logger.Debug(allConfigurations);
        }

#if NETSTANDARD2_0
        /// <summary>
        /// Generates a human-readable view of the configuration showing where each value came from.
        /// In version 3.0 this can be utilized directly.
        /// https://github.com/aspnet/Extensions/blob/d7f8e253d414ce6053ad59b6f974621d5620c0da/src/Configuration/Config.Abstractions/src/ConfigurationRootExtensions.cs#L15-L74.
        /// </summary>
        /// <returns> The debug view. </returns>
        public static string GetDebugView(this IConfigurationRoot root)
        {
            void RecurseChildren(
                StringBuilder stringBuilder,
                IEnumerable<IConfigurationSection> children,
                string indent)
            {
                foreach (var child in children)
                {
                    var (value, provider) = GetValueAndProvider(root, child.Path);

                    if (provider != null)
                    {
                        stringBuilder
                            .Append(indent)
                            .Append(child.Key)
                            .Append("=")
                            .Append(value)
                            .Append(" (")
                            .Append(provider)
                            .AppendLine(")");
                    }
                    else
                    {
                        stringBuilder
                            .Append(indent)
                            .Append(child.Key)
                            .AppendLine(":");
                    }

                    RecurseChildren(stringBuilder, child.GetChildren(), indent + "  ");
                }
            }

            var builder = new StringBuilder();

            RecurseChildren(builder, root.GetChildren(), string.Empty);

            return builder.ToString();
        }

        private static (string value, IConfigurationProvider provider) GetValueAndProvider(
            IConfigurationRoot root,
            string key)
        {
            foreach (var provider in root.Providers.Reverse())
            {
                if (provider.TryGet(key, out var value))
                {
                    return (value, provider);
                }
            }

            return (string.Empty, default!);
        }
#endif

        /// <summary>
        ///  Logging migration https://docs.microsoft.com/en-us/aspnet/core/migration/logging-nonaspnetcore?view=aspnetcore-2.2.
        /// </summary>
        /// <returns></returns>
        private static ILoggerFactory GetLoggerFactory()
        {
            ILoggerFactory? result = null;

#if NETSTANDARD2_1
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
