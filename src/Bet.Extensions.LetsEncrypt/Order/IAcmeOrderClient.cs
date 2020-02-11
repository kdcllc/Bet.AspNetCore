using System;
using System.Threading;
using System.Threading.Tasks;

using Certes.Acme;

namespace Bet.Extensions.LetsEncrypt.Order
{
    public interface IAcmeOrderClient
    {
        Task<byte[]> CompleteOrderAsync(string named, AcmeStartedOrder placedOrder, CancellationToken cancellationToken);

        Task<AcmeStartedOrder> StartOrderAsync(
            string named,
            Func<IAuthorizationContext, Task<IChallengeContext>> selector,
            CancellationToken cancellationToken);
    }
}
