using System.Threading;
using System.Threading.Tasks;

using Bet.AspNetCore.Jwt.ViewModels;

namespace Bet.AspNetCore.Jwt.Services
{
    public interface IUserStore : IUserService
    {
        Task DeleteAsync(string userName, CancellationToken cancellationToken = default);

        Task<AuthorizeUser> GetUserByNameAsync(string userName, CancellationToken cancellationToken = default);

        Task SaveAsync(AuthorizeUser user, CancellationToken cancellationToken = default);
    }
}
