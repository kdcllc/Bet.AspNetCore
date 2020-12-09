using System;
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

        /// <summary>
        /// Update refresh token.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="refreshToken"></param>
        /// <param name="refreshTokenExpiryTime"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> UpdateRefreshTokenAsync(
            string userName,
            string refreshToken,
            DateTime refreshTokenExpiryTime,
            CancellationToken cancellationToken);

        /// <summary>
        /// Validate existing refresh token.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="refreshToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ValidateRefreshTokenAsync(
            string userName,
            string refreshToken,
            CancellationToken cancellationToken);
    }
}
