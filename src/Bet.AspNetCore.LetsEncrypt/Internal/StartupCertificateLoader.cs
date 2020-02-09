using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.LetsEncrypt.Account;
using Bet.Extensions.LetsEncrypt.Certificates;
using Bet.Extensions.LetsEncrypt.Certificates.Stores;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    internal class StartupCertificateLoader : IHostedService
    {
        private readonly IEnumerable<ICertificateStore> _stores;
        private readonly KestrelCertificateSelector _certificateSelector;
        private readonly IOptions<AcmeAccountOptions> _accountOptions;
        private readonly IOptions<CertificateOptions> _certificateOptions;
        private readonly DevelopmentCertificate _developmentCertificate;
        private readonly HttpChallenge _httpChallenge;

        public StartupCertificateLoader(
            HttpChallenge httpChallenge,
            IOptions<AcmeAccountOptions> accountOptions,
            IOptions<CertificateOptions> certificateOptions,
            DevelopmentCertificate developmentCertificate,
            IEnumerable<ICertificateStore> stores,
            KestrelCertificateSelector certificateSelector)
        {
            _stores = stores ?? throw new ArgumentNullException(nameof(stores));
            _certificateSelector = certificateSelector ?? throw new ArgumentNullException(nameof(certificateSelector));
            _accountOptions = accountOptions;
            _certificateOptions = certificateOptions;
            _developmentCertificate = developmentCertificate ?? throw new ArgumentNullException(nameof(developmentCertificate));
            _httpChallenge = httpChallenge ?? throw new ArgumentNullException(nameof(httpChallenge));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var devCerts = _developmentCertificate.GetCertificates();

            if (devCerts.Any())
            {
                devCerts.ToList().ForEach(c => _certificateSelector.Add(c));
            }

            foreach (var domainName in _accountOptions.Value.Domains)
            {
                foreach (var store in _stores.Where(x => x.Configured))
                {
                    var cert = await store.LoadAsync(
                        domainName,
                        _certificateOptions.Value?.CertificatePassword ?? string.Empty,
                        cancellationToken);

                    if (cert == null)
                    {
                        var certificateBytes = await _httpChallenge.GetCertificateAsync(store.NamedOption, cancellationToken);
                        if (certificateBytes != null)
                        {
                            await store.SaveAsync(certificateBytes, domainName, cancellationToken);

                            cert = new X509Certificate2(certificateBytes, _certificateOptions.Value?.CertificatePassword ?? string.Empty);
                        }
                    }

                    if (cert != null)
                    {
                        _certificateSelector.Add(cert);
                    }
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
