using System;
using System.Linq;

using Bet.Extensions.AzureStorage.Builder;
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
        /// <param name="services">The services.</param>
        /// <param name="azureStorageSectionName">The Default value is ''. This corresponds to 'Account' in 'AzureStorage' configuration.</param>
        /// <returns></returns>
        public static IServiceCollection AddAzureStorage(
            this IServiceCollection services,
            string azureStorageSectionName = "")
        {
            return services.AddAzureStorage(o=> { }, azureStorageSectionName);
        }

        /// <summary>
        /// Adds Azure Storage Account Configurations.
        /// </summary>
        /// <param name="services">The services</param>
        /// <param name="configure">A Delegate to configure the <see cref="StorageAccountOptions"/>.</param>
        /// <param name="azureStorageSectionName">The Default value is ''. This corresponds to 'Account' in 'AzureStorage' configuration.</param>
        /// <returns></returns>
        public static IServiceCollection AddAzureStorage(
            this IServiceCollection services,
            Action<StorageAccountOptions> configure,
            string azureStorageSectionName = "")
        {
            var registered = services.Select(x => x.ImplementationInstance).OfType<ConfigureNamedOptions<StorageAccountOptions>>();

            if (!registered.Any() || (!registered.Any(p=> p.Name == azureStorageSectionName)) )
            {
                services.ConfigureOptions<StorageAccountOptionsSetup>();

                services.AddOptions<StorageAccountOptions>(azureStorageSectionName)
                    .Configure(options => configure(options));
            }

            return services;
        }

        /// <summary>
        /// Adds Configuration for UseStaticFiles middleware to use Azure Storage container.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAzureStorageForStaticFiles<TOptions>(
            this IServiceCollection services)
            where TOptions : StorageFileProviderOptions
        {
            return services.AddAzureStorageForStaticFiles<TOptions>("");
        }

        /// <summary>
        /// Adds Configuration for UseStaticFiles middleware to use Azure Storage container.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services">The services.</param>
        /// <param name="sectionAzureStorageName">The configuration name of the section used for Azure Storage Account.</param>
        /// <returns></returns>
        public static IServiceCollection AddAzureStorageForStaticFiles<TOptions>(
            this IServiceCollection services,
            string sectionAzureStorageName)
            where TOptions : StorageFileProviderOptions
        {
            return services.AddAzureStorageForStaticFiles<TOptions>(sectionAzureStorageName, o => { });
        }

        /// <summary>
        /// Adds Configuration for UseStaticFiles middleware to use Azure Storage container.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services">The services.</param>
        /// <param name="sectionAzureStorageName">The configuration name of the section used for Azure Storage Account.</param>
        /// <param name="configure">The delegate to configure the options. </param>
        /// <returns></returns>
        public static IServiceCollection AddAzureStorageForStaticFiles<TOptions>(
            this IServiceCollection services,
            string sectionAzureStorageName,
            Action<TOptions> configure)
            where TOptions : StorageFileProviderOptions
        {
            services.AddAzureStorage(sectionAzureStorageName);

            services.ConfigureOptions<TOptions>("StorageFileProviders", (config, path, options) =>
            {
                options.AzureStorageConfiguration = sectionAzureStorageName;

                if (path != typeof(TOptions).Name)
                {
                    path = ConfigurationPath.Combine(path, typeof(TOptions).Name);
                }
                var section = config.GetSection(path);
                section.Bind(options);

                configure(options);
            });

            return services;
        }

        public static IStorageBlobBuilder AddStorageBlob(
            this IServiceCollection services,
            string sectionAzureStorageName = default)
        {
            return new DefaultStorageBlobBuilder(services, sectionAzureStorageName);
        }

        public static IStorageQueueBuilder AddStorageQueue(
            this IServiceCollection services,
            string sectionAzureStorageName = default)
        {
            return new DefaultStorageQueueBuilder(services, sectionAzureStorageName);
        }
    }
}
