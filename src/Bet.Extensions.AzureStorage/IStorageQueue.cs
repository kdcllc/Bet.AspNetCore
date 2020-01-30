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
        /// <summary>
        /// Clears the all messages from Azure Storage Queue.
        /// </summary>
        /// <param name="named">The named options for the queue.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task ClearAsync(string named, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears the all messages from Azure Storage Queue.
        /// </summary>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task ClearAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a message as <see cref="CloudQueueMessage"/> from Azure Storage Queue.
        /// </summary>
        /// <param name="named">The named options for the queue.</param>
        /// <param name="message">The <see cref="CloudQueueMessage"/> to be deleted from the queue.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns>task.</returns>
        Task DeleteAsync(string named, CloudQueueMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a message as <see cref="CloudQueueMessage"/> from Azure Storage Queue.
        /// </summary>
        /// <param name="message">The <see cref="CloudQueueMessage"/> to be deleted from the queue.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        Task DeleteAsync(CloudQueueMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the Azure Storage Queue completely.
        /// </summary>
        /// <param name="named">The named options for the queue.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns>task.</returns>
        Task DeleteQueueAsync(string named, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the Azure Storage Queue completely.
        /// </summary>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        Task DeleteQueueAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves first message from Azure Storage Queue.
        /// </summary>
        /// <param name="named">The named options for the queue.</param>
        /// <param name="shouldDelete">Optional. The flag if message must be deleted right way. The default is false.</param>
        /// <param name="visibilityTimeout">Optional. The timespan for hiding the message from being visible in the queue. The default is 30 seconds. </param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task<CloudQueueMessage?> GetAsync(string named, bool shouldDelete = false, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves first message from Azure Storage Queue.
        /// </summary>
        /// <param name="shouldDelete">Optional. The flag if message must be deleted right way. The default is false.</param>
        /// <param name="visibilityTimeout">Optional. The timespan for hiding the message from being visible in the queue. The default is 30 seconds. </param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task<CloudQueueMessage?> GetAsync(bool shouldDelete = false, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves queued message of T type by de-serializing json from the Azure Storage Queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="named">The named options for the queue.</param>
        /// <param name="shouldDelete">Optional. The flag if message must be deleted right way. The default is false.</param>
        /// <param name="visibilityTimeout">Optional. The timespan for hiding the message from being visible in the queue. The default is 30 seconds. </param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string named, bool shouldDelete = true, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves queued message of T type by de-serializing json from the Azure Storage Queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="shouldDelete">Optional. The flag if message must be deleted right way. The default is false.</param>
        /// <param name="visibilityTimeout">Optional. The timespan for hiding the message from being visible in the queue. The default is 30 seconds. </param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(bool shouldDelete = true, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves messages of <see cref="CloudQueueMessage"/> from the Azure Storage Queue.
        /// </summary>
        /// <param name="named">The named options for the queue.</param>
        /// <param name="messageCount"></param>
        /// <param name="shouldDelete">Optional. The flag if message must be deleted right way. The default is false.</param>
        /// <param name="visibilityTimeout">Optional. The timespan for hiding the message from being visible in the queue. The default is 30 seconds. </param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task<IEnumerable<CloudQueueMessage>?> GetManyAsync(string named, int messageCount = 5, bool shouldDelete = false, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves messages of <see cref="CloudQueueMessage"/> from the Azure Storage Queue.
        /// </summary>
        /// <param name="messageCount"></param>
        /// <param name="shouldDelete">Optional. The flag if message must be deleted right way. The default is false.</param>
        /// <param name="visibilityTimeout">Optional. The timespan for hiding the message from being visible in the queue. The default is 30 seconds. </param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task<IEnumerable<CloudQueueMessage>?> GetManyAsync(int messageCount = 5, bool shouldDelete = false, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves queued messages of T type by de-serializing json from the Azure Storage Queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="named">The named options for the queue.</param>
        /// <param name="messageCount">The number of messages to be retrieved.</param>
        /// <param name="shouldDelete">Optional. The flag if message must be deleted right way. The default is false.</param>
        /// <param name="visibilityTimeout">Optional. The timespan for hiding the message from being visible in the queue. The default is 30 seconds. </param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task<IEnumerable<T>?> GetManyAsync<T>(string named, int messageCount = 5, bool shouldDelete = true, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves queued messages of T type by de-serializing json from the Azure Storage Queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageCount">The number of messages to be retrieved.</param>
        /// <param name="shouldDelete">Optional. The flag if message must be deleted right way. The default is false.</param>
        /// <param name="visibilityTimeout">Optional. The timespan for hiding the message from being visible in the queue. The default is 30 seconds. </param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task<IEnumerable<T>?> GetManyAsync<T>(int messageCount = 5, bool shouldDelete = true, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets named instance of the Azure Storage Queue.
        /// </summary>
        /// <param name="queueName">The named for the options.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Lazy<Task<CloudQueue>> GetNamedQueue(string queueName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Peek messages of <see cref="CloudQueueMessage"/> from the Azure Storage Queue.
        /// </summary>
        /// <param name="named">The named options for the queue.</param>
        /// <param name="count">The number of messages to be peeked at.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task<IEnumerable<CloudQueueMessage>> PeekAsync(string named, int count = 1, CancellationToken cancellationToken = default);

        /// <summary>
        /// Peek messages of <see cref="CloudQueueMessage"/> from the Azure Storage Queue.
        /// </summary>
        /// <param name="count">The number of messages to be peeked at.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task<IEnumerable<CloudQueueMessage>> PeekAsync(int count = 1, CancellationToken cancellationToken = default);

        /// <summary>
        /// Peek queued messages of T type by de-serializing json from the Azure Storage Queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="named">The named options for the queue.</param>
        /// <param name="count">The number of messages to be peeked at.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task<IEnumerable<T>?> PeekAsync<T>(string named, int count = 1, CancellationToken cancellationToken = default);

        /// <summary>
        /// Peek queued messages of T type by de-serializing json from the Azure Storage Queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count">The number of messages to be peeked at.</param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task<IEnumerable<T>?> PeekAsync<T>(int count = 1, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send message of type <see cref="CloudQueueMessage"/> to the Azure Storage Queue.
        /// </summary>
        /// <param name="named">The named options for the queue.</param>
        /// <param name="message"></param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task SendAsync(string named, CloudQueueMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send message of type <see cref="CloudQueueMessage"/> to the Azure Storage Queue.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task SendAsync(CloudQueueMessage message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send message of type Type as serialized json to the Azure Storage Queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="named">The named options for the queue.</param>
        /// <param name="message"></param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task SendAsync<T>(string named, T message, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send message of type Type as serialized json to the Azure Storage Queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="cancellationToken">A System.Threading.CancellationToken to observe while waiting for a task to complete.</param>
        /// <returns></returns>
        Task SendAsync<T>(T message, CancellationToken cancellationToken = default);
    }
}
