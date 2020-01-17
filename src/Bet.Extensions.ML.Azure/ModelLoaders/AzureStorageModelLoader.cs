using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage.Options;

using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.ML;

namespace Bet.Extensions.ML
{
    public class AzureStorageModelLoader : ModelLoader, IDisposable
    {
        private const int TimeoutMilliseconds = 60000;

        private readonly ILogger<AzureStorageModelLoader> _logger;
        private readonly MLContext _context;
        private readonly CancellationTokenSource _stopping;
        private Lazy<Task<CloudBlobContainer>> _container;
        private ModelReloadToken _reloadToken;
        private TimeSpan _interval;
        private string _fileName = string.Empty;
        private Task? _pollingTask;
        private ITransformer? _model;
        private string _eTag = string.Empty;

        public AzureStorageModelLoader(
            IOptions<MLOptions> contextOptions,
            ILogger<AzureStorageModelLoader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (contextOptions.Value?.MLContext == null)
            {
                throw new ArgumentNullException(nameof(contextOptions));
            }

            _context = contextOptions.Value.MLContext;
            _reloadToken = new ModelReloadToken();
            _stopping = new CancellationTokenSource();
        }

        public override ITransformer GetModel()
        {
            if (_pollingTask == null)
            {
                throw new InvalidOperationException("Start must be called on a ModelLoader before it can be used.");
            }

            return _model;
        }

        public override IChangeToken GetReloadToken()
        {
            if (_pollingTask == null)
            {
                throw new InvalidOperationException("Start must be called on a ModelLoader before it can be used.");
            }

            return _reloadToken;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Start(
            StorageAccountOptions storageAccountOptions,
            string containerName,
            string fileName,
            TimeSpan interval)
        {
            _container = new Lazy<Task<CloudBlobContainer>>(() => CreateCloudBlobContainer(storageAccountOptions, containerName, _stopping.Token));
            _interval = interval;

            _fileName = fileName;

            // run for the first time
            RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        internal async Task RunAsync()
        {
            var sw = ValueStopwatch.StartNew();
            CancellationTokenSource? cancellation = null;

            try
            {
                cancellation = CancellationTokenSource.CreateLinkedTokenSource(_stopping.Token);
                cancellation.CancelAfter(TimeoutMilliseconds);

                var blob = (await _container.Value).GetBlockBlobReference(_fileName);
                var etag = blob.Properties.ETag;

                if (_eTag != etag)
                {
                    await LoadModelAsync(_fileName, cancellation.Token);
                    var previousToken = Interlocked.Exchange(ref _reloadToken, new ModelReloadToken());
                    previousToken.OnReload();
                }

                _logger.LogInformation(
                    "[{loader}][Succeeded]-Time Elapsed {time}ms",
                    nameof(AzureStorageModelLoader),
                    sw.GetElapsedTime().TotalMilliseconds);
            }
            catch (OperationCanceledException) when (!_stopping.IsCancellationRequested)
            {
                // This is a cancellation - if the app is shutting down we want to ignore it.
            }
            catch (Exception ex)
            {
                _logger.LogError("Azure Storage Model Loader failed", ex);
            }
            finally
            {
                cancellation?.Dispose();
            }

            // schedule a polling task only if none exists and a valid delay is specified
            if (_pollingTask == null)
            {
                _pollingTask = PollForChangesAsync();
            }
        }

        internal async Task LoadModelAsync(string blobName, CancellationToken cancellationToken = default)
        {
            try
            {
                var blob = (await _container.Value).GetBlockBlobReference(blobName);
                if (blob == null
                    || !await blob.ExistsAsync(cancellationToken))
                {
                    await Task.CompletedTask;
                }

                using var stream = new MemoryStream();

                await blob.DownloadToStreamAsync(stream, cancellationToken);

                _model = _context.Model.Load(stream, out _);
            }
            catch (Exception ex)
            {
                _logger.LogError("Loading failed", ex);
                throw;
            }
        }

        internal async Task WaitForRetry()
        {
            await Task.Delay(_interval, _stopping.Token);
        }

        internal async Task PollForChangesAsync()
        {
            while (!_stopping.IsCancellationRequested)
            {
                await WaitForRetry();
                try
                {
                    await RunAsync();
                }
                catch
                {
                    // Ignore
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stopping?.Dispose();
            }
        }

        private async Task<CloudBlobContainer> CreateCloudBlobContainer(
            StorageAccountOptions storageAccountOptions,
            string containerName,
            CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();

            var cloudStorageAccount = await storageAccountOptions.CloudStorageAccount.Value;
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            var cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);

            var created = await cloudBlobContainer.CreateIfNotExistsAsync(cancellationToken);
            if (created)
            {
                _logger?.LogInformation("  - No Azure Blob [{blobName}] found - so one was auto created.", containerName);
            }
            else
            {
                _logger?.LogInformation("  - Using existing Azure Blob [{blobName}] [{optionsName}].", containerName);
            }

            await cloudBlobContainer.SetPermissionsAsync(
                new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Off
                },
                cancellationToken);

            sw.Stop();

            _logger?.LogInformation("  - {nameOf} ran for {seconds}", nameof(CreateCloudBlobContainer), sw.Elapsed.TotalSeconds);
            return cloudBlobContainer;
        }
    }
}
