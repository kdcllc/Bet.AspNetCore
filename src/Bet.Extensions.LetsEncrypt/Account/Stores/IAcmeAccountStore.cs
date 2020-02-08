using System.Threading;
using System.Threading.Tasks;

using Certes;

namespace Bet.Extensions.LetsEncrypt.Account.Stores
{
    public interface IAcmeAccountStore
    {
        Task SaveAsync(IKey value, string name, CancellationToken cancellationToken);

        Task<IKey?> LoadAsync(string name, CancellationToken cancellationToken);
    }
}
