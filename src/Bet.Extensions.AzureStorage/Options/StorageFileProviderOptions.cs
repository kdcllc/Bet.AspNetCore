namespace Bet.Extensions.AzureStorage.Options
{
    public class StorageFileProviderOptions
    {
        /// <summary>
        /// Http Request path to be served from Azure Storage Account.
        /// </summary>
        public string RequestPath { get; set; }

        /// <summary>
        /// Azure Storage Container name.
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Enable Directory browsing for the Azure Storage files.
        /// </summary>
        public bool EnableDirectoryBrowsing { get; set; } = false;

        /// <summary>
        /// Used to reference the Azure Storage Instance.
        /// </summary>
        internal string AzureStorageConfiguration { get; set; }
    }
}
