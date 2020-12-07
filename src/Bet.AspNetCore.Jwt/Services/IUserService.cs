using System.Threading;
using System.Threading.Tasks;

namespace Bet.AspNetCore.Jwt.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Validates Existing User of the Api.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> IsValidUserAsync(string username, string password, CancellationToken cancellationToken);
    }
}
