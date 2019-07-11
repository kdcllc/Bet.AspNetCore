using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Bet.AspNetCore.LetsEncrypt.Abstractions;

namespace Bet.AspNetCore.LetsEncrypt.InMemory
{
    public class InMemoryCertificateStoreProvider : ICertificateStoreProvider
    {
        private readonly ConcurrentDictionary<string, byte[]> _values = new ConcurrentDictionary<string, byte[]>();

        public Task<byte[]> GetAsync(string storeType, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromResult<byte[]>(null);
            }

            if (_values.TryGetValue(storeType, out var value))
            {
                return Task.FromResult(value);
            }

            return Task.FromResult<byte[]>(null);
        }

        public Task SaveAsync(string storeType, byte[] bytes, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.CompletedTask;
            }

            return Task.FromResult(_values.AddOrUpdate(storeType, bytes, (_, __) => bytes));
        }
    }
}
