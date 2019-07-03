using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Bet.AspNetCore.LetsEncrypt.Abstractions;
using Bet.AspNetCore.LetsEncrypt.Options;

using Certes;
using Certes.Acme;
using Certes.Acme.Resource;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    internal class LetsEncryptService : ILetsEncryptService
    {
        private IAcmeContext _acmeContext;
        private LetsEncryptOptions _options;
        private readonly ILogger<LetsEncryptService> _logger;
        private readonly IChallengeStore _challengeStore;

        public LetsEncryptService(
            IOptionsMonitor<LetsEncryptOptions> options,
            IChallengeStore challengeStore,
            ILogger<LetsEncryptService> logger)
        {
            _options = options.CurrentValue;

            options.OnChange(newOptions => _options = newOptions);

            _challengeStore = challengeStore ?? throw new ArgumentNullException(nameof(challengeStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task AuthenticateWithExistingAccount(IKey account, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Acme authenticating using existing Let's Encrypt account...");

            _acmeContext = new AcmeContext(_options.AcmeServer, account);

            await _acmeContext.Account();
        }

        public async Task<IKey> AuthenticateWithNewAccount(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Acme authenticating and creating Let's Encrypt account with email {email}...", _options.Email);
            _acmeContext = new AcmeContext(_options.AcmeServer);
            await _acmeContext.NewAccount(_options.Email, true);

            return _acmeContext.AccountKey;
        }

        public async Task<(X509Certificate2 cert, byte[] rawCert)> AcquireNewCertificateForHosts(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Ordering LetsEncrypt certificate for hosts {0}.", new object[] { _options.HostNames });
            var order = await _acmeContext.NewOrder(_options.HostNames);

            await ValidateOrderAsync(order, cancellationToken);

            var certificateBytes = await AcquireCertificateBytesFromOrderAsync(order, cancellationToken);
            if (certificateBytes == null)
            {
                throw new InvalidOperationException("The certificate from the order was null.");
            }

            var certificate = new X509Certificate2(certificateBytes, _options.CertificatePassword);

            return (certificate, certificateBytes);
        }

        private async Task<byte[]> AcquireCertificateBytesFromOrderAsync(IOrderContext order, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Acquiring certificate through signing request.");

            var privateKey = KeyFactory.NewKey(KeyAlgorithm.ES256);

            var certificateChain = await order.Generate(_options.CertificateSigningRequest, privateKey);

            var pfxBuilder = certificateChain.ToPfx(privateKey);

            pfxBuilder.FullChain = true;

            var pfxBytes = pfxBuilder.Build(_options.CertificateFriendlyName, _options.CertificatePassword);

            _logger.LogInformation("Certificate acquired.");

            return pfxBytes;
        }

        private async Task ValidateOrderAsync(IOrderContext order, CancellationToken cancellationToken)
        {
            var allAuthorizations = await order.Authorizations();
            var challengeContexts = await Task.WhenAll(allAuthorizations.Select(x => x.Http()));

            _logger.LogInformation("Validating all pending order authorizations.");

            var acmeChallengeResponses = challengeContexts.Select(x => new AcmeChallengeResponse(x.Token, x.KeyAuthz)).ToArray();
            await _challengeStore.SaveChallengesAsync(acmeChallengeResponses, cancellationToken);

            try
            {
                var challenges = await ValidateChallengesAsync(challengeContexts, cancellationToken);
                var challengeExceptions = challenges
                    .Where(x => x.Status == ChallengeStatus.Invalid)
                    .Select(x => new Exception(x.Error.Type + ": " + x.Error.Detail))
                    .ToArray();
                if (challengeExceptions.Length > 0)
                {
                    throw new AcmeRequestException(
                        "One or more LetsEncrypt orders were invalid. Make sure that LetsEncrypt can contact the domain you are trying to request an SSL certificate for, in order to verify it.",
                        new AggregateException(challengeExceptions));
                }
            }
            finally
            {
                await _challengeStore.SaveChallengesAsync(null, cancellationToken);
            }
        }

        private static async Task<Challenge[]> ValidateChallengesAsync(IChallengeContext[] challengeContexts, CancellationToken cancellationToken)
        {
            var challenges = await Task.WhenAll(challengeContexts.Select(x => x.Validate()));

            while (challenges.Any(x => x.Status == ChallengeStatus.Pending))
            {
                await Task.Delay(1000);
                challenges = await Task.WhenAll(challengeContexts.Select(x => x.Resource()));
            }

            return challenges;
        }
    }
}

