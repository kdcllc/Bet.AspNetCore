using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Certes;

using Microsoft.Extensions.Options;

namespace Bet.Extensions.LetsEncrypt.Account
{
    public class AcmeContextClientFactory : IAcmeContextClientFactory
    {
        private IOptionsMonitor<AcmeAccountOptions> _optionsMonitor;
        private Dictionary<string, AcmeContext> _acmeContextCollection = new Dictionary<string, AcmeContext>();

        public AcmeContextClientFactory(IOptionsMonitor<AcmeAccountOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        public async Task<IAcmeContext> GetContext(string named, CancellationToken cancellationToken)
        {
            if (_acmeContextCollection.TryGetValue(named, out var context))
            {
                return context;
            }

            var options = _optionsMonitor.Get(named);
            var presistence = options.AccountStore;

            if (presistence == null)
            {
                throw new NullReferenceException($"{nameof(options.AccountStore)} wasn't configured.");
            }

            var defaulDomain = options.Domains[0];

            // 1. Check if the account is stored
            var accountKey = await presistence.LoadAsync($"{defaulDomain}", cancellationToken);

            AcmeContext acmeContext;

            if (accountKey == null)
            {
                acmeContext = new AcmeContext(options.LetsEncryptUri);
                await acmeContext.NewAccount(options.Email, true);

                await presistence.SaveAsync(acmeContext.AccountKey, $"{defaulDomain}", cancellationToken);
            }
            else
            {
                acmeContext = new AcmeContext(options.LetsEncryptUri, accountKey);
            }

            _acmeContextCollection.Add(named, acmeContext);

            return acmeContext;
        }
    }
}
