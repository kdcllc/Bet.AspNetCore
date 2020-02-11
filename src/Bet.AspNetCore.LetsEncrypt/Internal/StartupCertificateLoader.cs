using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.LetsEncrypt.Account;
using Bet.Extensions.LetsEncrypt.Certificates;
using Bet.Extensions.LetsEncrypt.Certificates.Stores;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    internal class StartupCertificateLoader : IHostedService
    {
        private readonly IEnumerable<ICertificateStore> _stores;
        private readonly KestrelCertificateSelector _certificateSelector;
        private readonly IServer _server;
        private readonly IConfiguration _config;
        private readonly ILogger<StartupCertificateLoader> _logger;
        private readonly AcmeAccountOptions _accountOptions;
        private readonly CertificateOptions _certificateOptions;
        private readonly DevelopmentCertificate _developmentCertificate;
        private readonly HttpChallenge _httpChallenge;
        private readonly ICertificateValidator _certificateValidator;

        public StartupCertificateLoader(string named, IServiceProvider sp)
        {
            _httpChallenge = sp.GetRequiredService<HttpChallenge>();
            _certificateValidator = sp.GetRequiredService<ICertificateValidator>();

            _accountOptions = sp.GetRequiredService<IOptionsMonitor<AcmeAccountOptions>>().Get(named);
            _certificateOptions = sp.GetRequiredService<IOptionsMonitor<CertificateOptions>>().Get(named);

            _developmentCertificate = sp.GetRequiredService<DevelopmentCertificate>();
            _stores = sp.GetRequiredService<IEnumerable<ICertificateStore>>();
            _certificateSelector = sp.GetRequiredService<KestrelCertificateSelector>();

            _server = sp.GetRequiredService<IServer>();
            _config = sp.GetRequiredService<IConfiguration>();

            _logger = sp.GetRequiredService<ILogger<StartupCertificateLoader>>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!(_server is KestrelServer))
            {
                var serverType = _server.GetType().FullName;
                _logger.LogWarning("LetsEncrypt can only be used with Kestrel and is not supported on {serverType} servers. Skipping certificate provisioning.", serverType);
                await Task.CompletedTask;
            }

            if (_config.GetValue<bool>("UseIISIntegration"))
            {
                _logger.LogWarning("LetsEncrypt does not work with apps hosting in IIS. IIS does not allow for dynamic HTTPS certificate binding, " +
                    "so if you want to use Let's Encrypt, you'll need to use a different tool to do so.");
                await Task.CompletedTask;
            }

            var devCerts = _developmentCertificate.GetCertificates();

            if (devCerts.Any())
            {
                devCerts.ToList().ForEach(c => _certificateSelector.Add(c));
            }

            foreach (var domainName in _accountOptions.Domains)
            {
                foreach (var store in _stores.Where(x => x.Configured))
                {
                    var cert = await store.LoadAsync(
                        domainName,
                        _certificateOptions?.CertificatePassword ?? string.Empty,
                        cancellationToken);

                    if (!_certificateValidator.IsCertificateValid(store.NamedOption, cert))
                    {
#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Factory.StartNew(async () =>
                        {
                            const string ErrorMessage = "Failed to create certificate";

                            try
                            {
                                await CreateAcmeOrder(domainName, store, cancellationToken);
                            }
                            catch (AggregateException ex) when (ex.InnerException != null)
                            {
                                _logger.LogError(0, ex.InnerException, ErrorMessage);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(0, ex, ErrorMessage);
                            }
                        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler
                    }
                    else
                    {
                        _certificateSelector.Add(cert!);
                    }
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        private async Task CreateAcmeOrder(string domainName, ICertificateStore store, CancellationToken cancellationToken)
        {
            X509Certificate2? cert;

            var certificateBytes = await _httpChallenge.GetCertificateAsync(store.NamedOption, cancellationToken);
            if (certificateBytes != null)
            {
                await store.SaveAsync(certificateBytes, domainName, cancellationToken);

#pragma warning disable CA2000 // Dispose objects before losing scope
                cert = new X509Certificate2(certificateBytes, _certificateOptions?.CertificatePassword ?? string.Empty);
#pragma warning restore CA2000 // Dispose objects before losing scope

                _certificateSelector.Add(cert);
            }
        }
    }
}
