using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage.Options;

using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Bet.Extensions.AzureStorage
{
    public class StorageQueue
    {
        private readonly StorageQueueOptions _storageQueueOptions;
        private readonly ILogger _logger;
        private readonly Lazy<Task<CloudQueue>> _queue;

        public StorageQueue(
            StorageQueueOptions storageQueueOptions,
            StorageAccountOptions storageAccountOptions,
            ILogger logger = default)
        {
            if (storageAccountOptions == null)
            {
                throw new ArgumentNullException(nameof(storageAccountOptions));
            }

            _storageQueueOptions = storageQueueOptions ?? throw new ArgumentNullException(nameof(storageQueueOptions));

            _logger = logger;

            _queue = new Lazy<Task<CloudQueue>>(() => CreateCloudQueue(_storageQueueOptions, storageAccountOptions));
        }

        public Task<CloudQueue> Queue => _queue.Value;

        public async Task SendAsync(
            CloudQueueMessage message,
            CancellationToken cancellationToken = default)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await (await Queue).AddMessageAsync(message, cancellationToken);
        }

        public async Task SendAsync<T>(
            T message,
            CancellationToken cancellationToken = default)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message is CloudQueueMessage)
            {
                await SendAsync(message as CloudQueueMessage, cancellationToken);
            }
            else
            {
                await SendAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)), cancellationToken);
            }
        }

        public async Task<CloudQueueMessage> GetAsync(
            bool shouldDelete = false,
            TimeSpan? visibilityTimeout = null,
            CancellationToken cancellationToken = default)
        {
            visibilityTimeout = visibilityTimeout ?? _storageQueueOptions.VisibilityTimeout;

            var returned = await (await Queue).GetMessageAsync(visibilityTimeout, null, null, cancellationToken);

            if (shouldDelete
                && returned != null)
            {
                await (await Queue).DeleteMessageAsync(returned, cancellationToken);
            }

            return returned;
        }

        public async Task<T> GetAsync<T>(
            bool shouldDelete = true,
            TimeSpan? visibilityTimeout = null,
            CancellationToken cancellationToken = default)
        {
            var message = await GetAsync(shouldDelete, visibilityTimeout, cancellationToken);

            if (message != null)
            {
                return JsonConvert.DeserializeObject<T>(message.AsString);
            }

            return default;
        }

        public async Task<IEnumerable<CloudQueueMessage>> GetManyAsync(
            int messageCount = 5,
            bool shouldDelete = false,
            TimeSpan? visibilityTimeout = null,
            CancellationToken cancellationToken = default)
        {
            if (messageCount <= 0)
            {
                messageCount = 1;
            }

            visibilityTimeout = visibilityTimeout ?? _storageQueueOptions.VisibilityTimeout;

            var retuned = await (await Queue).GetMessagesAsync(messageCount, visibilityTimeout, null, null, cancellationToken);

            if (shouldDelete
                && retuned != null)
            {
                foreach (var message in retuned)
                {
                    await (await Queue).DeleteMessageAsync(message, cancellationToken);
                }
            }

            return retuned;
        }

        public async Task<IEnumerable<T>> GetManyAsync<T>(
            int messageCount = 5,
            bool shouldDelete = true,
            TimeSpan? visibilityTimeout = null,
            CancellationToken cancellationToken = default)
        {
            var returned = await GetManyAsync(messageCount, shouldDelete, visibilityTimeout, cancellationToken);

            return returned?.Select(m => JsonConvert.DeserializeObject<T>(m.AsString));
        }

        public async Task DeleteAsync(CloudQueueMessage message, CancellationToken cancellationToken = default)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            await (await Queue).DeleteMessageAsync(message, cancellationToken);
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            await (await Queue).ClearAsync(cancellationToken);
        }

        public async Task<IEnumerable<CloudQueueMessage>> PeekAsync(
            int count = 1,
            CancellationToken cancellationToken = default)
        {
            return await (await Queue).PeekMessagesAsync(count, cancellationToken);
        }

        public async Task<IEnumerable<T>> PeekAsync<T>(
            int count = 1,
            CancellationToken cancellationToken = default)
        {
            var returned = await PeekAsync(count, cancellationToken);

            return returned?.Select(m => JsonConvert.DeserializeObject<T>(m.AsString));
        }

        public async Task DeleteQueueAsync(CancellationToken cancellationToken = default)
        {
            await (await Queue).DeleteAsync(cancellationToken);
        }

        private async Task<CloudQueue> CreateCloudQueue(
            StorageQueueOptions options,
            StorageAccountOptions storageAccountOptions,
            CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();

            var cloudStorageAccount = await storageAccountOptions.CloudStorageAccount.Value;

            var cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();

            var queue = cloudQueueClient.GetQueueReference(options.QueueName);

            var created = await queue.CreateIfNotExistsAsync(cancellationToken);

            if (created)
            {
                _logger?.LogInformation("  - No Azure Queue [{queueName}] found - so one was auto created.", options.QueueName);
            }
            else
            {
                _logger?.LogInformation("  - Using existing Azure Queue [{QueueName}] [{optionsName}].", options.QueueName, options);
            }

            sw.Stop();

            _logger?.LogInformation("  - {nameOf} ran for {seconds}", nameof(CreateCloudQueue), sw.Elapsed.TotalSeconds);

            return queue;
        }
    }
}
