using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage.Options;

using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace Bet.Extensions.AzureStorage
{
    public class StorageQueue<TOptions> : IStorageQueue<TOptions>
        where TOptions : StorageQueueOptions
    {
        private readonly IOptionsMonitor<TOptions> _storageQueueOptionsMonitor;
        private readonly IOptionsFactory<StorageAccountOptions> _storageAccountOptionsFactory;
        private readonly ILogger<StorageQueue<TOptions>> _logger;
        private readonly ConcurrentDictionary<string, Lazy<Task<CloudQueue>>> _namedQueues = new ConcurrentDictionary<string, Lazy<Task<CloudQueue>>>();

        public StorageQueue(
            IOptionsMonitor<TOptions> storageQueueOptionsMonitor,
            IOptionsFactory<StorageAccountOptions> storageAccountOptionsFactory,
            ILogger<StorageQueue<TOptions>> logger)
        {
            _storageQueueOptionsMonitor = storageQueueOptionsMonitor ?? throw new ArgumentNullException(nameof(storageQueueOptionsMonitor));
            _storageAccountOptionsFactory = storageAccountOptionsFactory ?? throw new ArgumentNullException(nameof(storageAccountOptionsFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task SendAsync(
            string named,
            CloudQueueMessage message,
            CancellationToken cancellationToken = default)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var queue = await GetNamedQueue(named, cancellationToken).Value;
            await queue.AddMessageAsync(message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task SendAsync(
            CloudQueueMessage message,
            CancellationToken cancellationToken = default)
        {
            await SendAsync(string.Empty, message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task SendAsync<T>(
            string named,
            T message,
            CancellationToken cancellationToken = default)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message is CloudQueueMessage inst)
            {
                await SendAsync(named, inst, cancellationToken);
            }
            else
            {
                await SendAsync(named, new CloudQueueMessage(JsonConvert.SerializeObject(message)), cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task SendAsync<T>(
            T message,
            CancellationToken cancellationToken = default)
        {
            await SendAsync<T>(string.Empty, message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<CloudQueueMessage?> GetAsync(
            string named,
            bool shouldDelete = false,
            TimeSpan? visibilityTimeout = null,
            CancellationToken cancellationToken = default)
        {
            visibilityTimeout ??= _storageQueueOptionsMonitor.Get(named).VisibilityTimeout;

            var queue = await GetNamedQueue(named, cancellationToken).Value;

            var returned = await queue.GetMessageAsync(visibilityTimeout, null, null, cancellationToken);

            if (shouldDelete
                && returned != null)
            {
                await queue.DeleteMessageAsync(returned, cancellationToken);
            }

            return returned;
        }

        /// <inheritdoc />
        public async Task<CloudQueueMessage?> GetAsync(
            bool shouldDelete = false,
            TimeSpan? visibilityTimeout = null,
            CancellationToken cancellationToken = default)
        {
            return await GetAsync(string.Empty, shouldDelete, visibilityTimeout, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(
            string named,
            bool shouldDelete = true,
            TimeSpan? visibilityTimeout = null,
            CancellationToken cancellationToken = default)
        {
            var message = await GetAsync(named, shouldDelete, visibilityTimeout, cancellationToken);

            if (message != null)
            {
                return JsonConvert.DeserializeObject<T>(message.AsString);
            }

            return default!;
        }

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(
            bool shouldDelete = true,
            TimeSpan? visibilityTimeout = null,
            CancellationToken cancellationToken = default)
        {
            return await GetAsync<T>(string.Empty, shouldDelete, visibilityTimeout, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CloudQueueMessage>?> GetManyAsync(
            string named,
            int messageCount = 5,
            bool shouldDelete = false,
            TimeSpan? visibilityTimeout = null,
            CancellationToken cancellationToken = default)
        {
            if (messageCount <= 0)
            {
                messageCount = 1;
            }

            visibilityTimeout ??= _storageQueueOptionsMonitor.Get(named).VisibilityTimeout;

            var queue = await GetNamedQueue(named, cancellationToken).Value;

            var retuned = await queue.GetMessagesAsync(messageCount, visibilityTimeout, null, null, cancellationToken);

            if (shouldDelete
                && retuned != null)
            {
                foreach (var message in retuned)
                {
                    await queue.DeleteMessageAsync(message, cancellationToken);
                }
            }

            return retuned;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CloudQueueMessage>?> GetManyAsync(
            int messageCount = 5,
            bool shouldDelete = false,
            TimeSpan? visibilityTimeout = null,
            CancellationToken cancellationToken = default)
        {
            return await GetManyAsync(string.Empty, messageCount, shouldDelete, visibilityTimeout, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>?> GetManyAsync<T>(
            string named,
            int messageCount = 5,
            bool shouldDelete = true,
            TimeSpan? visibilityTimeout = null,
            CancellationToken cancellationToken = default)
        {
            var returned = await GetManyAsync(named, messageCount, shouldDelete, visibilityTimeout, cancellationToken);

            return returned?.Select(m => JsonConvert.DeserializeObject<T>(m.AsString));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>?> GetManyAsync<T>(
            int messageCount = 5,
            bool shouldDelete = true,
            TimeSpan? visibilityTimeout = null,
            CancellationToken cancellationToken = default)
        {
            return await GetManyAsync<T>(string.Empty, messageCount, shouldDelete, visibilityTimeout, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(
            string named,
            CloudQueueMessage message,
            CancellationToken cancellationToken = default)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var queue = await GetNamedQueue(named, cancellationToken).Value;

            await queue.DeleteMessageAsync(message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(
            CloudQueueMessage message,
            CancellationToken cancellationToken = default)
        {
            await DeleteAsync(string.Empty, message, cancellationToken);
        }

        /// <inheritdoc />
        public async Task ClearAsync(
            string named,
            CancellationToken cancellationToken = default)
        {
            var queue = await GetNamedQueue(named, cancellationToken).Value;

            await queue.ClearAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task ClearAsync(
            CancellationToken cancellationToken = default)
        {
            await ClearAsync(string.Empty, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CloudQueueMessage>> PeekAsync(
            string named,
            int count = 1,
            CancellationToken cancellationToken = default)
        {
            var queue = await GetNamedQueue(named, cancellationToken).Value;

            return await queue.PeekMessagesAsync(count, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CloudQueueMessage>> PeekAsync(
            int count = 1,
            CancellationToken cancellationToken = default)
        {
            return await PeekAsync(string.Empty, count, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>?> PeekAsync<T>(
            string named,
            int count = 1,
            CancellationToken cancellationToken = default)
        {
            var returned = await PeekAsync(named, count, cancellationToken);

            return returned?.Select(m => JsonConvert.DeserializeObject<T>(m.AsString));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>?> PeekAsync<T>(
            int count = 1,
            CancellationToken cancellationToken = default)
        {
            return await PeekAsync<T>(string.Empty, count, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteQueueAsync(string named, CancellationToken cancellationToken = default)
        {
            var queue = await GetNamedQueue(named, cancellationToken).Value;

            await queue.DeleteAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteQueueAsync(CancellationToken cancellationToken = default)
        {
            await DeleteQueueAsync(string.Empty, cancellationToken);
        }

        /// <inheritdoc />
        public Lazy<Task<CloudQueue>> GetNamedQueue(string queueName, CancellationToken cancellationToken = default)
        {
            if (_namedQueues.TryGetValue(queueName, out var container))
            {
                return container;
            }

            var options = _storageQueueOptionsMonitor.Get(queueName);
            var storageOptions = _storageAccountOptionsFactory.Create(options.AccountName);

            var createdQueue = new Lazy<Task<CloudQueue>>(() => CreateCloudQueue(options, storageOptions, cancellationToken));

            _namedQueues.AddOrUpdate(queueName, createdQueue, (_, __) => createdQueue);

            return createdQueue;
        }

        private async Task<CloudQueue> CreateCloudQueue(
            StorageQueueOptions options,
            StorageAccountOptions storageAccountOptions,
            CancellationToken cancellationToken = default)
        {
            if (storageAccountOptions.CloudStorageAccount == null)
            {
                throw new NullReferenceException($"{nameof(storageAccountOptions.CloudStorageAccount)} wasn't created");
            }

            var sw = ValueStopwatch.StartNew();

            var cloudStorageAccount = await storageAccountOptions.CloudStorageAccount.Value;

            var cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();

            var queue = cloudQueueClient.GetQueueReference(options.QueueName);

            var created = await queue.CreateIfNotExistsAsync(cancellationToken);

            if (created)
            {
                _logger.LogInformation("[Azure Queue] No Azure Queue [{queueName}] found - so one was auto created.", options.QueueName);
            }
            else
            {
                _logger.LogInformation("[Azure Queue] Using existing Azure Queue:[{queueName}].", options.QueueName);
            }

            _logger.LogInformation("[Azure Blob][{methodName}] Elapsed: {elapsed}sec", nameof(CreateCloudQueue), sw.GetElapsedTime().TotalSeconds);

            return queue;
        }
    }
}
