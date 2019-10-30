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
    }
}
