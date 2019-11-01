using System;
using System.Collections.Generic;

using Bet.Extensions.HealthChecks.AzureStorage;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureStorageHealthChecksBuilderExtensions
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
            IEnumerable<string>? tags = default)
        {
            builder.Services.AddOptions<StorageAccountOptions>(name)
                            .Configure((opt) =>
                            {
                                opt.ContainerName = containerName;
                            });

            RegisterOptions(builder, name, setup);

            builder.AddCheck<AzureBlobStorageHealthCheck>(name, failureStatus ?? HealthStatus.Degraded, tags);

            return builder;
        }

        /// <summary>
        /// Adds a queue check.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="name"></param>
        /// <param name="queueName"></param>
        /// <param name="setup"></param>
        /// <param name="failureStatus"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddAzureQueuetorageCheck(
            this IHealthChecksBuilder builder,
            string name,
            string queueName,
            Action<StorageAccountOptions> setup,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default)
        {
            builder.Services.AddOptions<StorageAccountOptions>(name)
                            .Configure((opt) =>
                            {
                                opt.QueueName = queueName;
                            });

            RegisterOptions(builder, name, setup);

            builder.AddCheck<AzureQueueStorageHealthCheck>(name, failureStatus ?? HealthStatus.Degraded, tags);

            return builder;
        }

        private static void RegisterOptions(
            IHealthChecksBuilder builder,
            string name,
            Action<StorageAccountOptions> setup)
        {
            var options = new StorageAccountOptions();
            setup?.Invoke(options);

            builder.Services.ConfigureOptions<StorageAccountOptionsSetup>();

            builder.Services.AddOptions<StorageAccountOptions>(name)
                .Configure((opt) =>
                {
                    opt.ConnectionString = options.ConnectionString;
                    opt.Name = options.Name;
                    opt.Token = options.Token;
                });
        }
    }
}
