using System;
using System.Collections.Generic;
using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;
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
        /// <param name="sectionName">
        /// The name of the configuration section for Azure Storage Account.
        /// Example: AzureStorage:DefaultAccount
        /// The default is <see cref="AzureStorageConstants.DefaultAccount"/>.
        /// </param>
        /// <param name="rootSectionName">
        /// The root section name for Azure Storage Account.
        /// Example: AzureStorage:DefaultAccount
        /// The default is <see cref="AzureStorageConstants.AzureStorage"/>.
        /// </param>
        /// <param name="failureStatus">The failure status to be returned. The default is 'HealthStatus.Degraded'.</param>
        /// <param name="tags">The optional tags.</param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddAzureBlobStorageCheck(
            this IHealthChecksBuilder builder,
            string name,
            string containerName,
            Action<StorageAccountOptions>? setup = null,
            string sectionName = AzureStorageConstants.DefaultAccount,
            string rootSectionName = AzureStorageConstants.AzureStorage,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default)
        {
            // register azure storage account with the container to be monitored.
            builder.Services
                    .AddAzureStorageAccount(name, sectionName, rootSectionName, setup)
                    .AddAzureBlobContainer(name, containerName);

            builder.AddCheck<AzureBlobStorageHealthCheck>(name, failureStatus ?? HealthStatus.Degraded, tags);

            return builder;
        }

        /// <summary>
        /// Adds a queue check.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="name"></param>
        /// <param name="queueName"></param>
        /// <param name="visibility"></param>
        /// <param name="setup"></param>
        /// <param name="sectionName"></param>
        /// <param name="rootSectionName"></param>
        /// <param name="failureStatus"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static IHealthChecksBuilder AddAzureQueuetorageCheck(
            this IHealthChecksBuilder builder,
            string name,
            string queueName,
            TimeSpan? visibility = null,
            Action<StorageAccountOptions>? setup = null,
            string sectionName = AzureStorageConstants.DefaultAccount,
            string rootSectionName = AzureStorageConstants.AzureStorage,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default)
        {
            builder.Services.AddAzureStorageAccount(name, sectionName, rootSectionName, setup)
                            .AddAzureQueue(name, queueName, visibility);

            builder.AddCheck<AzureQueueStorageHealthCheck>(name, failureStatus ?? HealthStatus.Degraded, tags);

            return builder;
        }
    }
}
