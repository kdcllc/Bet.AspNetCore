using System.Threading;
using System.Threading.Tasks;

using Certes;

namespace Bet.Extensions.LetsEncrypt.Account
{
    public interface IAcmeContextClientFactory
    {
        Task<IAcmeContext> GetContext(string named, CancellationToken cancellationToken);
    }
}
