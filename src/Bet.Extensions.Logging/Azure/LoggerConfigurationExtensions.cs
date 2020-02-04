using System;

using Bet.Extensions.Logging.Azure;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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
        /// <param name="applicationName">
        /// The name for the application logs in Azure Log Analytics. The name can't contain any characters.
        /// The Default value is <see cref="HostDefaults.ApplicationKey"/>.
        /// </param>
        /// <param name="batchSize">The size of the batch to send to Azue Log Analytics.</param>
        /// <returns></returns>
        public static LoggerConfiguration AddAzureLogAnalytics(
            this LoggerConfiguration loggerConfiguration,
            IConfiguration configuration,
            string sectionName = "AzureLogAnalytics",
            bool enableValidation = true,
            string? applicationName = null,
            int batchSize = 10)
        {
            // write to Log Analytics
            var azureAnalyticsOptions = configuration.Bind<AzureLogAnalyticsOptions>(sectionName, enableValidation);

            if (!string.IsNullOrEmpty(azureAnalyticsOptions.WorkspaceId))
            {
                var appName = applicationName ?? configuration[HostDefaults.ApplicationKey];

                loggerConfiguration.WriteTo.AzureAnalytics(
                    azureAnalyticsOptions.WorkspaceId,
                    azureAnalyticsOptions.AuthenticationId,
                    logName: appName.KeepAllLetters(),
                    batchSize: batchSize);
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
#pragma warning disable CA2000 // Dispose objects before losing scope
                // https://github.com/serilog/serilog-sinks-applicationinsights/issues/121
                var telemetryClient = TelemetryConfiguration.CreateDefault();
#pragma warning restore CA2000 // Dispose objects before losing scope

                if (appInsightConfig.EnableEvents)
                {
                    loggerConfiguration.WriteTo.ApplicationInsights(telemetryClient, TelemetryConverter.Events);
                }

                if (appInsightConfig.EnableTraces)
                {
                    loggerConfiguration.WriteTo.ApplicationInsights(telemetryClient, TelemetryConverter.Traces);
                }
            }

            return loggerConfiguration;
        }
    }
}
