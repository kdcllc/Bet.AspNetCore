using System;
using System.Threading;
using System.Threading.Tasks;

using Bet.AspNetCore.LetsEncrypt.Abstractions;
using Bet.AspNetCore.LetsEncrypt.Options;
using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;

using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.LetsEncrypt.Azure
{
    public class AzureStorageChallengeStoreProvider : IChallengeStoreProvider
    {
        private readonly IStorageBlob<StorageBlobOptions> _storage;
        private LetsEncryptOptions _options;

        public AzureStorageChallengeStoreProvider(
            IOptionsMonitor<LetsEncryptOptions> options,
            IStorageBlob<StorageBlobOptions> storage)
        {
            _options = options.CurrentValue;

            options.OnChange(n => _options = n);

            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public Task<byte[]> GetAsync(string responseToken, CancellationToken cancellationToken)
        {
            return _storage.GetBytesAsync(_options.ChallengeConainterName, responseToken, cancellationToken);
        }

        public Task SaveAsync(string responseToken, byte[] bytes, CancellationToken cancellationToken)
        {
            return _storage.AddAsync(_options.ChallengeConainterName, bytes, responseToken, null, cancellationToken);
        }
    }
}
