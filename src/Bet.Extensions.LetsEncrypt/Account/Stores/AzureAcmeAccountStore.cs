using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;

using Certes;

using Microsoft.Extensions.Options;

namespace Bet.Extensions.LetsEncrypt.Account.Stores
{
    public class AzureAcmeAccountStore : IAcmeAccountStore
    {
        private readonly AzureAcmeAccountStoreOptions _options;
        private readonly IStorageBlob<StorageBlobOptions> _storage;

        public AzureAcmeAccountStore(
            IOptions<AzureAcmeAccountStoreOptions> options,
            IStorageBlob<StorageBlobOptions> storage)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public bool Configured => _options.Configured;

        public async Task<IKey?> LoadAsync(string name, CancellationToken cancellationToken)
        {
            var bytes = await _storage.GetBytesAsync($"{_options.NamedOption}-account", name, cancellationToken);

            if (bytes == null)
            {
                return null;
            }

            var text = Encoding.UTF8.GetString(bytes);
            return KeyFactory.FromPem(text);
        }

        public async Task SaveAsync(IKey value, string name, CancellationToken cancellationToken)
        {
            var text = value.ToPem();
            var bytes = Encoding.UTF8.GetBytes(text);

            await _storage.AddAsync($"{_options.NamedOption}-account", bytes, name, null, cancellationToken);
        }
    }
}
