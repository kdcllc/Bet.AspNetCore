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

        protected abstract Func<string, Stream, CancellationToken, Task> SaveModelActionAsync { get; set; }

        protected abstract Func<string, string, CancellationToken, Task> SaveModelResultActionAsync { get; set; }

        public virtual async Task SaveAsync(Stream stream, CancellationToken cancellationToken)
        {
            var previousToken = Interlocked.Exchange(ref _reloadToken, new ReloadToken());

            await SaveModelActionAsync(Options.ModelFileName, stream, cancellationToken);

            previousToken.OnReload();
        }

        public virtual async Task SaveResultAsync<TResult>(TResult result, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);
            await SaveModelResultActionAsync(Options.ModelResultFileName, json, cancellationToken);
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
