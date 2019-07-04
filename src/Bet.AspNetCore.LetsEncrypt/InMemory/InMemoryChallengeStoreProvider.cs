using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Bet.AspNetCore.LetsEncrypt.Abstractions;

namespace Bet.AspNetCore.LetsEncrypt.InMemory
{
    public class InMemoryChallengeStoreProvider : IChallengeStoreProvider
    {
        private readonly ConcurrentDictionary<string, byte[]> _values = new ConcurrentDictionary<string, byte[]>();

        /// <inheritdoc/>
        public Task<byte[]> GetAsync(string responseToken, CancellationToken cancellationToken)
        {
            if (_values.TryGetValue(responseToken, out var value))
            {
                return Task.FromResult(value);
            }

            return Task.FromResult<byte[]>(null);
        }

        /// <inheritdoc/>
        public Task SaveAsync(string responseToken, byte[] bytes, CancellationToken cancellationToken)
        {
            return Task.FromResult(_values.AddOrUpdate(responseToken, bytes, (_, __) => bytes));
        }
    }
}
