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
    public class AzureStorageCertificateStoreProvider : ICertificateStoreProvider
    {
        private readonly IStorageBlob<StorageBlobOptions> _storage;
        private LetsEncryptOptions _options;

        public AzureStorageCertificateStoreProvider(
            IOptionsMonitor<LetsEncryptOptions> options,
            IStorageBlob<StorageBlobOptions> storage)
        {
            _options = options.CurrentValue;

            options.OnChange(n => _options = n);

            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public Task<byte[]> GetAsync(string storeType, CancellationToken cancellationToken)
        {
            return _storage.GetBytesAsync(_options.CertificateContainerName, storeType, cancellationToken);
        }

        public Task SaveAsync(string storeType, byte[] bytes, CancellationToken cancellationToken)
        {
            return _storage.AddAsync(_options.CertificateContainerName, bytes, storeType, null, cancellationToken);
        }
    }
}
