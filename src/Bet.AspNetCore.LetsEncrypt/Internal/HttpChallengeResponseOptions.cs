using Bet.Extensions.LetsEncrypt.Order.Stores;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    public class HttpChallengeResponseOptions
    {
        public string ValidationPath { get; set; } = "/.well-known/acme-challenge";

        public IAcmeChallengeStore? ChallengeStore { get; set; }
    }
}
