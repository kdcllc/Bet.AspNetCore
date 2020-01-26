using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.Azure;

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
        private ModelReloadToken _reloadToken;
        private AzureBlobContainerLoader? _containerLoader;
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
            AzureBlobContainerLoader containerLoader,
            string fileName,
            TimeSpan interval)
        {
            _containerLoader = containerLoader;
            _interval = interval;
            _fileName = fileName;

            // run for the first time
            RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        internal async Task RunAsync()
        {
            if (_containerLoader == null)
            {
                throw new ArgumentNullException($"{nameof(_containerLoader)} can't be null");
            }

            var sw = ValueStopwatch.StartNew();
            CancellationTokenSource? cancellation = null;

            try
            {
                cancellation = CancellationTokenSource.CreateLinkedTokenSource(_stopping.Token);
                cancellation.CancelAfter(TimeoutMilliseconds);

                var etag = await _containerLoader.GetETag(_fileName);

                if (_eTag != etag)
                {
                    await _containerLoader.LoadModelAsync(_fileName, cancellation.Token);
                    var previousToken = Interlocked.Exchange(ref _reloadToken, new ModelReloadToken());
                    previousToken.OnReload();
                }

                _logger.LogInformation(
                    "[{loader}][Succeeded] Elapsed {time}ms",
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
