using Certes.Acme;

namespace Bet.Extensions.LetsEncrypt.Order
{
    public class AcmeStartedOrder
    {
        public ChallengeResult[] Challenges { get; }

        public IOrderContext Order { get; }

        public IChallengeContext[] ChallengeContexts { get; }

        public AcmeStartedOrder(
            ChallengeResult[] challenges,
            IOrderContext order,
            IChallengeContext[] challengeContexts)
        {
            Challenges = challenges;
            Order = order;
            ChallengeContexts = challengeContexts;
        }
    }
}
