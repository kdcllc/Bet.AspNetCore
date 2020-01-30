using System;

namespace Bet.Extensions.AzureStorage.Options
{
    public class StorageQueueOptions : StorageOptionsBase
    {
        /// <summary>
        /// Azure Storage Container name.
        /// </summary>
        public string QueueName { get; set; } = string.Empty;

        /// <summary>
        /// The default value is null.
        /// </summary>
        public TimeSpan? VisibilityTimeout { get; set; } = null;
    }
}
