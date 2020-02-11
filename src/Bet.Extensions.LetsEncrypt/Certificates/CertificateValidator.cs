using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.LetsEncrypt.Certificates
{
    public class CertificateValidator : ICertificateValidator
    {
        private readonly IOptionsMonitor<CertificateOptions> _optionsMonitor;
        private readonly ILogger<CertificateValidator> _logger;

        public CertificateValidator(
            IOptionsMonitor<CertificateOptions> optionsMonitor,
            ILogger<CertificateValidator> logger)
        {
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsCertificateValid(string named, X509Certificate2? certificate)
        {
            try
            {
                if (certificate == null)
                {
                    return false;
                }

                var options = _optionsMonitor.Get(named);

                var now = DateTimeOffset.Now;

                if (options.TimeUntilExpiryBeforeRenewal != null && certificate.NotAfter - now < options.TimeUntilExpiryBeforeRenewal)
                {
                    return false;
                }

                if (options.TimeAfterIssueDateBeforeRenewal != null && now - certificate.NotBefore > options.TimeAfterIssueDateBeforeRenewal)
                {
                    return false;
                }

                if (certificate.NotBefore > now || certificate.NotAfter < now)
                {
                    return false;
                }

                return true;
            }
            catch (CryptographicException exc)
            {
                _logger.LogError(exc, "Exception occurred during certificate validation");
                return false;
            }
        }
    }
}
