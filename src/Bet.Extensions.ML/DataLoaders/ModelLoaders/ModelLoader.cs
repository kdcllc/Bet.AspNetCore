using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;

namespace Bet.Extensions.ML.DataLoaders.ModelLoaders
{
    public abstract class ModelLoader
    {
        private ReloadToken _reloadToken = new ReloadToken();

        protected virtual ModelLoderFileOptions Options { get; set; } = default!;

        protected Func<string, Stream, CancellationToken, Task>? SaveFunc { get; set; }

        protected Func<string, CancellationToken, Task<Stream>>? LoadFunc { get; set; }

        protected Func<string, string, CancellationToken, Task>? SaveResultFunc { get; set; }

        public virtual async Task SaveAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (SaveFunc == null)
            {
                throw new ArgumentNullException($"{nameof(SaveFunc)} is not configured in the provider.");
            }

            var previousToken = Interlocked.Exchange(ref _reloadToken, new ReloadToken());

            await SaveFunc(Options.ModelFileName, stream, cancellationToken);

            previousToken.OnReload();
        }

        public virtual Task<Stream> LoadModelAsync(CancellationToken cancellationToken)
        {
            if (LoadFunc == null)
            {
                throw new ArgumentNullException($"{nameof(LoadFunc)} is not configured in the provider.");
            }

            return LoadFunc(Options.ModelFileName, cancellationToken);
        }

        public virtual Task<TResult> LoadeResultAsync<TResult>(CancellationToken cancellationToken)
        {
            if (LoadFunc == null)
            {
                throw new ArgumentNullException($"{nameof(LoadFunc)} is not configured in the provider.");
            }

            return Task.Run(
                async () =>
                {
                    var ms = await LoadFunc(Options.ModelResultFileName, cancellationToken);
                    using (var reader = new StreamReader(ms))
                    {
                        var obj = JsonConvert.DeserializeObject<TResult>(reader.ReadToEnd());

                        return await Task.FromResult(obj);
                    }
                },
                cancellationToken);
        }

        public virtual async Task SaveResultAsync<TResult>(TResult result, CancellationToken cancellationToken)
        {
            if (SaveResultFunc == null)
            {
                throw new ArgumentNullException($"{nameof(SaveResultFunc)} is not configured in the provider.");
            }

            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
            await SaveResultFunc(Options.ModelResultFileName, json, cancellationToken);
        }

        public virtual IChangeToken GetReloadToken()
        {
            return _reloadToken;
        }

        public virtual void Setup(ModelLoderFileOptions options)
        {
            Options = options;
        }
    }
}
