using System.Collections.Generic;

using Bet.Extensions.HealthChecks.ML;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MachineLearningHealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder AddMachineLearningModelCheck<TInput, TPrediction>(
            this IHealthChecksBuilder builder,
            string name,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default) where TInput : class, new() where TPrediction : class, new()
        {
            if (tags == default)
            {
                tags = new[] { "machine_learning" };
            }

            builder.AddCheck<MachineLearningHealthCheck<TInput, TPrediction>>(name, failureStatus ?? HealthStatus.Unhealthy, tags);

            return builder;
        }
    }
}
