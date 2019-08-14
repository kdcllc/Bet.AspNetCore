using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Bet.AspNetCore.LetsEncrypt.Options;

using Certes;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.LetsEncrypt.Abstractions
{
    /// <inheritdoc/>
    public class CertificateStore : ICertificateStore
    {
        private readonly IEnumerable<ICertificateStoreProvider> _providers;
        private readonly ILogger<CertificateStore> _logger;

        private LetsEncryptOptions _options;

        public CertificateStore(
            IOptionsMonitor<LetsEncryptOptions> options,
            IEnumerable<ICertificateStoreProvider> providers,
            ILogger<CertificateStore> logger)
        {
            _options = options.CurrentValue;
            options.OnChange((newOptions) => _options = newOptions);

            _providers = providers ?? throw new ArgumentNullException(nameof(providers));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IKey> GetAccountAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving Account Key...");

            IKey key = null;

            var bytes = await GetBytesAsync(StoreType.Account.ToString(), _providers, cancellationToken);
            if (bytes != null)
            {
                var text = Encoding.UTF8.GetString(bytes);
                key = KeyFactory.FromPem(text);

                _logger.LogInformation("Account Key was retrieved.");
            }

            return key;
        }

        /// <inheritdoc/>
        public async Task<X509Certificate2> GetCertificateAsync(string hostName, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Retrieving SSL Certificate ...");

            X509Certificate2 certificate = null;
            var bytes = await GetBytesAsync(hostName, _providers, cancellationToken);

            if (bytes != null)
            {
                certificate = new X509Certificate2(bytes, _options.CertificatePassword);
                _logger.LogInformation(
                    "Retrieved Certificate {subjectName} ({thumbprint})",
                    certificate.Subject,
                    certificate.Thumbprint);
            }

            return certificate;
        }

        /// <inheritdoc/>
        public async Task SaveAccountAsync(IKey certificate, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Saving Account Key...");

            var text = certificate.ToPem();
            var bytes = Encoding.UTF8.GetBytes(text);

            await SaveAsync(StoreType.Account.ToString(), bytes, _providers, cancellationToken);

            _logger.LogInformation("Account Key is saved for later use.");
        }

        /// <inheritdoc/>
        public async Task SaveCertificateAsync(string hostName, byte[] certificateBytes, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Saving Certificate...");

            await SaveAsync(hostName, certificateBytes, _providers, cancellationToken);

            _logger.LogInformation("Certificate is saved for later use.");
        }

        private async Task SaveAsync(
            string storeType,
            byte[] bytes,
            IEnumerable<ICertificateStoreProvider> providers,
            CancellationToken cancellationToken)
        {
            _logger.LogTrace("Using Provider {0} to save...", storeType);

            var tasks = providers.Select(x => x.SaveAsync(storeType, bytes ?? Array.Empty<byte>(), cancellationToken));
            await Task.WhenAll(tasks);
        }

        private async Task<byte[]> GetBytesAsync(
            string storeType,
            IEnumerable<ICertificateStoreProvider> providers,
            CancellationToken cancellationToken)
        {
            foreach (var provider in providers)
            {
                var bytes = await provider.GetAsync(storeType, cancellationToken);
                if (bytes?.Length > 0)
                {
                    return bytes;
                }
            }

            return null;
        }
    }
}
