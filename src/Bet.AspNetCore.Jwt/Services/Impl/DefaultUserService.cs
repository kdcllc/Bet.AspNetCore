using System.Threading;
using System.Threading.Tasks;

namespace Bet.AspNetCore.Jwt.Services
{
    public sealed class DefaultUserService : IUserService
    {
        public Task<bool> IsValidUserAsync(string username, string password, CancellationToken cancellationToken)
        {
            // TODO: add some kind of store for user info
            return Task.FromResult(true);
        }
    }
}
