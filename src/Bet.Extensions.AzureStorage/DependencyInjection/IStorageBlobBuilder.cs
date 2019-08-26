using System;

using Bet.Extensions.AzureStorage.Options;

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Azure Storage Blob Builder.
    /// </summary>
    public interface IStorageBlobBuilder
    {
        /// <summary>
        /// Dependency Injection services.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Adds Azure Storage Container with provided configuration.
        /// </summary>
        /// <typeparam name="TOptions">The options configuration type to be used to register Azure Storage Blob container.</typeparam>
        /// <param name="sectionAzureStorageName">The Azure Storage Account Configuration section. The default value is null.</param>
        /// <param name="configure">The delegate to configure the options. The default value is null.</param>
        /// <returns></returns>
        IStorageBlobBuilder AddBlobContainer<TOptions>(string sectionAzureStorageName = default, Action<TOptions> configure = default) where TOptions : StorageBlobOptions;
    }
}
