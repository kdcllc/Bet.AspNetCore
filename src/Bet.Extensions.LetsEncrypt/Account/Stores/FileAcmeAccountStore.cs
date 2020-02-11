using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Certes;

using Microsoft.Extensions.Options;

namespace Bet.Extensions.LetsEncrypt.Account.Stores
{
    public class FileAcmeAccountStore : IAcmeAccountStore
    {
        private readonly FileAcmeAccountStoreOptions _options;

        public FileAcmeAccountStore(IOptions<FileAcmeAccountStoreOptions> options)
        {
            _options = options.Value;
        }

        public bool Configured => _options.Configured;

        public async Task<IKey?> LoadAsync(string name, CancellationToken cancellationToken)
        {
            var fullPath = Path.Combine(_options.RootPath, name);

            if (!File.Exists(fullPath))
            {
                return await Task.FromResult<IKey?>(null);
            }

            var text = await File.ReadAllTextAsync(fullPath, cancellationToken);

            return KeyFactory.FromPem(text);
        }

        public async Task SaveAsync(IKey value, string name, CancellationToken cancellationToken)
        {
            var fullPath = Path.Combine(_options.RootPath, name);

            var directoryPath = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var text = value.ToPem();
            var bytes = Encoding.UTF8.GetBytes(text);

            await File.WriteAllBytesAsync(fullPath, bytes);
        }
    }
}
