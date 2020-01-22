using System;
using System.Threading.Tasks;

namespace Bet.Extensions.ML.DataLoaders.ModelLoaders
{
    public class InMemoryModelLoader : ModelLoader
    {
        private readonly InMemoryStorage _storage;

        public InMemoryModelLoader(InMemoryStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));

            SaveFunc = async (fileName, stream, cancellationToken) =>
            {
                await _storage.SaveAsync(fileName, stream, cancellationToken);

                await Task.CompletedTask;
            };

            SaveResultFunc = async (fileName, json, cancellationToken) =>
            {
                await _storage.SaveAsync(json, fileName, cancellationToken);
                await Task.CompletedTask;
            };

            LoadFunc = (fileName, cancellationToken) => storage.LoadAsync(fileName, cancellationToken);
        }
    }
}
