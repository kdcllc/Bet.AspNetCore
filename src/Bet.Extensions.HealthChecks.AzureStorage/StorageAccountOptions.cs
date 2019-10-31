using System;
using System.Threading.Tasks;

using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage;

namespace Bet.Extensions.HealthChecks.AzureStorage
{
    public class StorageAccountOptions
    {
        /// <summary>
        /// Returns <see cref="CloudStorageAccount"/> instance based on configurations provided.
        /// </summary>
        public Lazy<Task<CloudStorageAccount>>? CloudStorageAccount { get; set; }

        /// <summary>
        /// Azure Storage Connection String.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Name of the Storage Account. Used with <see cref="AzureServiceTokenProvider"/>.
        /// This is enable only if ConnectionString is empty or null.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// SAS Token used with Name of the Storage Account.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        internal string ContainerName { get; set; } = string.Empty;

        internal string QueueName { get; set; } = string.Empty;
    }
}
