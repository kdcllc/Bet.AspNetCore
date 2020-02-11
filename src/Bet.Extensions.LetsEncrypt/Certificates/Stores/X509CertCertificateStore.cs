using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.LetsEncrypt.Certificates.Stores
{
    public class X509CertCertificateStore : ICertificateStore, IDisposable
    {
        private readonly X509CertCertificateStoreOptions _options;
        private readonly ILogger<X509CertCertificateStore> _logger;
        private readonly X509Store _store;

        public X509CertCertificateStore(
            IOptions<X509CertCertificateStoreOptions> options,
            ILogger<X509CertCertificateStore> logger)
        {
            _options = options.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            _store.Open(OpenFlags.ReadWrite);
        }

        public bool Configured => _options.Configured;

        public string NamedOption => _options.NamedOption;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Task<X509Certificate2?> LoadAsync(string name, string certificatePassword, CancellationToken cancellationToken)
        {
            var certs = _store.Certificates.Find(
               X509FindType.FindBySubjectDistinguishedName,
               "CN=" + name,
               validOnly: !_options.AllowInvalidCerts);

            if (certs == null || certs.Count == 0)
            {
                return Task.FromResult<X509Certificate2>(null);
            }

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                foreach (var cert in certs)
                {
                    _logger.LogTrace("Found certificate {subject}", cert.SubjectName.Name);
                }
            }

            X509Certificate2? certWithMostTtl = null;
            foreach (var cert in certs)
            {
                if (certWithMostTtl == null || cert.NotAfter > certWithMostTtl.NotAfter)
                {
                    certWithMostTtl = cert;
                }
            }

            return Task.FromResult(certWithMostTtl);
        }

        public Task SaveAsync(byte[] value, string name, CancellationToken cancellationToken)
        {
            try
            {
                using var certificate = new X509Certificate2(value);
                _store.Add(certificate);
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, "Failed to save certificate to store");
                throw;
            }

            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _store?.Dispose();
            }
        }
    }
}
