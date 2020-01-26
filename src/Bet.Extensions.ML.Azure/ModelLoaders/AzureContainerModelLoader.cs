using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.DataLoaders.ModelLoaders;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Bet.Extensions.ML.Azure.ModelLoaders
{
    public class AzureContainerModelLoader : ModelLoader, IDisposable
    {
        private readonly ILogger<AzureContainerModelLoader> _logger;
        private bool _disposed;

        public AzureContainerModelLoader(ILogger<AzureContainerModelLoader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public override Task<Stream> LoadAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override Task SaveAsync(Stream stream, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
