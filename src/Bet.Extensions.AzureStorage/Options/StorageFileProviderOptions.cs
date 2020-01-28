namespace Bet.Extensions.AzureStorage.Options
{
    public class StorageFileProviderOptions : StorageOptionsBase
    {
        /// <summary>
        /// Http Request path to be served from Azure Storage Account.
        /// </summary>
        public string RequestPath { get; set; } = string.Empty;

        /// <summary>
        /// Azure Storage Container name.
        /// </summary>
        public string ContainerName { get; set; } = string.Empty;

        /// <summary>
        /// Enable Directory browsing for the Azure Storage files.
        /// </summary>
        public bool EnableDirectoryBrowsing { get; set; }
    }
}
