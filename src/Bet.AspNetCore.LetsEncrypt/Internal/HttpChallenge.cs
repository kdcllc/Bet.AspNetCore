using System;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.LetsEncrypt.Order;

using Certes;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    public class HttpChallenge
    {
        private readonly IAcmeOrderClient _orderClient;

        public HttpChallenge(IAcmeOrderClient orderClient)
        {
            _orderClient = orderClient ?? throw new ArgumentNullException(nameof(orderClient));
        }

        public async Task<byte[]> GetCertificateAsync(string named, CancellationToken cancellationToken)
        {
            var order = await _orderClient.StartOrderAsync(named, x => x.Http(), CancellationToken.None);

            await Task.Delay(100);

            return await _orderClient.CompleteOrderAsync(named, order, cancellationToken);
        }
    }
}
