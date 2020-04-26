using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.LetsEncrypt.Account;
using Bet.Extensions.LetsEncrypt.Certificates;
using Bet.Extensions.LetsEncrypt.Certificates.Stores;

using CronScheduler.Extensions.Scheduler;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    internal class AcmeRenewalJob : IScheduledJob
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<AcmeRenewalJob> _logger;

        private AcmeRenewalJobOptions _options;

        public AcmeRenewalJob(
            IServiceProvider provider,
            IOptionsMonitor<AcmeRenewalJobOptions> options,
            ILogger<AcmeRenewalJob> logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _options = options.Get(Name);

            options.OnChange((o, n) =>
            {
                if (n == Name)
                {
                    _options = o;
                }
            });
        }

        public string Name => nameof(AcmeRenewalJob);

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{name}][started] executing", nameof(AcmeRenewalJob));
            try
            {
                using var scope = _provider.CreateScope();
                var sp = scope.ServiceProvider;

                // options
                var accountOptions = sp.GetRequiredService<IOptionsMonitor<AcmeAccountOptions>>()
                                       .Get(_options.NamedOptions);

                var certificateOptions = sp.GetRequiredService<IOptionsMonitor<CertificateOptions>>()
                                           .Get(_options.NamedOptions);

                var stores = sp.GetRequiredService<IEnumerable<ICertificateStore>>();

                var httpChallenge = sp.GetRequiredService<HttpChallenge>();

                var certificateSelector = sp.GetRequiredService<KestrelCertificateSelector>();

                var certificateValidator = sp.GetRequiredService<ICertificateValidator>();

                var updated = new List<string>();
                var expiredInMemory = certificateSelector.GetCertificatesAboutToExpire();

                foreach (var domainName in accountOptions.Domains)
                {
                    foreach (var store in stores.Where(x => x.Configured))
                    {
                        var cert = await store.LoadAsync(
                            domainName,
                            certificateOptions?.CertificatePassword ?? string.Empty,
                            cancellationToken);

                        if (!certificateValidator.IsCertificateValid(store.NamedOption, cert))
                        {
                            const string ErrorMessage = "Failed to create certificate";

                            try
                            {
                                cert = await CreateAcmeOrder(domainName, httpChallenge, certificateOptions, store, cancellationToken);
                                certificateSelector.Add(cert!);
                                updated.Add(domainName);
                            }
                            catch (AggregateException ex) when (ex.InnerException != null)
                            {
                                _logger.LogError(0, ex.InnerException, ErrorMessage);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(0, ex, ErrorMessage);
                            }
                        }
                    }
                }

                if (expiredInMemory.Length != updated.Count)
                {
                    _logger.LogWarning(1, "Update SSL {updatedCount} and InMemory outdated {expiredInMemoryCount}", updated.Count, expiredInMemory.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, $"{nameof(AcmeRenewalJob)} failed to renew certificate");
            }

            _logger.LogInformation("[{name}][ended] executing", nameof(AcmeRenewalJob));
        }

        private async Task<X509Certificate2?> CreateAcmeOrder(
            string domainName,
            HttpChallenge httpChallenge,
            CertificateOptions certificateOptions,
            ICertificateStore store,
            CancellationToken cancellationToken)
        {
            var certificateBytes = await httpChallenge.GetCertificateAsync(store.NamedOption, cancellationToken);

            if (certificateBytes == null)
            {
                return await Task.FromResult<X509Certificate2?>(null);
            }

            await store.SaveAsync(certificateBytes, domainName, cancellationToken);

#pragma warning disable CA2000 // Dispose objects before losing scope
            return await Task.FromResult(new X509Certificate2(certificateBytes, certificateOptions?.CertificatePassword ?? string.Empty));
#pragma warning restore CA2000 // Dispose objects before losing scope

        }
    }
}
