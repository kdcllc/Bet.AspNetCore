using System;

namespace Bet.Extensions.AzureStorage.Options
{
    public class StorageQueueOptions
    {
        /// <summary>
        /// Azure Storage Container name.
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// The default value is null.
        /// </summary>
        public TimeSpan? VisibilityTimeout { get; set; } = null;

        /// <summary>
        /// The default value is 'StorageQueues'.
        /// </summary>
        internal string RootSectionName { get; set; } = Constants.StorageQueues;

        /// <summary>
        /// Enables connection to AzureStorage configuration.
        /// </summary>
        internal string AzureStorageConfiguration { get; set; }
    }
}
