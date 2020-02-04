using Bet.Extensions.Logging.Azure;

using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LoggingServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Application Insights Telemetry based on Instrument Id.
        /// The instrument id is validated upon binding.
        /// </summary>
        /// <param name="services">The DI services.</param>
        /// <param name="sectionName">The configuration section name of the <see cref="ApplicationInsightsOptions"/>. The default is "ApplicationInsights".</param>
        /// <param name="validateOptions">The ability to validate the options binding.</param>
        /// <returns></returns>
        public static IServiceCollection AddAppInsightsTelemetry(
            this IServiceCollection services,
            string sectionName = "ApplicationInsights",
            bool validateOptions = true)
        {
            services.AddOptions<ApplicationInsightsServiceOptions>()
                 .Configure<IConfiguration>((options, configuration) =>
                 {
                     var instrumentId = configuration.Bind<ApplicationInsightsOptions>(sectionName, validateOptions);
                     options.InstrumentationKey = instrumentId.InstrumentationKey;
                 });

            services.AddApplicationInsightsTelemetry();

            return services;
        }
    }
}
