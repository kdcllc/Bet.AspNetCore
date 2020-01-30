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
        protected virtual ModelLoderFileOptions Options { get; set; } = default!;

        protected Func<ModelLoderFileOptions, string, CancellationToken, Task>? SaveResultFunc { get; set; }

        protected Func<ModelLoderFileOptions, CancellationToken, Task<Stream>>? LoadFunc { get; set; }

        public abstract Task SaveAsync(Stream stream, CancellationToken cancellationToken);

        public abstract Task<Stream> LoadAsync(CancellationToken cancellationToken);

        public virtual Task<TResult> LoadeResultAsync<TResult>(CancellationToken cancellationToken)
        {
            if (LoadFunc == null)
            {
                throw new ArgumentNullException($"{nameof(LoadFunc)} is not configured in the provider.");
            }

            return Task.Run(
                async () =>
                {
                    var ms = await LoadFunc(Options, cancellationToken);
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
            await SaveResultFunc(Options, json, cancellationToken);
        }

        public abstract IChangeToken GetReloadToken();

        public virtual void Setup(ModelLoderFileOptions options)
        {
            Options = options;
            Polling();
        }

        protected abstract void Polling();
    }
}
