using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage.Options;

using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace Bet.Extensions.AzureStorage
{
    public sealed class StorageBlob<TOptions> : IStorageBlob<TOptions>
        where TOptions : StorageBlobOptions
    {
        private readonly IOptionsMonitor<TOptions> _storageBlobOptionsMonitor;
        private readonly IOptionsFactory<StorageAccountOptions> _storageAccountOptionsFactory;

        private readonly ILogger<StorageBlob<TOptions>> _logger;
        private readonly ConcurrentDictionary<string, Lazy<Task<CloudBlobContainer>>> _namedContainers = new ConcurrentDictionary<string, Lazy<Task<CloudBlobContainer>>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageBlob{TOptions}"/> class.
        /// </summary>
        /// <param name="storageBlobOptionsMonitor"></param>
        /// <param name="storageAccountOptionsFactory"></param>
        /// <param name="logger"></param>
        public StorageBlob(
            IOptionsMonitor<TOptions> storageBlobOptionsMonitor,
            IOptionsFactory<StorageAccountOptions> storageAccountOptionsFactory,
            ILogger<StorageBlob<TOptions>> logger)
        {
            _storageBlobOptionsMonitor = storageBlobOptionsMonitor;
            _storageAccountOptionsFactory = storageAccountOptionsFactory;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CloudBlockBlob>> GetAllAsync(
            string named,
            string prefix = "",
            CancellationToken cancellationToken = default)
        {
            BlobContinuationToken? blobContinuationToken = default;

            var result = new List<CloudBlockBlob>();

            do
            {
                var container = await GetNamedContainer(named, cancellationToken).Value;
                var segment = await container.ListBlobsSegmentedAsync(prefix ?? string.Empty, blobContinuationToken, cancellationToken);

                blobContinuationToken = segment.ContinuationToken;

                foreach (var item in segment.Results)
                {
                    if (item is CloudBlockBlob && item != null)
                    {
                        result.Add((CloudBlockBlob)item);
                    }
                }
            }
            while (blobContinuationToken != null);

            return result;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CloudBlockBlob>> GetAllAsync(
            string prefix = "",
            CancellationToken cancellationToken = default)
        {
            return await GetAllAsync(string.Empty, prefix, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<byte[]?> GetBytesAsync(
            string named,
            string blobName,
            CancellationToken cancellationToken = default)
        {
            var stream = await GetAsync(named, blobName, cancellationToken);
            if (stream == null)
            {
                return null;
            }

            return await stream.ToByteArrayAsync();
        }

        /// <inheritdoc />
        public async Task<byte[]?> GetBytesAsync(
            string blobName,
            CancellationToken cancellationToken = default)
        {
            return await GetBytesAsync(string.Empty, blobName, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Stream?> GetAsync(
            string named,
            string blobName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException(nameof(blobName));
            }

            var container = await GetNamedContainer(named, cancellationToken).Value;

            var blob = container.GetBlobReference(blobName);
            if (blob == null
                || !await blob.ExistsAsync(cancellationToken))
            {
                return null;
            }

            var stream = new MemoryStream();

            try
            {
                await blob.DownloadToStreamAsync(stream, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        /// <inheritdoc />
        public async Task<Stream?> GetAsync(
            string blobName,
            CancellationToken cancellationToken = default)
        {
            return await GetAsync(string.Empty, blobName, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<CloudBlockBlob?> GetBlobAsync(
            string named,
            string blobName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException(nameof(blobName));
            }

            var container = await GetNamedContainer(named, cancellationToken).Value;

            return container.GetBlockBlobReference(blobName);
        }

        /// <inheritdoc />
        public async Task<CloudBlockBlob?> GetBlobAsync(
            string blobName,
            CancellationToken cancellationToken = default)
        {
            return await GetBlobAsync(string.Empty, blobName, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(
            string named,
            string blobName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException(nameof(blobName));
            }

            string data;

            using (var stream = await GetAsync(blobName, named, cancellationToken))
            {
                if (stream == null)
                {
                    return default!;
                }

                stream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(stream))
                {
                    data = await reader.ReadToEndAsync();
                }
            }

            if (string.IsNullOrWhiteSpace(data))
            {
                return default!;
            }

            if (typeof(T).IsSimpleType())
            {
                // Assumption: Item was stored 'raw' and not serialized as Json.
                //             No need to do anything special, just use the current data.
                return (T)Convert.ChangeType(data, typeof(T));
            }

            // Assumption: Item was probably serialized (because it was not a simple type), so we now deserialize it.
            return JsonConvert.DeserializeObject<T>(data);
        }

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(
            string blobName,
            CancellationToken cancellationToken = default)
        {
            return await GetAsync<T>(string.Empty, blobName, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> AddAsync(
            string named,
            byte[] content,
            string? blobId = default,
            string? contentType = default,
            CancellationToken cancellationToken = default)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            string generatedBlobName;
            using (var stream = new MemoryStream(content)
            {
                Position = 0
            })
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                generatedBlobName = await AddAsync(named, stream, blobId, contentType, cancellationToken);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }

            if (generatedBlobName == null)
            {
                throw new ApplicationException("Failed to Add the file");
            }

            return generatedBlobName;
        }

        // <inheritdoc />
        public async Task<string> AddAsync(
            byte[] content,
            string? blobId = default,
            string? contentType = default,
            CancellationToken cancellationToken = default)
        {
            return await AddAsync(string.Empty, content, blobId, contentType, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string?> AddAsync(
            string named,
            Stream content,
            string? blobId = default,
            string? contentType = default,
            CancellationToken cancellationToken = default)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (string.IsNullOrWhiteSpace(blobId))
            {
                blobId = Guid.NewGuid().ToString();
            }

            var container = await GetNamedContainer(named, cancellationToken).Value;

            var blob = container.GetBlockBlobReference(blobId);
            if (blob == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                blob.Properties.ContentType = contentType;
            }

            await blob.UploadFromStreamAsync(content, cancellationToken);

            return blob.Name;
        }

        /// <inheritdoc />
        public async Task<string?> AddAsync(
            Stream content,
            string? blobId = default,
            string? contentType = default,
            CancellationToken cancellationToken = default)
        {
            return await AddAsync(string.Empty, content, blobId, contentType, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> AddAsync(
            string named,
            object item,
            string blobId,
            Encoding encoding,
            CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            string data;
            string contentType;

            if (item.GetType().IsSimpleType())
            {
                // No need to convert this item to json.
                data = item.ToString();
                contentType = "text/plain";
            }
            else
            {
                data = JsonConvert.SerializeObject(item);
                contentType = "application/json";
            }

            var bytes = encoding.GetBytes(data);

            return await AddAsync(named, bytes, blobId, contentType, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> AddAsync(
            object item,
            string blobId,
            Encoding encoding,
            CancellationToken cancellationToken = default)
        {
            return await AddAsync(string.Empty, item, blobId, encoding, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> AddAsync(
            string named,
            object item,
            string? blobId = null,
            CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return await AddAsync(named, item, blobId ?? Guid.NewGuid().ToString(), Encoding.UTF8, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string> AddAsync(
            object item,
            string? blobId = null,
            CancellationToken cancellationToken = default)
        {
            return await AddAsync(string.Empty, item, blobId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string?> AddAsync(
            string named,
            Uri sourceUri,
            string? blobId = default,
            string? contentType = default,
            CancellationToken cancellationToken = default)
        {
            if (sourceUri == null)
            {
                throw new ArgumentNullException(nameof(sourceUri));
            }

            if (string.IsNullOrWhiteSpace(blobId))
            {
                blobId = Guid.NewGuid()
                             .ToString();
            }

            var container = await GetNamedContainer(named, cancellationToken).Value;

            var blob = container.GetBlockBlobReference(blobId);
            if (blob == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                blob.Properties.ContentType = contentType;
            }

            await blob.StartCopyAsync(sourceUri, cancellationToken);

            var copyInProgress = true;
            while (copyInProgress)
            {
                // wait before we can check the if the copy has completed.
                await Task.Delay(500, cancellationToken);

                await blob.FetchAttributesAsync(cancellationToken);

                copyInProgress = blob.CopyState.Status == CopyStatus.Pending;
            }

            return blob.Name;
        }

        /// <inheritdoc />
        public async Task<string?> AddAsync(
            Uri sourceUri,
            string? blobId = default,
            string? contentType = default,
            CancellationToken cancellationToken = default)
        {
            return await AddAsync(string.Empty, sourceUri, blobId, contentType, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IList<string>> AddBatchAsync<T>(
            string named,
            IEnumerable<T> items,
            Encoding encoding,
            int batchSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize));
            }

            var itemCount = items.Count();
            var finalBatchSize = itemCount > batchSize ? batchSize : itemCount;

            var blobIds = new List<string>();

            foreach (var batch in items.Batch<T>(finalBatchSize))
            {
                var tasks = batch.Select(item =>
                {
                    if (item != null)
                    {
                        return AddAsync(named, item, Guid.NewGuid().ToString(), encoding, cancellationToken);
                    }
                    else
                    {
                        return Task.FromResult(string.Empty);
                    }
                });

                // executing batch
                var results = await Task.WhenAll(tasks);

                blobIds.AddRange(results);
            }

            return blobIds;
        }

        /// <inheritdoc />
        public async Task<IList<string>> AddBatchAsync<T>(
            IEnumerable<T> items,
            Encoding encoding,
            int batchSize = 25,
            CancellationToken cancellationToken = default)
        {
            return await AddBatchAsync<T>(string.Empty, items, encoding, batchSize, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IList<string>> AddBatchAsync<T>(
            string named,
            IEnumerable<T> items,
            int batchSize = 25,
            CancellationToken cancellationToken = default)
        {
            return await AddBatchAsync(named, items, Encoding.UTF8, batchSize, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IList<string>> AddBatchAsync<T>(
            IEnumerable<T> items,
            int batchSize = 25,
            CancellationToken cancellationToken = default)
        {
            return await AddBatchAsync<T>(string.Empty, items, batchSize, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(
            string named,
            string blobName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException(nameof(blobName));
            }

            var container = await GetNamedContainer(named, cancellationToken).Value;

            var blob = container.GetBlockBlobReference(blobName);
            if (blob != null
                && await blob.ExistsAsync(cancellationToken))
            {
                await blob.DeleteAsync(cancellationToken);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(
            string blobName,
            CancellationToken cancellationToken = default)
        {
            return await DeleteAsync(string.Empty, blobName, cancellationToken);
        }

        /// <inheritdoc />
        public async Task SaveAsync(
            byte[] data,
            string pathLocation,
            string fileName,
            FileMode mode = FileMode.Create,
            CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(pathLocation))
            {
                Directory.CreateDirectory(pathLocation);
            }

            var fileLocation = Path.Combine(pathLocation, fileName);

            using var fs = new FileStream(fileLocation, mode, FileAccess.ReadWrite, FileShare.ReadWrite);
            await fs.WriteAsync(data, 0, data.Length, cancellationToken);
        }

        /// <inheritdoc />
        public Lazy<Task<CloudBlobContainer>> GetNamedContainer(string named, CancellationToken cancellationToken = default)
        {
            if (_namedContainers.TryGetValue(named, out var container))
            {
                return container;
            }

            var options = _storageBlobOptionsMonitor.Get(named);

            if (string.IsNullOrEmpty(options.ContainerName))
            {
                throw new ArgumentNullException($"Please make sure that {nameof(options.ContainerName)} is registered");
            }

            var storageOptions = _storageAccountOptionsFactory.Create(options.AccountName);

            var createdContainer = new Lazy<Task<CloudBlobContainer>>(() => CreateCloudBlobContainer(options, storageOptions, cancellationToken));

            _namedContainers.AddOrUpdate(named, createdContainer, (_, __) => createdContainer);

            return createdContainer;
        }

        private async Task<CloudBlobContainer> CreateCloudBlobContainer(
            StorageBlobOptions options,
            StorageAccountOptions storageAccountOptions,
            CancellationToken cancellationToken = default)
        {
            if (storageAccountOptions.CloudStorageAccount == null)
            {
                throw new NullReferenceException($"{nameof(storageAccountOptions.CloudStorageAccount)} wasn't created");
            }

            var sw = ValueStopwatch.StartNew();

            var cloudStorageAccount = await storageAccountOptions.CloudStorageAccount.Value;
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            var cloudBlobContainer = cloudBlobClient.GetContainerReference(options.ContainerName);

            var created = await cloudBlobContainer.CreateIfNotExistsAsync(cancellationToken);
            if (created)
            {
                _logger.LogInformation("[Azure Blob] No Azure Blob [{blobName}] found - so one was auto created.", options.ContainerName);
            }
            else
            {
                _logger.LogInformation("[Azure Blob] Using existing Azure Blob:[{blobName}].", options.ContainerName);
            }

            // this stop working...
            // await cloudBlobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = options.PublicAccessType }, cancellationToken);
            _logger.LogInformation("[Azure Blob][{methodName}] Elapsed: {elapsed}sec", nameof(CreateCloudBlobContainer), sw.GetElapsedTime().TotalSeconds);

            return cloudBlobContainer;
        }
    }
}
