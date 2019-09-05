namespace Bet.Extensions.AzureStorage.Options
{
    public class StorageTableOptions
    {
        /// <summary>
        /// The name of the table.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// The default value is 'StorageQueues'.
        /// </summary>
        internal string RootSectionName { get; set; } = Constants.StorageTables;

        /// <summary>
        /// Enables connection to AzureStorage configuration.
        /// </summary>
        internal string AzureStorageConfiguration { get; set; }
    }
}
