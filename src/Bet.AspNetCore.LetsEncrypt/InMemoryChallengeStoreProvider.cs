using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Bet.AspNetCore.LetsEncrypt.Abstractions;

namespace Bet.AspNetCore.LetsEncrypt
{
    public class InMemoryChallengeStoreProvider : IChallengeStoreProvider
    {
        private readonly ConcurrentDictionary<string, byte[]> _values = new ConcurrentDictionary<string, byte[]>();

        private const string Tokens = nameof(Tokens);

        /// <inheritdoc/>
        public Task<byte[]> GetAsync(CancellationToken cancellationToken)
        {
            if (_values.TryGetValue(Tokens, out var value))
            {
                return Task.FromResult(_values[Tokens]);
            }

            return Task.FromResult<byte[]>(null);
        }

        /// <inheritdoc/>
        public Task SaveAsync(byte[] bytes, CancellationToken cancellationToken)
        {
            return Task.FromResult(_values.AddOrUpdate(Tokens, bytes, (_, __) => bytes));
        }
    }
}
