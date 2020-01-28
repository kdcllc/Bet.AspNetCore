using System;
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
    public class AzureStorageModelManager : ModelLoader, IDisposable
    {
        private readonly IStorageBlob<StorageBlobOptions> _storageBlob;
        private readonly ILogger<AzureStorageModelManager> _logger;
        private bool _disposed;
        private ReloadToken? _reloadToken;

        public AzureStorageModelManager(
            IStorageBlob<StorageBlobOptions> storageBlob,
            ILogger<AzureStorageModelManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _storageBlob = storageBlob ?? throw new ArgumentNullException(nameof(storageBlob));

            LoadFunc = (options, cancellationToken) => LoadModelResult(options, cancellationToken);
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
                throw new InvalidOperationException($"{nameof(AzureStorageModelManager)} failed to call {nameof(Setup)} method.");
            }

            return _reloadToken;
        }

        public override async Task<Stream> LoadAsync(CancellationToken cancellationToken)
        {
            return await _storageBlob.GetAsync(Options.ModelName, Options.ModelFileName, cancellationToken);
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
            await _storageBlob.AddAsync(Options.ModelName, result, Options.ModelResultFileName, cancellationToken);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    //_watcher?.Dispose();
                }
            }

            _disposed = true;
        }

        protected override void Polling()
        {
            _reloadToken = new ReloadToken();
        }

        private async Task<Stream> LoadModelResult(ModelLoderFileOptions options, CancellationToken cancellationToken)
        {
            return await _storageBlob.GetAsync(options.ModelName, options.ModelResultFileName, cancellationToken);
        }
    }
}
