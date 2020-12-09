using System.Collections.Generic;
using System.Security.Claims;

namespace Bet.AspNetCore.Jwt.Services
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);

        string GenerateRefreshToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
