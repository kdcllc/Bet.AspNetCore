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
    public sealed class StorageBlob<TOptions> where TOptions : StorageBlobOptions
    {
        private readonly IOptionsMonitor<TOptions> _storageBlobOptionsMonitor;
        private readonly IOptionsMonitor<StorageAccountOptions> _storageAccountOptionsFactory;

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
            IOptionsMonitor<StorageAccountOptions> storageAccountOptionsFactory,
            ILogger<StorageBlob<TOptions>> logger)
        {
            _storageBlobOptionsMonitor = storageBlobOptionsMonitor;
            _storageAccountOptionsFactory = storageAccountOptionsFactory;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all blobs in the container.
        /// </summary>
        /// <param name="namedContainer">The name of the container options that were registered.</param>
        /// <param name="prefix">The prefix to be used for the search.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<IEnumerable<CloudBlockBlob>> GetAllAsync(
            string namedContainer,
            string prefix = "",
            CancellationToken cancellationToken = default)
        {
            BlobContinuationToken? blobContinuationToken = default;

            var result = new List<CloudBlockBlob>();

            do
            {
                var container = await GetNamedContainer(namedContainer, cancellationToken).Value;
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

        /// <summary>
        /// Gets blob bytes based on the blob name in the container.
        /// </summary>
        /// <param name="namedContainer"></param>
        /// <param name="blobName">The name of the blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<byte[]?> GetBytesAsync(
            string namedContainer,
            string blobName,
            CancellationToken cancellationToken = default)
        {
            var stream = await GetAsync(blobName, namedContainer, cancellationToken);
            if (stream == null)
            {
                return null;
            }

            return await stream.ToByteArrayAsync();
        }

        /// <summary>
        /// Gets a single blob as stream based on blob's name.
        /// </summary>
        /// <param name="namedContainer"></param>
        /// <param name="blobName">The name of the blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when blob name is empty.</exception>
        public async Task<Stream?> GetAsync(
            string namedContainer,
            string blobName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException(nameof(blobName));
            }

            var container = await GetNamedContainer(namedContainer, cancellationToken).Value;

            var blob = container.GetBlockBlobReference(blobName);
            if (blob == null
                || !await blob.ExistsAsync(cancellationToken))
            {
                return null;
            }

            var stream = new MemoryStream();

            await blob.DownloadToStreamAsync(stream, cancellationToken);

            return stream;
        }

        /// <summary>
        /// Gets <see cref="CloudBlockBlob"/>.
        /// </summary>
        /// <param name="namedContainer"></param>
        /// <param name="blobName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<CloudBlockBlob?> GetBlobAsync(
            string namedContainer,
            string blobName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException(nameof(blobName));
            }

            var container = await GetNamedContainer(namedContainer, cancellationToken).Value;

            return container.GetBlockBlobReference(blobName);
        }

        /// <summary>
        /// Get serialized type of the blob based on the name.
        /// </summary>
        /// <typeparam name="T">The type of the serialized object.</typeparam>
        /// <param name="namedContainer"></param>
        /// <param name="blobName">The name of the blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when blob name is empty.</exception>
        public async Task<T> GetAsync<T>(
            string namedContainer,
            string blobName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException(nameof(blobName));
            }

            string data;

            using (var stream = await GetAsync(blobName, namedContainer, cancellationToken))
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

        /// <summary>
        /// Adds byte array content to Azure Blob Container.
        /// </summary>
        /// <param name="namedContainer"></param>
        /// <param name="content">The byte array.</param>
        /// <param name="blobId">The id of the blob.</param>
        /// <param name="contentType">The MIME type of the content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException">If file fails to be added.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="content"/> is <c>null</c>.</exception>
        public async Task<string> AddAsync(
            string namedContainer,
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
                generatedBlobName = await AddAsync(namedContainer, stream, blobId, contentType, cancellationToken);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }

            if (generatedBlobName == null)
            {
                throw new ApplicationException("Failed to Add the file");
            }

            return generatedBlobName;
        }

        /// <summary>
        /// Adds Steam content to Azure Blob Container.
        /// </summary>
        /// <param name="namedContainer"></param>
        /// <param name="content">The byte array.</param>
        /// <param name="blobId">The id of the blob.</param>
        /// <param name="contentType">The MIME type of the content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<string?> AddAsync(
            string namedContainer,
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

            var container = await GetNamedContainer(namedContainer, cancellationToken).Value;

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

        /// <summary>
        /// Adds an object to Azure Blob Container.
        /// </summary>
        /// <param name="namedContainer"></param>
        /// <param name="item">The object to be serialized to Azure Blob Container.</param>
        /// <param name="blobId">The id of the blob.</param>
        /// <param name="encoding">The encoding type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<string> AddAsync(
            string namedContainer,
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

            return await AddAsync(namedContainer, bytes, blobId, contentType, cancellationToken);
        }

        /// <summary>
        /// Adds the object to Azure Blob Container.
        /// </summary>
        /// <param name="namedContainer"></param>
        /// <param name="item">The object to be serialized to Azure Blob Container.</param>
        /// <param name="blobId">The id of the blob.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<string> AddAsync(
            string namedContainer,
            object item,
            string? blobId = null,
            CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return await AddAsync(namedContainer, item, blobId ?? Guid.NewGuid().ToString(), Encoding.UTF8, cancellationToken);
        }

        /// <summary>
        /// Adds data from URI to Azure Blob Container.
        /// </summary>
        /// <param name="namedContainer"></param>
        /// <param name="sourceUri"></param>
        /// <param name="blobId">The id of the blob.</param>
        /// <param name="contentType">The MIME type of the content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<string?> AddAsync(
            string namedContainer,
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

            var container = await GetNamedContainer(namedContainer, cancellationToken).Value;

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

        /// <summary>
        /// Adds array of {T} objects to Azure Blob Container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="namedContainer"></param>
        /// <param name="items">The array of {T} objects.</param>
        /// <param name="encoding">The encoding to be used.</param>
        /// <param name="batchSize">The batch size.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<IList<string>> AddBatchAsync<T>(
            string namedContainer,
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
                        return AddAsync(namedContainer, item, Guid.NewGuid().ToString(), encoding, cancellationToken);
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

        /// <summary>
        /// Adds the array of object to Azure Blob Container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="namedContainer"></param>
        /// <param name="items">The array of objects.</param>
        /// <param name="batchSize">The batch size. The default is 25.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<IList<string>> AddBatchAsync<T>(
            string namedContainer,
            IEnumerable<T> items,
            int batchSize = 25,
            CancellationToken cancellationToken = default)
        {
            return await AddBatchAsync(namedContainer, items, Encoding.UTF8, batchSize, cancellationToken);
        }

        /// <summary>
        /// Deletes the Blob from Azure Blob Container.
        /// </summary>
        /// <param name="namedContainer"></param>
        /// <param name="blobName"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<bool> DeleteAsync(
            string namedContainer,
            string blobName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                throw new ArgumentException(nameof(blobName));
            }

            var container = await GetNamedContainer(namedContainer, cancellationToken).Value;

            var blob = container.GetBlockBlobReference(blobName);
            if (blob != null
                && await blob.ExistsAsync(cancellationToken))
            {
                await blob.DeleteAsync(cancellationToken);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Saves the byte array to the OS file system.
        /// </summary>
        /// <param name="data">The byte array blob.</param>
        /// <param name="pathLocation">The location on OS.</param>
        /// <param name="fileName">The file name.</param>
        /// <param name="mode"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
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

        public Lazy<Task<CloudBlobContainer>> GetNamedContainer(string containerName, CancellationToken cancellationToken = default)
        {
            if (_namedContainers.TryGetValue(containerName, out var container))
            {
                return container;
            }

            var options = _storageBlobOptionsMonitor.Get(containerName);
            var storageOptions = _storageAccountOptionsFactory.Get(options.AccountName);

            var createdContainer = new Lazy<Task<CloudBlobContainer>>(() => CreateCloudBlobContainer(options, storageOptions, cancellationToken));

            _namedContainers.AddOrUpdate(containerName, createdContainer, (_, __) => createdContainer);

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
                _logger.LogInformation("[Azure Blob] Using existing Azure Blob:[{blobName}]; Options:[{optionsName}].", options.ContainerName, options);
            }

            _logger.LogInformation("[Azure Blob][{methodName}] Elapsed: {seconds}sec", nameof(CreateCloudBlobContainer), sw.GetElapsedTime().TotalSeconds);

            return cloudBlobContainer;
        }
    }
}
