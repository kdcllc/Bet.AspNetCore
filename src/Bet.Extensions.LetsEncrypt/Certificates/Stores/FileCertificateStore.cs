using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace Bet.Extensions.LetsEncrypt.Certificates.Stores
{
    public class FileCertificateStore : ICertificateStore
    {
        private readonly FileCertificateStoreOptions _options;

        public FileCertificateStore(IOptions<FileCertificateStoreOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public bool Configured => _options.Configured;

        public string NamedOption => _options.NamedOption;

        public async Task<X509Certificate2?> LoadAsync(string name, string certificatePassword, CancellationToken cancellationToken)
        {
            var fullPath = Path.Combine(_options.RootPath, name);
            if (!File.Exists(fullPath))
            {
                return null;
            }

            var certificateBytes = await File.ReadAllBytesAsync(name, cancellationToken);

            return new X509Certificate2(certificateBytes, certificatePassword);
        }

        public async Task SaveAsync(byte[] value, string name, CancellationToken cancellationToken)
        {
            var fullPath = Path.Combine(_options.RootPath, name);
            var directoryPath = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            await File.WriteAllBytesAsync(fullPath, value, cancellationToken);
        }
    }
}
