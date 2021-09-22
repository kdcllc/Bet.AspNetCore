using System.Collections.Concurrent;
using System.Threading.Tasks;

using Bet.AspNetCore.ApiKeyAuthentication.Options;

using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.ApiKeyAuthentication
{
    public class InMemoryApiUserStore : IApiUserStore
    {
        private readonly ConcurrentDictionary<string, ApiKeyUser> _store = new ConcurrentDictionary<string, ApiKeyUser>();
        private ApiUserStoreOptions _options;

        public InMemoryApiUserStore(IOptionsMonitor<ApiUserStoreOptions> optionsMonitor)
        {
            _options = optionsMonitor.Get(nameof(InMemoryApiUserStore));

            optionsMonitor.OnChange((o, n) =>
            {
                if (n == nameof(InMemoryApiUserStore))
                {
                    _options = o;
                }
            });

            foreach (var user in _options.ApiKeyUsers)
            {
                _store.AddOrUpdate(user.Key, i => user, (i, u) => u);
            }
        }

        public Task<ApiKeyUser> GetAsync(string apiKey)
        {
            if (_store.TryGetValue(apiKey, out var user))
            {
                return Task.FromResult(user);
            }

            return Task.FromResult(default(ApiKeyUser) !);
        }
    }
}
