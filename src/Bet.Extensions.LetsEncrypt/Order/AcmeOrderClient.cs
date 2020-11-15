using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.LetsEncrypt.Account;
using Bet.Extensions.LetsEncrypt.Certificates;
using Certes;
using Certes.Acme;
using Certes.Acme.Resource;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.LetsEncrypt.Order
{
    public class AcmeOrderClient : IAcmeOrderClient
    {
        private readonly ILogger<AcmeOrderClient> _logger;
        private readonly IOptionsMonitor<AcmeAccountOptions> _accountOptionsMonitor;
        private readonly IOptionsMonitor<AcmeOrderOptions> _orderOptionsMonitor;
        private readonly IAcmeContextClientFactory _acmeContextClientFactory;
        private readonly IOptionsMonitor<CertificateOptions> _certificateOptionsMonitor;

        public AcmeOrderClient(
            IAcmeContextClientFactory acmeContextClientFactory,
            IOptionsMonitor<AcmeAccountOptions> accountOptionsMonitor,
            IOptionsMonitor<AcmeOrderOptions> orderOptionsMonitor,
            IOptionsMonitor<CertificateOptions> certificateOptionsMonitor,
            ILogger<AcmeOrderClient> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountOptionsMonitor = accountOptionsMonitor ?? throw new ArgumentNullException(nameof(accountOptionsMonitor));
            _orderOptionsMonitor = orderOptionsMonitor ?? throw new ArgumentNullException(nameof(orderOptionsMonitor));
            _acmeContextClientFactory = acmeContextClientFactory ?? throw new ArgumentNullException(nameof(acmeContextClientFactory));
            _certificateOptionsMonitor = certificateOptionsMonitor;
        }

        public async Task<AcmeStartedOrder> StartOrderAsync(
            string named,
            Func<IAuthorizationContext, Task<IChallengeContext>> selector,
            CancellationToken cancellationToken)
        {
            var accountClient = await _acmeContextClientFactory.GetContext(named, cancellationToken);

            var accountOptions = _accountOptionsMonitor.Get(named);
            var orderOptions = _orderOptionsMonitor.Get(named);

            _logger.LogInformation("[LetsEncrypt][Ordering] certificate for domains {0}.", new object[] { accountOptions.Domains });

            var challengesStore = orderOptions.ChallengesStore;

            if (challengesStore == null)
            {
                throw new NullReferenceException($"{nameof(orderOptions.ChallengesStore)} wasn't set");
            }

            var order = await accountClient.NewOrder(accountOptions.Domains);

            var allAuthorizations = await order.Authorizations();

            var challengeContexts = await Task.WhenAll(allAuthorizations.Select(selector));

            var nonNullChallengeContexts = challengeContexts.Where(x => x != null).ToArray();

            var challenges = nonNullChallengeContexts.Select((x, c) => new AcmeChallengeResponse
            {
                Token = x.Type == ChallengeTypes.Dns01 ? accountClient.AccountKey.DnsTxt(x.Token) : x.Token,
                Response = x.KeyAuthz,
                Domain = accountOptions.Domains[c]
            }).ToArray();

            foreach (var challenge in challenges)
            {
                await challengesStore.SaveAsync(challenge, challenge.Token, cancellationToken);
            }

            _logger.LogInformation("[LetsEncrypt][Order Placed] certificate for domains {0}.", new object[] { accountOptions.Domains });

            return new AcmeStartedOrder(challenges, order, nonNullChallengeContexts);
        }

        public async Task<byte[]> CompleteOrderAsync(
            string named,
            AcmeStartedOrder placedOrder,
            CancellationToken cancellationToken)
        {
            var accountOptions = _accountOptionsMonitor.Get(named);
            var orderOptions = _orderOptionsMonitor.Get(named);
            var certificateOptions = _certificateOptionsMonitor.Get(named);

            var challengesStore = orderOptions.ChallengesStore;

            if (challengesStore == null)
            {
                throw new NullReferenceException($"{nameof(orderOptions.ChallengesStore)} wasn't set");
            }

            await ValidateChallenges(placedOrder.ChallengeContexts, orderOptions);

            var certificateBytes = await AcquireCertificateBytesFromOrderAsync(
                placedOrder.Order,
                orderOptions,
                accountOptions,
                certificateOptions);

            foreach (var challenge in placedOrder.Challenges)
            {
                await challengesStore.DeleteAsync(challenge.Token, cancellationToken);
            }

            return certificateBytes;
        }

        private async Task<byte[]> AcquireCertificateBytesFromOrderAsync(
            IOrderContext order,
            AcmeOrderOptions orderOptions,
            AcmeAccountOptions accountOptions,
            CertificateOptions certificateOptions)
        {
            _logger.LogInformation("[LetsEncrypt][Certificate] Acquiring certificate through signing request.");

            var privateKey = KeyFactory.NewKey((Certes.KeyAlgorithm)orderOptions.KeyAlgorithm);

            if (orderOptions?.CertificateSigningRequest == null)
            {
                var commonName = accountOptions.Domains[0];
                _logger.LogDebug("Creating cert for {commonName}", commonName);

                var csrInfo = new CsrInfo
                {
                    CommonName = commonName,
                };

                if (orderOptions != null)
                {
                    orderOptions.CertificateSigningRequest = csrInfo;
                }
            }

            var certificateChain = await order.Generate(orderOptions?.CertificateSigningRequest, privateKey);

            var pfxBuilder = certificateChain.ToPfx(privateKey);

            pfxBuilder.FullChain = true;

            var pfxBytes = pfxBuilder.Build(
                $"Let's Encrypt - {accountOptions.Domains[0]} ",
                certificateOptions?.CertificatePassword ?? string.Empty);

            _logger.LogInformation("[LetsEncrypt][Certificate] Certificate acquired.");

            return pfxBytes;
        }

        private async Task ValidateChallenges(IChallengeContext[] challengeContexts, AcmeOrderOptions orderOptions)
        {
            _logger.LogInformation("[LetsEncrypt][Validating] all pending order authorizations.");

            var challengeValidationResponses = await ValidateChallengesAsync(challengeContexts, orderOptions);

            var nonNullChallengeValidationResponses = challengeValidationResponses.Where(x => x != null).ToArray();

            if (challengeValidationResponses.Length > nonNullChallengeValidationResponses.Length)
            {
                _logger.LogInformation("[LetsEncrypt][Validating] Some challenge responses were null.");
            }

            var challengeExceptions = nonNullChallengeValidationResponses
                .Where(x => x.Status == ChallengeStatus.Invalid)
                .Select(x => new Exception($"{x.Error?.Type ?? "errortype null"}: {x.Error?.Detail ?? "null errordetails"} (challenge type {x.Type ?? "null"})"))
                .ToArray();

            if (challengeExceptions.Length > 0)
            {
                throw new LetsEncryptException(
                    "One or more LetsEncrypt orders were invalid." +
                    " Make sure that LetsEncrypt can contact the domain you are trying to request an SSL certificate for, in order to verify it.",
                    new AggregateException(challengeExceptions));
            }

            _logger.LogInformation("[LetsEncrypt][Validating] Completed.");
        }

        private async Task<Challenge[]> ValidateChallengesAsync(IChallengeContext[] challengeContexts, AcmeOrderOptions orderOptions)
        {
            var challenges = await Task.WhenAll(challengeContexts.Select(x => x.Validate()));

            var retries = 0;
            var delay = TimeSpan.FromSeconds(orderOptions.ValidationDelay);
            while (true)
            {
                var anyValid = challenges.Any(x => x.Status == ChallengeStatus.Valid);
                var anyPending = challenges.Any(x => x.Status == ChallengeStatus.Pending);

                var allInvalid = challenges.All(x => (x.Status == ChallengeStatus.Invalid && x.Type == ChallengeTypes.Http01));

                if ((anyValid && !anyPending)
                    || (allInvalid && retries >= orderOptions.ValidationRetry))
                {
                    break;
                }

                if (retries >= orderOptions.ValidationRetry)
                {
                    break;
                }

                await Task.Delay(delay);
                challenges = await Task.WhenAll(challengeContexts.Select(x => x.Resource()));
                retries++;
            }

            return challenges;
        }
    }
}
