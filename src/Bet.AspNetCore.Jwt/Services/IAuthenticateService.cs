using System.Threading;
using System.Threading.Tasks;

using Bet.AspNetCore.Jwt.ViewModels;

namespace Bet.AspNetCore.Jwt.Services
{
    public interface IAuthenticateService
    {
        Task<TokenResponse> IsAuthenticatedAsync(TokenRequest request, CancellationToken cancellationToken);
    }
}
