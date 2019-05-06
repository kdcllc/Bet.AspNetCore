using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage.Options;

using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.AzureStorage
{
    /// <inheritdoc />
    public class StorageQueue<TOptions> : IStorageQueue<TOptions> where TOptions : StorageQueueOptions
    {
        private readonly StorageQueue _queue;

        public StorageQueue(
            IOptionsMonitor<TOptions> storageQueueOptions,
            IOptionsMonitor<StorageAccountOptions> storageAccountOptions,
            ILoggerFactory logger = default)
        {
            var options = storageQueueOptions.CurrentValue;

            var accountOptions = storageAccountOptions.Get(options.AzureStorageConfiguration);

            _queue = new StorageQueue(options as StorageQueueOptions, accountOptions, logger.CreateLogger(nameof(StorageQueue<TOptions>)));
        }

        /// <inheritdoc />
        public async Task SendAsync(
            CloudQueueMessage message,
            CancellationToken cancellationToken = default)
        {
            await _queue.SendAsync(message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task SendAsync<T>(
            T message,
            CancellationToken cancellationToken = default)
        {
            await _queue.SendAsync(message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<CloudQueueMessage> GetAsync(
            bool shouldDelete = false,
            TimeSpan? visibilityTimeout = null,
            CancellationToken cancellationToken = default)
        {
            return await _queue.GetAsync(shouldDelete, visibilityTimeout, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(
            bool shouldDelete = true,
            TimeSpan? visibilityTimeout = null,
            CancellationToken cancellationToken = default)
        {
            return await _queue.GetAsync<T>(shouldDelete, visibilityTimeout, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CloudQueueMessage>> GetManyAsync(
            int messageCount = 5,
            bool shouldDelete = false,
            TimeSpan? visibilityTimeout = null,
            CancellationToken cancellationToken = default)
        {
            return await _queue.GetManyAsync(messageCount, shouldDelete, visibilityTimeout, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> GetManyAsync<T>(
            int messageCount = 5,
            bool shouldDelete = true,
            TimeSpan? visibilityTimeout = null,
            CancellationToken cancellationToken = default)
        {
            return await _queue.GetManyAsync<T>(messageCount, shouldDelete, visibilityTimeout, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(CloudQueueMessage message, CancellationToken cancellationToken = default)
        {
            await _queue.DeleteAsync(message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            await _queue.ClearAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CloudQueueMessage>> PeekAsync(
            int count = 1,
            CancellationToken cancellationToken = default)
        {
            return await _queue.PeekAsync(count, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> PeekAsync<T>(
            int count = 1,
            CancellationToken cancellationToken = default)
        {
            return await _queue.PeekAsync<T>(count, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteQueueAsync(CancellationToken cancellationToken = default)
        {
            await _queue.DeleteQueueAsync(cancellationToken);
        }
    }
}
