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
                                              .ReadFrom.Configuration(config)
                                              .WriteTo.Console()
                                              .CreateLogger();

            var allConfigurations = config.GetDebugView();
            logger.Debug(allConfigurations);
        }


        /// <summary>
        ///  Logging migration https://docs.microsoft.com/en-us/aspnet/core/migration/logging-nonaspnetcore?view=aspnetcore-2.2.
        /// </summary>
        /// <returns></returns>
        private static ILoggerFactory GetLoggerFactory()
        {
            ILoggerFactory result = null;
#if NETSTANDARD2_1 || NETCOREAPP3_0
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
