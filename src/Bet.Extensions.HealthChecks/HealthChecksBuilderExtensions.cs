using System.Collections.Generic;

using Bet.Extensions.HealthChecks.PingCheck;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Collection of the Health Checks.
    /// </summary>
    public static class HealthChecksBuilderExtensions
    {
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
    }
}
