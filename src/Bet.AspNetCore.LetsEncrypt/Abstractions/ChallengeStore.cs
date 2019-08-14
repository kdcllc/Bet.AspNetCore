using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Bet.AspNetCore.LetsEncrypt.Abstractions
{
    /// <inheritdoc/>
    public class ChallengeStore : IChallengeStore
    {
        private readonly IEnumerable<IChallengeStoreProvider> _providers;
        private readonly ILogger<IChallengeStore> _logger;

        public ChallengeStore(
            IEnumerable<IChallengeStoreProvider> providers,
            ILogger<IChallengeStore> logger)
        {
            _providers = providers ?? throw new ArgumentNullException(nameof(providers));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<AcmeChallengeResponse> GetChallengesAsync(string responseToken, CancellationToken cancellationToken)
        {
            byte[] bytes = null;

            foreach (var provider in _providers)
            {
                bytes = await provider.GetAsync(responseToken, cancellationToken);
                if (bytes?.Length > 0)
                {
                    break;
                }
            }

            if (bytes != null)
            {
                var text = Encoding.UTF8.GetString(bytes);

                _logger.LogDebug("Retrieved Approval Challenges {0}", text);

                return JsonConvert.DeserializeObject<AcmeChallengeResponse>(text);
            }

            _logger.LogDebug("Approval Challenges were not found.");

            return null;
        }

        /// <inheritdoc/>
        public async Task SaveChallengesAsync(
            AcmeChallengeResponse challenges,
            CancellationToken cancellationToken)
        {
            var json = challenges == null ? null : JsonConvert.SerializeObject(challenges);
            _logger.LogDebug("Persisting challenges {0}", json);

            var bytes = json == null ? null : Encoding.UTF8.GetBytes(json);

            var tasks = _providers.Select(p => p.SaveAsync(challenges.Token, bytes ?? Array.Empty<byte>(), cancellationToken));

            await Task.WhenAll(tasks);
        }
    }
}
