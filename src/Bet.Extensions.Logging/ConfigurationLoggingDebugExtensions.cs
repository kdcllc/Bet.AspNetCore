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

        /// <summary>
        ///  Logging migration https://docs.microsoft.com/en-us/aspnet/core/migration/logging-nonaspnetcore?view=aspnetcore-2.2.
        /// </summary>
        /// <returns></returns>
        private static ILoggerFactory GetLoggerFactory()
        {
            return LoggerFactory.Create(builder =>
            {
                builder.AddDebug();
                builder.AddConsole();
                builder.AddFilter((_) => true);
            });
        }
    }
}
