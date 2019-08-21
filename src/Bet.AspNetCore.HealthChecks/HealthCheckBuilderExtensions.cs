using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Threading.Tasks;

using Bet.AspNetCore.HealthChecks.CertificateCheck;
using Bet.AspNetCore.HealthChecks.MemoryCheck;
using Bet.AspNetCore.HealthChecks.SigtermCheck;
using Bet.AspNetCore.HealthChecks.UriCheck;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddSslCertificateCheck(
            this IHealthChecksBuilder builder,
            string name,
            string baseUrl,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default)
        {
            builder.Services.AddHttpClient(name, (sp, config) =>
            {
                config.BaseAddress = new Uri(baseUrl);
            }).ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var handler = new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual,

                    ServerCertificateCustomValidationCallback = (httpRequestMessage, certificate, cetChain, sslPolicyErrors) =>
                    {
                        var expirationDate = DateTime.Parse(certificate.GetExpirationDateString());
                        if (expirationDate - DateTime.Today < TimeSpan.FromDays(30))
                        {
                            throw new Exception("Time to renew the certificate!");
                        }

                        if (sslPolicyErrors == SslPolicyErrors.None)
                        {
                            return true;
                        }
                        else
                        {
                            throw new Exception("Cert policy errors: " + sslPolicyErrors.ToString());
                        }
                    }
                };
                return handler;
            });

            builder.AddCheck<SslCertificateHealthCheck>(name, failureStatus, tags);
            return builder;
        }

        /// <summary>
        /// Add SIGTERM Healcheck that provides notification for orchestrator with unhealthy status once the application begins to shut down.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="name">The name of the HealthCheck.</param>
        /// <param name="failureStatus">The <see cref="HealthStatus"/>The type should be reported when the health check fails. Optional. If <see langword="null"/> then.</param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddSigtermCheck(
            this IHealthChecksBuilder builder,
            string name,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = default)
        {
            builder.AddCheck<SigtermHealthCheck>(name, failureStatus, tags);

            return builder;
        }

        public static IHealthChecksBuilder AddUriHealthCheck(
            this IHealthChecksBuilder builder,
            string name,
            Action<UriOptionsSetup> uriOptions,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default)
        {
            // TODO ability to add custom httpclient for the calls.
            var client = builder.Services.AddHttpClient(name, (sp, config) =>
            {
                // config.Timeout = TimeSpan.FromSeconds(10);
            });

            var check = new UriHealthCheckBuilder(builder.Services, name);

            check.Add(uriOptions);

            builder.AddCheck<UriHealthCheck>(name, failureStatus, tags);

            return builder;
        }

        /// <summary>
        /// Add a HealthCHeck for a single <see cref="Uri"/> or many <see cref="Uri"/>s instances.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="name">The name of the HealthCheck.</param>
        /// <param name="registration">The <see cref="Action{UriHealthCheckBuilder}"/> delegate.</param>
        /// <param name="failureStatus">The <see cref="HealthStatus"/>The type should be reported when the health check fails. Optional. If <see langword="null"/> then.</param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddUriHealthCheck(
            this IHealthChecksBuilder builder,
            string name,
            Action<UriHealthCheckBuilder> registration,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default)
        {
            // TODO ability to add custom httpclient for the calls.
            var client = builder.Services.AddHttpClient(name, (sp, config) =>
            {
                // config.Timeout = TimeSpan.FromSeconds(10);
            });

            var check = new UriHealthCheckBuilder(builder.Services, name);

            registration(check);

            builder.AddCheck<UriHealthCheck>(name, failureStatus, tags);

            return builder;
        }

        /// <summary>
        /// Add a HealthCheck for Garbage Collection Memory check.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="name">The name of the HealthCheck.</param>
        /// <param name="failureStatus">The <see cref="HealthStatus"/>The type should be reported when the health check fails. Optional. If <see langword="null"/> then.</param>
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

        /// <summary>
        /// Enable usage of the basic liveness check that returns 200 http status code.
        /// Default registered health check is self.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="healthCheckPath"></param>
        /// <param name="healthCheckOptions"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseLivenessHealthCheck(
            this IApplicationBuilder builder,
            string healthCheckPath = "/liveness",
            HealthCheckOptions healthCheckOptions = default)
        {
            if (healthCheckOptions == default)
            {
                // Exclude all checks and return a 200-Ok. Default registered health check is self.
                healthCheckOptions = new HealthCheckOptions { Predicate = (p) => false };
            }

            builder.UseHealthChecks(healthCheckPath, healthCheckOptions);

            return builder;
        }

        public static IApplicationBuilder UseHealthyHealthCheck(
            this IApplicationBuilder builder,
            string healthCheckPath = "/healthy",
            HealthCheckOptions healthCheckOptions = default)
        {
            if (healthCheckOptions == default)
            {
                healthCheckOptions = new HealthCheckOptions { ResponseWriter = WriteResponse };
            }

            builder.UseHealthChecks(healthCheckPath, healthCheckOptions);

            return builder;
        }

        /// <summary>
        /// Custom HealthCheck <see cref="HealthReport"/> renderer.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="result"></param>
        /// <returns></returns>
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
