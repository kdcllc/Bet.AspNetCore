using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Bet.Extensions.ML.DataLoaders.ModelLoaders
{
    public class InMemoryModelLoader : ModelLoader
    {
        private readonly InMemoryStorage _storage;

        public InMemoryModelLoader(InMemoryStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));

            SaveModelActionAsync = async (fileName, stream, cancellationToken) =>
            {
                await _storage.SaveAsync(fileName, stream, cancellationToken);

                await Task.CompletedTask;
            };

            SaveModelResultActionAsync = async (fileName, json, cancellationToken) =>
            {
                await _storage.SaveAsync(json, fileName, cancellationToken);
                await Task.CompletedTask;
            };
        }

        protected override Func<string, Stream, CancellationToken, Task> SaveModelActionAsync { get; set; }

        protected override Func<string, string, CancellationToken, Task> SaveModelResultActionAsync { get; set; }
    }
}
