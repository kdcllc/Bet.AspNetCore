using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace Bet.Extensions.LetsEncrypt.Order.Stores
{
    public class InMemoryChallengeStore : IAcmeChallengeStore
    {
        private readonly ConcurrentDictionary<string, string> _store = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private ChallengeStoreOptions _options;

        public InMemoryChallengeStore(IOptions<ChallengeStoreOptions> options)
        {
            _options = options.Value;
        }

        public bool Configured => _options.Configured;

        public Task DeleteAsync(string name, CancellationToken cancellationToken)
        {
            return Task.Run(() => _store.TryRemove(name, out var value));
        }

        public Task<T> LoadAsync<T>(string name, CancellationToken cancellationToken) where T : class
        {
            if (_store.TryGetValue(name, out var json))
            {
                return Task.Run(() => JsonConvert.DeserializeObject<T>(json), cancellationToken);
            }

            return Task.FromResult<T>(null!);
        }

        public Task SaveAsync<T>(T value, string name, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(value, Formatting.Indented);

            return Task.Run(() => _store.AddOrUpdate(name, json, (_, __) => json), cancellationToken);
        }
    }
}
