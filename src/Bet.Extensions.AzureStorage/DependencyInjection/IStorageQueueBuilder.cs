using System;

using Bet.Extensions.AzureStorage.Options;

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Azure Storage Queue Builder.
    /// </summary>
    public interface IStorageQueueBuilder
    {
        /// <summary>
        /// Dependency Injection services.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Adds Azure Storage Queues with provided configuration.
        /// </summary>
        /// <typeparam name="TOptions">The options configuration type to be used to register Azure Storage Queue container.</typeparam>
        /// <param name="sectionAzureStorageName">The Azure Storage Account Configuration section. The default value is null.</param>
        /// <param name="configure">The delegate to configure the options. The default value is null.</param>
        /// <returns></returns>
        IStorageQueueBuilder AddQueue<TOptions>(string sectionAzureStorageName = default, Action<TOptions> configure = default) where TOptions : StorageQueueOptions;
    }
}
