using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Security;

using Bet.AspNetCore.HealthChecks.AzureBlobStorage;
using Bet.AspNetCore.HealthChecks.CertificateCheck;
using Bet.AspNetCore.HealthChecks.MemoryCheck;
using Bet.AspNetCore.HealthChecks.SigtermCheck;
using Bet.AspNetCore.HealthChecks.UriCheck;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthChecksBuilderExtensions
    {
        /// <summary>
        /// Adds Azure Storage Health Check.
        /// </summary>
        /// <param name="builder">The hc builder.</param>
        /// <param name="name">The name of the hc.</param>
        /// <param name="containerName">The name of the container to be checked.</param>
        /// <param name="setup">The setup action for the hc.</param>
        /// <param name="failureStatus">The failure status to be returned. The default is 'HealthStatus.Degraded'.</param>
        /// <param name="tags">The optional tags.</param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddAzureBlobStorageCheck(
            this IHealthChecksBuilder builder,
            string name,
            string containerName,
            Action<StorageAccountOptions> setup,
            HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default)
        {
            var options = new StorageAccountOptions();
            setup?.Invoke(options);

            builder.Services.AddOptions<StorageAccountOptions>(name)
                .Configure((opt) =>
                {
                    opt.ConnectionString = options.ConnectionString;
                    opt.ContainerName = containerName;
                    opt.Name = options.Name;
                    opt.Token = options.Token;
                });

            builder.AddCheck<AzureBlobStorageHealthCheck>(name, failureStatus ?? HealthStatus.Degraded, tags);

            return builder;
        }

        /// <summary>
        /// Adds SSL Website Certificate check.
        /// </summary>
        /// <param name="builder">The hc builder.</param>
        /// <param name="name">The name of the  hc.</param>
        /// <param name="baseUrl">The website base url.</param>
        /// <param name="beforeSslExpriesDays">The number of days before SSL expires.</param>
        /// <param name="failureStatus">The failure status to be returned. The default is 'HealthStatus.Degraded'.</param>
        /// <param name="tags">The optional tags.</param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddSslCertificateCheck(
            this IHealthChecksBuilder builder,
            string name,
            string baseUrl,
            int beforeSslExpriesDays = 30,
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
                        if (expirationDate - DateTime.Today < TimeSpan.FromDays(beforeSslExpriesDays))
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

            builder.AddCheck<SslCertificateHealthCheck>(name, failureStatus ?? HealthStatus.Degraded, tags);
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
    }
}
