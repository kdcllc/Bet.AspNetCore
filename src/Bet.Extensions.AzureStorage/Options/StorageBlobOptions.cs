namespace Bet.Extensions.AzureStorage.Options
{
    public class StorageBlobOptions : StorageOptionsBase
    {
        /// <summary>
        /// Azure Storage Blob Container Name.
        /// </summary>
        public string ContainerName { get; set; } = string.Empty;
    }
}
