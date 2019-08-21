using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Bet.AspNetCore.LetsEncrypt.Abstractions;

using Certes;
using Certes.Acme;
using Certes.Acme.Resource;

using Microsoft.Extensions.Logging;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    internal class LetsEncryptService : ILetsEncryptService
    {
        private readonly ILogger<LetsEncryptService> _logger;
        private readonly IChallengeStore _challengeStore;

        private IAcmeContext _acmeContext;

        public LetsEncryptService(
            IChallengeStore challengeStore,
            ILogger<LetsEncryptService> logger)
        {
            _challengeStore = challengeStore ?? throw new ArgumentNullException(nameof(challengeStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IKey> AuthenticateWithExistingAccount(
            Uri acmeServer,
            IKey account,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("Acme authenticating using existing Let's Encrypt account...");

            cancellationToken.ThrowIfCancellationRequested();

            if (_acmeContext != null)
            {
                return _acmeContext.AccountKey;
            }

            _acmeContext = new AcmeContext(acmeServer, account);

            await _acmeContext.Account();

            return _acmeContext.AccountKey;
        }

        public async Task<IKey> AuthenticateWithNewAccount(
            string email,
            Uri acmeServer,
            bool termsOfServiceAgreed,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("Acme authenticating and creating Let's Encrypt account with email {email}...", email);

            cancellationToken.ThrowIfCancellationRequested();

            if (_acmeContext != null)
            {
                return _acmeContext.AccountKey;
            }

            _acmeContext = new AcmeContext(acmeServer);
            await _acmeContext.NewAccount(email, true);

            return _acmeContext.AccountKey;
        }

        public async Task<(X509Certificate2 cert, byte[] rawCert)> AcquireNewCertificateForHosts(
                string hostName,
                CsrInfo certificateSigningRequest,
                string certificateFriendlyName,
                string certificatePassword,
                CancellationToken cancellationToken)
        {
            _logger.LogInformation("Ordering LetsEncrypt certificate for hosts {0}.", new object[] { hostName });
            var order = await _acmeContext.NewOrder(new string[] { hostName });
            await ValidateOrderAsync(order, cancellationToken);
            var certificateBytes = await AcquireCertificateBytesFromOrderAsync(
                order,
                certificateSigningRequest,
                certificateFriendlyName,
                certificatePassword,
                cancellationToken);
            if (certificateBytes == null)
            {
                throw new InvalidOperationException("The certificate from the order was null.");
            }

            var certificate = new X509Certificate2(certificateBytes, certificatePassword);
            return (certificate, certificateBytes);
        }

        private static async Task<Challenge[]> ValidateChallengesAsync(IChallengeContext[] challengeContexts, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var challenges = await Task.WhenAll(challengeContexts.Select(x => x.Validate()));
            while (challenges.Any(x => x.Status == ChallengeStatus.Pending))
            {
                await Task.Delay(1000);
                challenges = await Task.WhenAll(challengeContexts.Select(x => x.Resource()));
            }

            return challenges;
        }

        private async Task<byte[]> AcquireCertificateBytesFromOrderAsync(
            IOrderContext order,
            CsrInfo certificateSigningRequest,
            string certificateFriendlyName,
            string certificatePassword,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Acquiring certificate through signing request.");

            var privateKey = KeyFactory.NewKey(KeyAlgorithm.ES256);

            var certificateChain = await order.Generate(certificateSigningRequest, privateKey);

            var pfxBuilder = certificateChain.ToPfx(privateKey);

            pfxBuilder.FullChain = true;

            var pfxBytes = pfxBuilder.Build(certificateFriendlyName, certificatePassword);

            _logger.LogInformation("Certificate acquired.");

            return pfxBytes;
        }

        private async Task ValidateOrderAsync(IOrderContext order, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var allAuthorizations = await order.Authorizations();
            var challengeContexts = await Task.WhenAll(allAuthorizations.Select(x => x.Http()));

            _logger.LogInformation("Validating all pending order authorizations.");

            var acmeChallengeResponses = challengeContexts.Select(x => new AcmeChallengeResponse(x.Token, x.KeyAuthz)).ToArray();

            foreach (var acmeChallengeResponse in acmeChallengeResponses)
            {
                await _challengeStore.SaveChallengesAsync(acmeChallengeResponse, cancellationToken);
            }

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
    }
}
