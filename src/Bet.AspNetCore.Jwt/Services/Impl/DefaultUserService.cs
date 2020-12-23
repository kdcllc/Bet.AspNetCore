using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bet.AspNetCore.Jwt.Services
{
    public sealed class DefaultUserService : IUserService
    {
        private readonly IUserStore _userStore;

        public DefaultUserService(IUserStore userStore)
        {
            _userStore = userStore ?? throw new System.ArgumentNullException(nameof(userStore));
        }

        public Task<bool> IsValidUserAsync(string username, string password, CancellationToken cancellationToken)
        {
            return _userStore.IsValidUserAsync(username, password, cancellationToken);
        }

        public Task<bool> ValidateRefreshTokenAsync(
            string userName,
            string refreshToken,
            CancellationToken cancellationToken)
        {
            return _userStore.ValidateRefreshTokenAsync(userName, refreshToken, cancellationToken);
        }

        public Task<bool> UpdateRefreshTokenAsync(
            string userName,
            string refreshToken,
            DateTime refreshTokenExpiryTime,
            CancellationToken cancellationToken)
        {
            return _userStore.UpdateRefreshTokenAsync(userName, refreshToken, refreshTokenExpiryTime, cancellationToken);
        }
    }
}
