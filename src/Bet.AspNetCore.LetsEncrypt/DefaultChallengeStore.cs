using System.Collections.Generic;

using Bet.AspNetCore.LetsEncrypt.Abstractions;

using Microsoft.Extensions.Logging;

namespace Bet.AspNetCore.LetsEncrypt
{
    public class DefaultChallengeStore : ChallengeStore
    {
        public DefaultChallengeStore(IEnumerable<IChallengeStoreProvider> providers, ILogger<IChallengeStore> logger)
            : base(providers, logger)
        {
        }
    }
}
