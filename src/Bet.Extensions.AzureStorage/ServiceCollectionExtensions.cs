using System;
using System.Linq;

using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Azure Storage Account Configurations.
        /// </summary>
        /// <param name="services">The collection of Dependency Injection services.</param>
        /// <param name="azureStorageSectionName">The Default value is 'Account'. This corresponds to 'Account' in 'AzureStorage' configuration.</param>
        /// <param name="configure">The delegate to configure the <see cref="StorageAccountOptions"/>.</param>
        /// <returns></returns>
        public static IServiceCollection AddAzureStorage(
            this IServiceCollection services,
            string azureStorageSectionName = "",
            Action<StorageAccountOptions> configure = null)
        {
            var registered = services.Select(x => x.ImplementationInstance).OfType<ConfigureNamedOptions<StorageAccountOptions>>();

            if (!registered.Any() || (!registered.Any(p => p.Name == azureStorageSectionName)))
            {
                services.ConfigureOptions<StorageAccountOptionsSetup>();

                services.AddOptions<StorageAccountOptions>(azureStorageSectionName)
                    .Configure(options => configure?.Invoke(options));
            }

            return services;
        }

        /// <summary>
        /// Adds Configuration for UseStaticFiles middleware to use Azure Storage container.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services">The collection of Dependency Injection services.</param>
        /// <param name="azureStorageSectionName">The Default value is 'Account'. This corresponds to 'Account' in 'AzureStorage' configuration.</param>
        /// <param name="configure">The delegate to configure the <see cref="StorageAccountOptions"/>.</param>
        /// <returns></returns>
        public static IServiceCollection AddAzureStorageForStaticFiles<TOptions>(
            this IServiceCollection services,
            string azureStorageSectionName = "",
            Action<TOptions> configure = null)
            where TOptions : StorageFileProviderOptions
        {
            services.AddAzureStorage(azureStorageSectionName);

            services.ConfigureOptions<TOptions>(Constants.StorageFileProviders, (config, path, options) =>
            {
                options.AzureStorageConfiguration = azureStorageSectionName;

                if (path != typeof(TOptions).Name)
                {
                    path = ConfigurationPath.Combine(path, typeof(TOptions).Name);
                }

                var section = config.GetSection(path);
                section.Bind(options);

                configure?.Invoke(options);
            });

            return services;
        }

        /// <summary>
        /// Adds Azure Storage Blob to Azure Storage Account.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="azureStorageSectionName"></param>
        /// <returns></returns>
        public static IStorageBlobBuilder AddStorageBlob(
            this IServiceCollection services,
            string azureStorageSectionName = default)
        {
            return new DefaultStorageBlobBuilder(services, azureStorageSectionName);
        }

        /// <summary>
        /// Adds Azure Storage Queue to Azure Storage Account.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="azureStorageSectionName"></param>
        /// <returns></returns>
        public static IStorageQueueBuilder AddStorageQueue(
            this IServiceCollection services,
            string azureStorageSectionName = default)
        {
            return new DefaultStorageQueueBuilder(services, azureStorageSectionName);
        }

        /// <summary>
        /// Adds required default values for Azure Storage Table.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="azureStorageSectionName"></param>
        /// <returns></returns>
        public static IStorageTableBuilder AddStorageTable(this IServiceCollection services, string azureStorageSectionName = default)
        {
            return new DefaultStorageTableBuilder(services, azureStorageSectionName);
        }
    }
}
