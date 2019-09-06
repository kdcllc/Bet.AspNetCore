using System;

using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public class DefaultStorageTableBuilder : IStorageTableBuilder
    {
        private readonly string _sectionAzureStorageName;

        public DefaultStorageTableBuilder(IServiceCollection services, string sectionAzureStorageName)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            _sectionAzureStorageName = sectionAzureStorageName;
        }

        /// <inheritdoc/>
        public IServiceCollection Services { get; }

        /// <inheritdoc/>
        public IStorageTableBuilder AddTable<TOptions>(
            string sectionAzureStorageName = null,
            Action<TOptions> configure = null) where TOptions : StorageTableOptions
        {
            var finalSectionAzureStorageName = (sectionAzureStorageName ?? _sectionAzureStorageName) ?? string.Empty;

            Services.AddAzureStorage(finalSectionAzureStorageName);

            Services.ConfigureOptions<TOptions>(Constants.StorageTables, (config, path, options) =>
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

            Services.TryAddTransient<IStorageTable<TOptions>, StorageTable<TOptions>>();

            return this;
        }
    }
}
