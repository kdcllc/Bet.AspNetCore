using System.Collections.Generic;

using Bet.Extensions.HealthChecks.PingCheck;
using Bet.Extensions.HealthChecks.Publishers;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Collection of the Health Checks.
    /// </summary>
    public static class HealthChecksBuilderExtensions
    {
        /// <summary>
        /// Adds Ping HealthCheck.
        /// </summary>
        /// <param name="builder">The healthcheck builder.</param>
        /// <param name="name">The name of the healthcheck.</param>
        /// <param name="options">The options for the healthcheck.</param>
        /// <param name="tags">The optional tags.</param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddPingHealthCheck(
            this IHealthChecksBuilder builder,
            string name,
            PingHealthCheckOptions options,
            IEnumerable<string> tags = null)
        {
            builder.Services.Configure<PingHealthCheckOptions>(op =>
            {
                op.Host = options.Host;
                op.Timeout = options.Timeout;
            });

            builder.AddCheck<PingHealthCheck>(name, failureStatus: null, tags: tags);

            return builder;
        }

        /// <summary>
        /// Adds Socket Listener for Worker pods that create HealthChecks on TCP protocol.
        /// </summary>
        /// <param name="builder">The healthchecks builder.</param>
        /// <param name="port">The port to execute on.</param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddSocketListener(this IHealthChecksBuilder builder, int port)
        {
            builder.Services.AddSingleton<IHealthCheckPublisher>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<SocketHealthCheckPublisher>>();

                return new SocketHealthCheckPublisher(port, logger);
            });
            return builder;
        }

        /// <summary>
        /// Adds Logger HealthCheck publisher.
        /// </summary>
        /// <param name="builder">The healthchecks builder.</param>
        /// <param name="exclude">The list of the names of the healthchecks to exclude.</param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddLoggerPublisher(this IHealthChecksBuilder builder, IList<string> exclude = null)
        {
            exclude ??= new List<string> { string.Empty };

            builder.Services.AddSingleton<IHealthCheckPublisher>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<LoggingHealthCheckPublisher>>();

                return new LoggingHealthCheckPublisher(exclude, logger);
            });

            return builder;
        }
    }
}
