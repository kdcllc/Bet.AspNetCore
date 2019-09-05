using System;

using Bet.Extensions.AzureStorage.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IStorageTableBuilder
    {
        /// <summary>
        /// Dependency Injection services.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Adds Azure Table Storage with provided configuration.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="sectionAzureStorageName"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        IStorageTableBuilder AddTable<TOptions>(string sectionAzureStorageName = default, Action<TOptions> configure = default) where TOptions : StorageTableOptions;
    }
}
