using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage.Options;

using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using Microsoft.ML;

namespace Bet.Extensions.ML.Azure
{
    internal class AzureBlobContainerLoader
    {
        private readonly ILogger<AzureBlobContainerLoader> _logger;
        private readonly MLContext _mlContext;
        private readonly StorageAccountOptions _storageAccountOptions;
        private readonly Lazy<Task<CloudBlobContainer>> _container;

        public AzureBlobContainerLoader(
            string containerName,
            MLOptions mLOptions,
            StorageAccountOptions storageAccountOptions,
            ILogger<AzureBlobContainerLoader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mlContext = mLOptions?.MLContext ?? throw new ArgumentNullException(nameof(mLOptions));
            _storageAccountOptions = storageAccountOptions ?? throw new ArgumentNullException(nameof(storageAccountOptions));

            _container = new Lazy<Task<CloudBlobContainer>>(() => CreateCloudBlobContainer(storageAccountOptions, containerName, CancellationToken.None));
        }

        public async Task<string> GetETag(string fileName)
        {
            var blob = (await _container.Value).GetBlockBlobReference(fileName);
            return blob.Properties.ETag;
        }

        public async Task<ITransformer> LoadModelAsync(string blobName, CancellationToken cancellationToken = default)
        {
            if (_container == null)
            {
                throw new ArgumentNullException($"{nameof(_container)} can't be null");
            }

            try
            {
                var blob = (await _container.Value).GetBlockBlobReference(blobName);
                if (blob == null
                    || !await blob.ExistsAsync(cancellationToken))
                {
                    await Task.CompletedTask;
                }

                using var stream = new MemoryStream();

                await blob!.DownloadToStreamAsync(stream, cancellationToken);

                return _mlContext.Model.Load(stream, out _);
            }
            catch (Exception ex)
            {
                _logger.LogError("Loading failed", ex);
                throw;
            }
        }

        private async Task<CloudBlobContainer> CreateCloudBlobContainer(
          StorageAccountOptions storageAccountOptions,
          string containerName,
          CancellationToken cancellationToken = default)
        {
            var sw = ValueStopwatch.StartNew();

            var cloudStorageAccount = await storageAccountOptions.CloudStorageAccount.Value;
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            var cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);

            var created = await cloudBlobContainer.CreateIfNotExistsAsync(cancellationToken);
            if (created)
            {
                _logger?.LogInformation("[{providerName}][Created] No Azure Blob [{blobName}] found - so one was auto created.", nameof(AzureBlobContainerLoader), containerName);
            }
            else
            {
                _logger?.LogInformation("[{providerName}][Found] Using existing Azure Blob [{blobName}].", nameof(AzureBlobContainerLoader), containerName);
            }

            await cloudBlobContainer.SetPermissionsAsync(
                new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Off
                },
                cancellationToken);

            _logger?.LogInformation("[{providerName}][{methodName}]  Eclipsed {seconds}sec", nameof(AzureBlobContainerLoader), nameof(CreateCloudBlobContainer), sw.GetElapsedTime().TotalSeconds);

            return cloudBlobContainer;
        }
    }
}
