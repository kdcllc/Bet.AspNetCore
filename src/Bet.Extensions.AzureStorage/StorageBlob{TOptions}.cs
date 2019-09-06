using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage.Options;

using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.AzureStorage
{
    /// <inheritdoc />
    public sealed class StorageBlob<TOptions> : IStorageBlob<TOptions> where TOptions : StorageBlobOptions
    {
        private readonly StorageBlob _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageBlob{TOptions}"/> class.
        /// </summary>
        /// <param name="storageAccountOptions">The named <see cref="StorageAccountOptions"/>.</param>
        /// <param name="blobOptions">The named TOptions configurations.</param>
        /// <param name="logger">The logger.</param>
        public StorageBlob(
            IOptionsMonitor<StorageAccountOptions> storageAccountOptions,
            IOptionsMonitor<TOptions> blobOptions,
            ILoggerFactory logger)
        {
            var options = blobOptions.CurrentValue;

            var accountOptions = storageAccountOptions.Get(options.AzureStorageConfiguration);

            Name = options.ContainerName;

            _storage = new StorageBlob(blobOptions.CurrentValue as StorageBlobOptions, accountOptions, logger.CreateLogger(nameof(StorageBlob<TOptions>)));
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public async Task<IEnumerable<CloudBlockBlob>> GetAllAsync(
            string prefix = default,
            CancellationToken cancellationToken = default)
        {
            return await _storage.GetAllAsync(prefix, cancellationToken);
        }

        /// <inheritdoc  />
        public async Task<byte[]> GetBytesAsync(
            string blobName,
            CancellationToken cancellationToken = default)
        {
            return await _storage.GetBytesAsync(blobName, cancellationToken);
        }

        /// <inheritdoc  />
        public async Task<Stream> GetAsync(
            string blobName,
            CancellationToken cancellationToken = default)
        {
            return await _storage.GetAsync(blobName, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(
            string blobName,
            CancellationToken cancellationToken = default)
        {
            return await _storage.GetAsync<T>(blobName, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> AddAsync(
            byte[] content,
            string blobId = default,
            string contentType = default,
            CancellationToken cancellationToken = default)
        {
            return await _storage.AddAsync(content, blobId, contentType, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> AddAsync(
            Stream content,
            string blobId = default,
            string contentType = default,
            CancellationToken cancellationToken = default)
        {
            return await _storage.AddAsync(content, blobId, contentType, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> AddAsync(
            object item,
            string blobId,
            Encoding encoding,
            CancellationToken cancellationToken = default)
        {
            return await _storage.AddAsync(item, blobId, encoding, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> AddAsync(
            object item,
            string blobId = default,
            CancellationToken cancellationToken = default)
        {
            return await _storage.AddAsync(item, blobId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> AddAsync(
            Uri sourceUri,
            string blobId = default,
            string contentType = default,
            CancellationToken cancellationToken = default)
        {
            return await _storage.AddAsync(sourceUri, blobId, contentType, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IList<string>> AddBatchAsync<T>(
            IEnumerable<T> items,
            Encoding encoding,
            int batchSize = 25,
            CancellationToken cancellationToken = default)
        {
            return await _storage.AddBatchAsync<T>(items, encoding, batchSize, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IList<string>> AddBatchAsync<T>(
            IEnumerable<T> items,
            int batchSize = 25,
            CancellationToken cancellationToken = default)
        {
            return await _storage.AddBatchAsync(items, Encoding.UTF8, batchSize, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(
            string blobName,
            CancellationToken cancellationToken = default)
        {
            return await _storage.DeleteAsync(blobName, cancellationToken);
        }

        /// <inheritdoc />
        public async Task SaveAsync(
            byte[] data,
            string pathLocation,
            string fileName,
            FileMode mode = FileMode.Create,
            CancellationToken cancellationToken = default)
        {
            await _storage.SaveAsync(data, pathLocation, fileName, mode, cancellationToken);
        }
    }
}
