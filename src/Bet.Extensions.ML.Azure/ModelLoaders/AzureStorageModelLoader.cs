using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;
using Bet.Extensions.ML.DataLoaders.ModelLoaders;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Bet.Extensions.ML.Azure.ModelLoaders
{
    public class AzureStorageModelLoader : ModelLoader, IDisposable
    {
        private const int TimeoutMilliseconds = 60000;

        private readonly IStorageBlob<StorageBlobOptions> _storageBlob;
        private readonly ILogger<AzureStorageModelLoader> _logger;
        private ReloadToken? _reloadToken;
        private CancellationTokenSource _stopping;
        private string _eTag = string.Empty;
        private Task? _pollingTask;

        public AzureStorageModelLoader(
            IStorageBlob<StorageBlobOptions> storageBlob,
            ILogger<AzureStorageModelLoader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _storageBlob = storageBlob ?? throw new ArgumentNullException(nameof(storageBlob));

            LoadFunc = (options, cancellationToken) => LoadModelResult(options, cancellationToken);

            _reloadToken = new ReloadToken();
            _stopping = new CancellationTokenSource();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override IChangeToken GetReloadToken()
        {
            if (_reloadToken == null)
            {
                throw new InvalidOperationException($"{nameof(AzureStorageModelLoader)} failed to call {nameof(Setup)} method.");
            }

            return _reloadToken;
        }

        public override async Task<Stream> LoadAsync(CancellationToken cancellationToken)
        {
            var result = await _storageBlob.GetAsync(Options.ModelName, Options.ModelFileName, cancellationToken);

            if (result == null)
            {
                throw new ApplicationException("No Model was retrieved");
            }

            return result;
        }

        public override async Task SaveAsync(Stream stream, CancellationToken cancellationToken)
        {
            await _storageBlob.AddAsync(
                Options.ModelName,
                stream,
                Options.ModelFileName,
                contentType: "application/zip",
                cancellationToken: cancellationToken);
        }

        public override async Task SaveResultAsync<TResult>(TResult result, CancellationToken cancellationToken)
        {
            await _storageBlob.AddAsync(Options.ModelName, result!, Options.ModelResultFileName, cancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stopping?.Dispose();
            }
        }

        protected override void Polling()
        {
            if (Options.WatchForChanges
                && Options.ReloadInterval.HasValue)
            {
                RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                _logger.LogInformation(
                    "[{provideName}][Watching] {modelName}-{fileName} with reloading interval {interval}",
                    nameof(AzureStorageModelLoader),
                    Options.ModelName,
                    Options.ModelFileName,
                    Options.ReloadInterval);
            }
        }

        private async Task RunAsync()
        {
            var sw = ValueStopwatch.StartNew();
            CancellationTokenSource? cancellation = null;

            try
            {
                cancellation = CancellationTokenSource.CreateLinkedTokenSource(_stopping.Token);
                cancellation.CancelAfter(TimeoutMilliseconds);

                var blob = await _storageBlob.GetBlobAsync(Options.ModelName, Options.ModelFileName, cancellation.Token);

                var etag = string.Empty;

                if (blob != null)
                {
                    await blob.FetchAttributesAsync();
                    etag = blob.Properties.ETag;
                }

                if (_eTag != etag)
                {
                    var previousToken = Interlocked.Exchange(ref _reloadToken, new ReloadToken());

                    await Task.Delay(100, cancellation.Token);

                    _logger.LogInformation(
                        "[{loader}][Reloaded] Model Name: {modelName} Elapsed: {elapsed}ms",
                        nameof(AzureStorageModelLoader),
                        Options.ModelName,
                        sw.GetElapsedTime().TotalMilliseconds);

                    _eTag = etag;
                    previousToken?.OnReload();
                }
            }
            catch (OperationCanceledException) when (!_stopping.IsCancellationRequested)
            {
                // This is a cancellation - if the app is shutting down we want to ignore it.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure Storage Model Loader failed for Model: {modelName}", Options.ModelName);
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

        private async Task WaitForRetry()
        {
            if (Options.ReloadInterval.HasValue)
            {
                await Task.Delay(Options.ReloadInterval.Value, _stopping.Token);
            }
        }

        private async Task PollForChangesAsync()
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

        private async Task<Stream> LoadModelResult(ModelLoderFileOptions options, CancellationToken cancellationToken)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _storageBlob.GetAsync(options.ModelName, options.ModelResultFileName, cancellationToken);
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}
