using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Primitives;

namespace Bet.Extensions.ML.DataLoaders.ModelLoaders
{
    public class InMemoryModelLoader : ModelLoader
    {
        private readonly InMemoryStorage _storage;
        private ReloadToken? _reloadToken;

        public InMemoryModelLoader(InMemoryStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));

            SaveResultFunc = async (options, json, cancellationToken) =>
            {
                await _storage.SaveAsync(json, options.ModelResultFileName, cancellationToken);
                await Task.CompletedTask;
            };

            LoadFunc = (options, cancellationToken) => storage.LoadAsync(options.ModelResultFileName, cancellationToken);
        }

        public override async Task SaveAsync(Stream stream, CancellationToken cancellationToken)
        {
            var previousToken = Interlocked.Exchange(ref _reloadToken, new ReloadToken());

            await _storage.SaveAsync(Options.ModelFileName, stream, cancellationToken);

            previousToken?.OnReload();
        }

        public override async Task<Stream> LoadAsync(CancellationToken cancellationToken)
        {
            return await _storage.LoadAsync(Options.ModelFileName, cancellationToken);
        }

        public override IChangeToken GetReloadToken()
        {
            if (_reloadToken == null)
            {
                throw new InvalidOperationException($"{nameof(InMemoryModelLoader)} failed to call {nameof(Setup)} method.");
            }

            return _reloadToken;
        }

        protected override void Polling()
        {
            _reloadToken = new ReloadToken();
        }
    }
}
