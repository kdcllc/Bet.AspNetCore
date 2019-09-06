using System;
using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public class DefaultStorageQueueBuilder : IStorageQueueBuilder
    {
        private readonly string _sectionAzureStorageName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultStorageQueueBuilder"/> class.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="sectionAzureStorageName"></param>
        public DefaultStorageQueueBuilder(IServiceCollection services, string sectionAzureStorageName)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            _sectionAzureStorageName = sectionAzureStorageName;
        }

        /// <inheritdoc/>
        public IServiceCollection Services { get; }

        /// <inheritdoc/>
        public IStorageQueueBuilder AddQueue<TOptions>(
            string sectionAzureStorageName = default,
            Action<TOptions> configure = default) where TOptions : StorageQueueOptions
        {
            var finalSectionAzureStorageName = (sectionAzureStorageName ?? _sectionAzureStorageName) ?? string.Empty;

            Services.AddAzureStorage(finalSectionAzureStorageName);

            Services.ConfigureOptions<TOptions>(Constants.StorageQueues, (config, path, options) =>
            {
                options.AzureStorageConfiguration = finalSectionAzureStorageName;
                if (path != typeof(TOptions).Name)
                {
                    path = ConfigurationPath.Combine(path, typeof(TOptions).Name);
                }

                var section = config.GetSection(path);
                section.Bind(options);

                configure?.Invoke(options);
            });

            Services.TryAddTransient<IStorageQueue<TOptions>, StorageQueue<TOptions>>();

            return this;
        }
    }
}
