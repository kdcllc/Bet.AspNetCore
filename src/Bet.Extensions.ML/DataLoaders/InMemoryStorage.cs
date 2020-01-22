using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Bet.Extensions.ML.DataLoaders
{
    public class InMemoryStorage
    {
        private readonly ConcurrentDictionary<string, Stream> _modelStorage = new ConcurrentDictionary<string, Stream>();

        public Task SaveAsync(string name, Stream stream, CancellationToken cancellationToken)
        {
            return Task.Run(
                () =>
                {
                  _modelStorage.AddOrUpdate(name, stream, (_, __) => stream);
                },
                cancellationToken);
        }

        public Task<Stream> LoadAsync(string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_modelStorage.TryGetValue(name, out var result))
            {
                return Task.FromResult(result);
            }

            return Task.FromResult<Stream>(default!);
        }

        public Task SaveAsync(string json, string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var ms = new MemoryStream())
            {
                var arr = Encoding.UTF8.GetBytes(json);
                ms.Write(arr, 0, arr.Length);
                ms.Position = 0;

                _modelStorage.AddOrUpdate(name, ms, (_, __) => ms);
            }

            return Task.CompletedTask;
        }

        public Task<TResult> LoadAsync<TResult>(string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_modelStorage.TryGetValue(name, out var stream))
            {
                using (var reader = new StreamReader(stream))
                {
                    var obj = JsonConvert.DeserializeObject<TResult>(reader.ReadToEnd());

                    return Task.FromResult(obj);
                }
            }

            return Task.FromResult<TResult>(default!);
        }
    }
}
