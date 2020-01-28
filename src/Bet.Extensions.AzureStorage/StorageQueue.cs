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
    public class StorageQueue<TOptions> where TOptions : StorageQueueOptions
    {
        private readonly IOptionsMonitor<TOptions> _storageQueueOptionsMonitor;
        private readonly IOptionsMonitor<StorageAccountOptions> _storageAccountOptionsMonitor;
        private readonly ILogger<StorageQueue<TOptions>> _logger;
        private readonly ConcurrentDictionary<string, Lazy<Task<CloudQueue>>> _namedQueues = new ConcurrentDictionary<string, Lazy<Task<CloudQueue>>>();

        public StorageQueue(
            IOptionsMonitor<TOptions> storageQueueOptionsMonitor,
            IOptionsMonitor<StorageAccountOptions> storageAccountOptionsMonitor,
            ILogger<StorageQueue<TOptions>> logger)
        {
            _storageQueueOptionsMonitor = storageQueueOptionsMonitor ?? throw new ArgumentNullException(nameof(storageQueueOptionsMonitor));
            _storageAccountOptionsMonitor = storageAccountOptionsMonitor ?? throw new ArgumentNullException(nameof(storageAccountOptionsMonitor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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

        public async Task ClearAsync(
            string named,
            CancellationToken cancellationToken = default)
        {
            var queue = await GetNamedQueue(named, cancellationToken).Value;

            await queue.ClearAsync(cancellationToken);
        }

        public async Task<IEnumerable<CloudQueueMessage>> PeekAsync(
            string named,
            int count = 1,
            CancellationToken cancellationToken = default)
        {
            var queue = await GetNamedQueue(named, cancellationToken).Value;

            return await queue.PeekMessagesAsync(count, cancellationToken);
        }

        public async Task<IEnumerable<T>?> PeekAsync<T>(
            string named,
            int count = 1,
            CancellationToken cancellationToken = default)
        {
            var returned = await PeekAsync(named, count, cancellationToken);

            return returned?.Select(m => JsonConvert.DeserializeObject<T>(m.AsString));
        }

        public async Task DeleteQueueAsync(string named, CancellationToken cancellationToken = default)
        {
            var queue = await GetNamedQueue(named, cancellationToken).Value;

            await queue.DeleteAsync(cancellationToken);
        }

        public Lazy<Task<CloudQueue>> GetNamedQueue(string queueName, CancellationToken cancellationToken = default)
        {
            if (_namedQueues.TryGetValue(queueName, out var container))
            {
                return container;
            }

            var options = _storageQueueOptionsMonitor.Get(queueName);
            var storageOptions = _storageAccountOptionsMonitor.Get(options.AccountName);

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
                _logger.LogInformation("[Azure Queue] Using existing Azure Queue:[{queueName}]; Options:[{optionsName}].", options.QueueName, options);
            }

            _logger.LogInformation("[Azure Blob][{methodName}] Elapsed: {seconds}sec", nameof(CreateCloudQueue), sw.GetElapsedTime().TotalSeconds);

            return queue;
        }
    }
}
