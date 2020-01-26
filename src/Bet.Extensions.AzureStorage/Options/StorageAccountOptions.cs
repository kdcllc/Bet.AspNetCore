using System;
using System.Threading.Tasks;

using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace Bet.Extensions.AzureStorage.Options
{
    /// <summary>
    /// Azure Storage Account based on configurations.
    /// </summary>
    public class StorageAccountOptions
    {
        /// <summary>
        /// Returns <see cref="CloudStorageAccount"/> instance based on configurations provided.
        /// </summary>
        public Lazy<Task<CloudStorageAccount>> CloudStorageAccount { get; set; }

        /// <summary>
        /// Azure Storage Connection String.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Name of the Storage Account. Used with <see cref="AzureServiceTokenProvider"/>.
        /// This is enable only if ConnectionString is empty or null.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// SAS Token used with Name of the Storage Account.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// The default value is 'AzureStorage'.
        /// </summary>
        public string RootSectionName { get; set; } = Constants.AzureStorage;

        /// <summary>
        /// The default value is empty for the non named option.
        /// </summary>
        internal string OptionName { get; set; } = string.Empty;
    }
}
