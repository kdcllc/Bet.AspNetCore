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

        public async Task<IKey?> LoadAsync(string name, CancellationToken cancellationToken)
        {
            var blob = await _storage.GetBytesAsync(_options.NamedOption, name, cancellationToken);
            var text = BitConverter.ToString(blob);

            return KeyFactory.FromPem(text);
        }

        public async Task SaveAsync(IKey value, string name, CancellationToken cancellationToken)
        {
            var text = value.ToPem();
            var bytes = Encoding.UTF8.GetBytes(text);

            await _storage.AddAsync(_options.NamedOption, bytes, name, null, cancellationToken);
        }
    }
}
