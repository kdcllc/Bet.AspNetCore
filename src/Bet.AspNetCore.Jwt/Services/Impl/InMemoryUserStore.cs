using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Bet.AspNetCore.Jwt.Options;
using Bet.AspNetCore.Jwt.ViewModels;

using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.Jwt.Services
{
    public class InMemoryUserStore : IUserStore
    {
        private readonly ConcurrentDictionary<string, AuthorizeUser> _store = new ConcurrentDictionary<string, AuthorizeUser>();
        private UserStoreOptions _options;

        public InMemoryUserStore(IOptionsMonitor<UserStoreOptions> optionsMonitor)
        {
            _options = optionsMonitor.Get(nameof(InMemoryUserStore));

            optionsMonitor.OnChange((o, n) =>
            {
                if (n == nameof(InMemoryUserStore))
                {
                    _options = o;
                }
            });

            foreach (var user in _options.Users)
            {
                _store.AddOrUpdate(user.UserName, i => user, (i, u) => u);
            }
        }

        public Task<AuthorizeUser> GetUserByNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            if (_store.TryGetValue(userName, out var user))
            {
                return Task.FromResult(user);
            }

            throw new ApplicationException("User Not found");
        }

        public Task SaveAsync(AuthorizeUser user, CancellationToken cancellationToken = default)
        {
            Task.Run(() =>
            {
                var result = _store.AddOrUpdate(user.UserName, user, (i, u) => user);
            });

            return Task.CompletedTask;
        }

        public Task DeleteAsync(string userName, CancellationToken cancellationToken = default)
        {
            Task.Run(() =>
            {
                if (!_store.TryRemove(userName, out var user))
                {
                    throw new ApplicationException("User Not found");
                }
            });

            return Task.CompletedTask;
        }

        public Task<bool> IsValidUserAsync(string username, string password, CancellationToken cancellationToken)
        {
            if (_store.TryGetValue(username, out var user)
                && user.UserName == username
                && user.Password == password)
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<bool> UpdateRefreshTokenAsync(
            string userName,
            string refreshToken,
            DateTime refreshTokenExpiryTime,
            CancellationToken cancellationToken)
        {
            if (_store.TryGetValue(userName, out var user))
            {
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = refreshTokenExpiryTime;

                _store.AddOrUpdate(userName, user, (_, __) => user);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<bool> ValidateRefreshTokenAsync(
            string userName,
            string refreshToken,
            CancellationToken cancellationToken)
        {
            if (_store.TryGetValue(userName, out var user))
            {
               if (user.RefreshToken == refreshToken || user.RefreshTokenExpiryTime >= DateTime.Now)
               {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
    }
}
