using Bet.AspNetCore.HealthChecks.MemoryCheck;
using Bet.AspNetCore.HealthChecks.UriCheck;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddUriHealthCheck(
            this IHealthChecksBuilder builder,
            string name,
            Action<UriHealthCheckBuilder> registration,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default)
        {
            var client = builder.Services.AddHttpClient(name, (sp, config) =>
            {
                config.Timeout = TimeSpan.FromSeconds(10);
            });

            var check = new UriHealthCheckBuilder(builder.Services, name);

            registration(check);

            builder.AddCheck<UriHealthCheck>(name,failureStatus,tags);

            return builder;
        }

        /// <summary>
        /// Add a HealthCheck for Garbage Collection Memory check.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="name">The name of the HealthCheck.</param>
        /// <param name="failureStatus">The <see cref="HealthStatus"/>The type should be reported when the health check fails. Optional. If <see langword="null"/> then</param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="thresholdInBytes">The Threshold in bytes. The default is 1073741824 bytes or 1Gig.</param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddMemoryHealthCheck(
                    this IHealthChecksBuilder builder,
                    string name = "memory",
                    HealthStatus? failureStatus = null,
                    IEnumerable<string> tags = null,
                    long? thresholdInBytes = null)
        {
            // Register a check of type GCInfo.
            builder.AddCheck<MemoryHealthCheck>(name, failureStatus ?? HealthStatus.Degraded, tags);

            // Configure named options to pass the threshold into the check.
            if (thresholdInBytes.HasValue)
            {
                builder.Services.Configure<MemoryCheckOptions>(name, options =>
                {
                    options.Threshold = thresholdInBytes.Value;
                });
            }

            return builder;
        }

        public static Task WriteResponse(
            HttpContext httpContext,
            HealthReport result)
        {
            httpContext.Response.ContentType = "application/json";

            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", new JObject(result.Entries.Select(pair =>
                    new JProperty(pair.Key, new JObject(
                        new JProperty("status", pair.Value.Status.ToString()),
                        new JProperty("description", pair.Value.Description),
                        new JProperty("data", new JObject(pair.Value.Data.Select(
                            p => new JProperty(p.Key, p.Value))))))))));

            return httpContext.Response.WriteAsync(
                json.ToString(Formatting.Indented));
        }
    }
}
