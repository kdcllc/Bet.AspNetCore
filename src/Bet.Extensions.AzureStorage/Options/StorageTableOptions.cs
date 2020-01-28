namespace Bet.Extensions.AzureStorage.Options
{
    public class StorageTableOptions : StorageOptionsBase
    {
        /// <summary>
        /// The name of the table.
        /// </summary>
        public string TableName { get; set; } = string.Empty;
    }
}
