namespace Bet.Extensions.AzureStorage.Options
{
    public class StorageTableOptions : StorageOptionsBase
    {
        /// <summary>
        /// The name of the table.
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// The default value is 'StorageQueues'.
        /// </summary>
        internal string RootSectionName { get; set; } = AzureStorageConstants.StorageTables;
    }
}
