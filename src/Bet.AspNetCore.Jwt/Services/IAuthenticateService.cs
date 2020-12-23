using System.Threading;
using System.Threading.Tasks;

using Bet.AspNetCore.Jwt.ViewModels;

namespace Bet.AspNetCore.Jwt.Services
{
    public interface IAuthenticateService
    {
        Task<AuthorizeTokenResponse> GetTokenAsync(AuthorizeTokenRequest request, CancellationToken cancellationToken);

        Task<AuthorizeTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellation);

        Task<bool> RevokeAsync(string userName, CancellationToken cancellation);
    }
}
