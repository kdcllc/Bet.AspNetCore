using System;

using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureStorageAccountBuilderExtensions
    {
        public static IAzureStorageAccountBuilder AddAzureStorageAccount(
            this IServiceCollection services,
            string named = "",
            string sectionName = AzureStorageConstants.DefaultAccount,
            string rootSectionName = AzureStorageConstants.AzureStorage,
            Action<StorageAccountOptions>? configure = default)
        {
            var builder = new AzureStorageAccountBuilder(services, named);

            services.ConfigureOptions<StorageAccountOptionsSetup>();

            services.AddOptions<StorageAccountOptions>(named)
                    .Configure<IConfiguration>((options, configuration) =>
                    {
                        var basePath = configuration.GetSection(rootSectionName).Exists() ? rootSectionName : options.GetType().Name;

                        // AzureStorage/Account AzureStorage/ModelSource
                        basePath = ConfigurationPath.Combine(basePath, sectionName);

                        var section = configuration.GetSection(basePath);
                        section.Bind(options);

                        configure?.Invoke(options);
                    });

            return builder;
        }

        public static IAzureStorageAccountBuilder AddAzureBlobContainer(
            this IAzureStorageAccountBuilder builder,
            string named,
            string containerName)
        {
            builder.AddAzureBlobContainer<StorageBlobOptions>(
                named: named,
                configure: options => options.ContainerName = containerName);

            return builder;
        }

        public static IAzureStorageAccountBuilder AddAzureBlobContainer<TOptions>(
            this IAzureStorageAccountBuilder builder,
            string named = "",
            string sectionName = "",
            string rootSectionName = AzureStorageConstants.StorageBlobs,
            Action<TOptions>? configure = default)
                where TOptions : StorageBlobOptions
        {
            builder.Services.TryAddSingleton<IStorageBlob<TOptions>, StorageBlob<TOptions>>();

            builder.AddOptions(named, sectionName, rootSectionName, configure);

            return builder;
        }

        public static IAzureStorageAccountBuilder AddAzureQueue(
            this IAzureStorageAccountBuilder builder,
            string named,
            string queueName,
            TimeSpan? visibilityTimeout = null)
        {
            return builder.AddAzureQueue<StorageQueueOptions>(named, configure: options =>
            {
                options.QueueName = queueName;
                options.VisibilityTimeout = visibilityTimeout;
            });
        }

        public static IAzureStorageAccountBuilder AddAzureQueue<TOptions>(
            this IAzureStorageAccountBuilder builder,
            string named = "",
            string sectionName = "",
            string rootSectionName = AzureStorageConstants.StorageQueues,
            Action<TOptions>? configure = default)
                where TOptions : StorageQueueOptions
        {
            builder.Services.TryAddSingleton<IStorageQueue<TOptions>, StorageQueue<TOptions>>();

            builder.AddOptions(named, sectionName, rootSectionName, configure);

            return builder;
        }

        public static IAzureStorageAccountBuilder AddAzureTable(
            this IAzureStorageAccountBuilder builder,
            string named,
            string tableName)
        {
           return builder.AddAzureTable<StorageTableOptions>(named, configure: options =>
           {
               options.TableName = tableName;
           });
        }

        public static IAzureStorageAccountBuilder AddAzureTable<TOptions>(
                this IAzureStorageAccountBuilder builder,
                string named = "",
                string sectionName = "",
                string rootSectionName = AzureStorageConstants.StorageTables,
                Action<TOptions>? configure = default)
        where TOptions : StorageTableOptions
        {
            builder.Services.TryAddSingleton<IStorageTable<TOptions>, StorageTable<TOptions>>();

            return builder.AddOptions(named, sectionName, rootSectionName, configure);
        }

        /// <summary>
        /// Adds Configuration for UseStaticFiles middleware to use Azure Storage container.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="builder"></param>
        /// <param name="named"></param>
        /// <param name="sectionName"></param>
        /// <param name="rootSectionName"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IAzureStorageAccountBuilder AddAzureStorageForStaticFiles<TOptions>(
             this IAzureStorageAccountBuilder builder,
             string named = "",
             string sectionName = "",
             string rootSectionName = AzureStorageConstants.StorageFileProviders,
             Action<TOptions>? configure = null)
                where TOptions : StorageFileProviderOptions
        {
            return builder.AddOptions(named, sectionName, rootSectionName, configure);
        }

        private static IAzureStorageAccountBuilder AddOptions<TOptions>(
            this IAzureStorageAccountBuilder builder,
            string named,
            string sectionName,
            string rootSectionName,
            Action<TOptions>? configure = default)
                where TOptions : StorageOptionsBase
        {
            if (string.IsNullOrEmpty(sectionName))
            {
                sectionName = typeof(TOptions).Name;
            }

            builder.Services.AddOptions<TOptions>(named)
                    .Configure<IConfiguration>((options, configuration) =>
                    {
                        options.AccountName = builder.AccountName;

                        var rootSection = configuration.GetSection(rootSectionName).Exists() ? rootSectionName : sectionName;
                        var configPath = ConfigurationPath.Combine(rootSection, sectionName);

                        var section = configuration.GetSection(configPath);
                        section.Bind(options);

                        configure?.Invoke(options);
                    });

            return builder;
        }
    }
}
