using Bet.AspNetCore.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Serilog
{
    public static class LoggerConfigurationExtensions
    {
        /// <summary>
        /// Adds Azure Log Analytics to the Serilog Sink.
        /// </summary>
        /// <param name="loggerConfiguration">The instance of LoggerConfiguration.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="sectionName">The options configuration section name.</param>
        /// <param name="enableValidation">The option to enable or disable options validations on the startup.</param>
        /// <param name="applicationName">The name for the application for the logs. The Default value is <see cref="WebHostDefaults.ApplicationKey"/></param>
        /// <returns></returns>
        public static LoggerConfiguration AddAzureLogAnalytics(
            this LoggerConfiguration loggerConfiguration,
            IConfiguration configuration,
            string sectionName = "AzureLogAnalytics",
            bool enableValidation = true,
            string applicationName = null)
        {
            // write to Log Analytics
            var azureAnalyticsOptions = configuration.Bind<AzureLogAnalyticsOptions>(sectionName, enableValidation);

            if (!string.IsNullOrEmpty(azureAnalyticsOptions.WorkspaceId))
            {
                var appName = configuration[WebHostDefaults.ApplicationKey];

                loggerConfiguration.WriteTo.AzureAnalytics(
                    azureAnalyticsOptions.WorkspaceId,
                    azureAnalyticsOptions.AuthenticationId,
                    logName: appName);
            }

            return loggerConfiguration;
        }

        /// <summary>
        /// Adds Azure ApplicationInsights Serilog Sink.
        /// </summary>
        /// <param name="loggerConfiguration">The instance of LoggerConfiguration.</param>
        /// <param name="configuration">The configuration instance.</param>
        /// <param name="sectionName">The options configuration section name.</param>
        /// <param name="enableValidation">The option to enable or disable options validations on the startup.</param>
        /// <returns></returns>
        public static LoggerConfiguration AddApplicationInsights(
            this LoggerConfiguration loggerConfiguration,
            IConfiguration configuration,
            string sectionName = "ApplicationInsights",
            bool enableValidation = true)
        {
            // writes to Application Insights window
            var appInsightConfig = configuration.Bind<ApplicationInsightsOptions>(sectionName, enableValidation);

            if (!string.IsNullOrEmpty(appInsightConfig.InstrumentationKey))
            {
                if (appInsightConfig.EnableEvents)
                {
                    loggerConfiguration.WriteTo.ApplicationInsightsEvents(appInsightConfig.InstrumentationKey);
                }
                if (appInsightConfig.EnableTraces)
                {
                    loggerConfiguration.WriteTo.ApplicationInsightsTraces(appInsightConfig.InstrumentationKey);
                }
            }

            return loggerConfiguration;
        }
    }
}
