using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;

using Microsoft.Extensions.Options;

namespace Bet.Extensions.LetsEncrypt.Certificates.Stores
{
    public class AzureCertificateStore : ICertificateStore
    {
        private readonly AzureCertificateStoreOptions _options;
        private readonly IStorageBlob<StorageBlobOptions> _storage;

        public AzureCertificateStore(
            IOptions<AzureCertificateStoreOptions> options,
            IStorageBlob<StorageBlobOptions> storage)
        {
            _options = options.Value;
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public bool Configured => _options.Configured;

        public string NamedOption => _options.NamedOption;

        public async Task<X509Certificate2?> LoadAsync(string name, string certificatePassword, CancellationToken cancellationToken)
        {
            var blob = await _storage.GetBytesAsync($"{_options.NamedOption}-cert", name, cancellationToken);

            if (blob == null)
            {
                return null;
            }

            return new X509Certificate2(blob, certificatePassword);
        }

        public async Task SaveAsync(byte[] value, string name, CancellationToken cancellationToken)
        {
            await _storage.AddAsync($"{_options.NamedOption}-cert", value, name, null, cancellationToken);
        }
    }
}
