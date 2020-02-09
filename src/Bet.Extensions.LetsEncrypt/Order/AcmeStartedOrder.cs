using Certes.Acme;

namespace Bet.Extensions.LetsEncrypt.Order
{
    public class AcmeStartedOrder
    {
        public AcmeStartedOrder(
            AcmeChallengeResponse[] challenges,
            IOrderContext order,
            IChallengeContext[] challengeContexts)
        {
            Challenges = challenges;
            Order = order;
            ChallengeContexts = challengeContexts;
        }

        public AcmeChallengeResponse[] Challenges { get; }

        public IOrderContext Order { get; }

        public IChallengeContext[] ChallengeContexts { get; }
    }
}
