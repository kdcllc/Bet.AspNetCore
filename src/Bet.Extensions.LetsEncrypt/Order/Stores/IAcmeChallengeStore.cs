using System.Threading;
using System.Threading.Tasks;

namespace Bet.Extensions.LetsEncrypt.Order.Stores
{
    public interface IAcmeChallengeStore
    {
        bool Configured { get;  }

        Task<T?> LoadAsync<T>(string name, CancellationToken cancellationToken) where T : class;

        Task SaveAsync<T>(T value, string name, CancellationToken cancellationToken);

        Task DeleteAsync(string name, CancellationToken cancellationToken);
    }
}
