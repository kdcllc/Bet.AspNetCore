using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage.Options;

using Microsoft.Azure.Storage.Queue;

namespace Bet.Extensions.AzureStorage
{
    public interface IStorageQueue<TOptions> where TOptions : StorageQueueOptions
    {
        Task ClearAsync(CancellationToken cancellationToken = default);

        Task DeleteAsync(CloudQueueMessage message, CancellationToken cancellationToken = default);

        Task DeleteQueueAsync(CancellationToken cancellationToken = default);

        Task<CloudQueueMessage> GetAsync(bool shouldDelete = false, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);

        Task<T> GetAsync<T>(bool shouldDelete = true, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<CloudQueueMessage>> GetManyAsync(int messageCount = 5, bool shouldDelete = false, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> GetManyAsync<T>(int messageCount = 5, bool shouldDelete = true, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<CloudQueueMessage>> PeekAsync(int count = 1, CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> PeekAsync<T>(int count = 1, CancellationToken cancellationToken = default);

        Task SendAsync(CloudQueueMessage message, CancellationToken cancellationToken = default);

        Task SendAsync<T>(T message, CancellationToken cancellationToken = default);
    }
}
