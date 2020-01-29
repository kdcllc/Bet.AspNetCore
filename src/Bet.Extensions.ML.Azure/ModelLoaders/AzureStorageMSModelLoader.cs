using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.ML;

namespace Bet.Extensions.ML
{
    public class AzureStorageMSModelLoader : ModelLoader, IDisposable
    {
        private const int TimeoutMilliseconds = 60000;

        private readonly IStorageBlob<StorageBlobOptions> _storageBlob;
        private readonly ILogger<AzureStorageMSModelLoader> _logger;

        private readonly MLContext _mlContext;
        private readonly CancellationTokenSource _stopping;
        private ModelReloadToken _reloadToken;
        private string _modelName = string.Empty;
        private TimeSpan _interval;
        private string _fileName = string.Empty;
        private Task? _pollingTask;
        private ITransformer? _model;
        private string _eTag = string.Empty;

        public AzureStorageMSModelLoader(
            IOptions<MLOptions> contextOptions,
            IStorageBlob<StorageBlobOptions> storageBlob,
            ILogger<AzureStorageMSModelLoader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (contextOptions.Value?.MLContext == null)
            {
                throw new ArgumentNullException(nameof(contextOptions));
            }

            _mlContext = contextOptions.Value.MLContext;
            _reloadToken = new ModelReloadToken();
            _stopping = new CancellationTokenSource();
            _storageBlob = storageBlob ?? throw new ArgumentNullException(nameof(storageBlob));
        }

        public override ITransformer? GetModel()
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
            string modelName,
            string fileName,
            TimeSpan interval)
        {
            _modelName = modelName;
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

                var blob = await _storageBlob.GetBlobAsync(_modelName, _fileName, cancellation.Token);

                var etag = string.Empty;

                if (blob != null)
                {
                    await blob.FetchAttributesAsync();
                    etag = blob.Properties.ETag;
                }

                if (_eTag != etag)
                {
                    var stream = await _storageBlob.GetAsync(_modelName, _fileName, cancellation.Token);

                    if (stream != null)
                    {
                        var previousToken = Interlocked.Exchange(ref _reloadToken, new ModelReloadToken());

                        _model = _mlContext.Model.Load(stream, out _);

                        _logger.LogInformation("[{loader}][Reloaded] {modelName} Elapsed {time}ms", nameof(AzureStorageMSModelLoader), _modelName, sw.GetElapsedTime().TotalMilliseconds);

                        previousToken.OnReload();
                    }
                }
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
    }
}
