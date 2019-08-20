using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;

namespace Bet.Extensions.ML.ModelStorageProviders
{
    public class InMemoryModelStorageProvider : IModelStorageProvider
    {
        private readonly ConcurrentDictionary<string, MemoryStream> _modelStorage = new ConcurrentDictionary<string, MemoryStream>();
        private readonly object _lock = new object();

        private ReloadToken _reloadToken;

        public InMemoryModelStorageProvider()
        {
            _reloadToken = new ReloadToken();
        }

        public IChangeToken GetReloadToken()
        {
            return _reloadToken;
        }

        public Task<MemoryStream> LoadModelAsync(string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_modelStorage.TryGetValue(name, out var result))
            {
                return Task.FromResult(result);
            }

            return Task.FromResult<MemoryStream>(default);
        }

        public Task<TResult> LoadModelResultAsync<TResult>(string name, CancellationToken cancellationToken)
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

            return Task.FromResult<TResult>(default);
        }

        public Task<IEnumerable<TResult>> LoadRawDataAsync<TResult>(string name, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SaveModelAsync(string name, MemoryStream stream, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var previousToken = Interlocked.Exchange(ref _reloadToken, new ReloadToken());

                lock (_lock)
                {
                    _modelStorage.AddOrUpdate(name, stream, (_, __) => stream);
                }

                previousToken.OnReload();
            });
        }

        public Task SaveModelResultAsync<TResult>(TResult result, string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var json = JsonConvert.SerializeObject(result, Formatting.Indented);

            using (var ms = new MemoryStream())
            {
                var arr = Encoding.UTF8.GetBytes(json);
                ms.Write(arr, 0, arr.Length);
                ms.Position = 0;

                _modelStorage.AddOrUpdate(name, ms, (_, __) => ms);
            }

            return Task.CompletedTask;
        }
    }
}
