using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Bet.AspNetCore.Jwt.Options;
using Bet.AspNetCore.Jwt.ViewModels;

using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.Jwt.Services
{
    public class JwtTokenAuthenticationService : IAuthenticateService
    {
        private readonly IUserService _userService;
        private readonly JwtTokenAuthOptions _jwtTokenOptions;
        private readonly IJwtTokenService _jwtTokenService;

        public JwtTokenAuthenticationService(
            IUserService service,
            IJwtTokenService jwtTokenService,
            IOptionsSnapshot<JwtTokenAuthOptions> jwtTokenOptions)
        {
            _userService = service ?? throw new ArgumentNullException(nameof(service));
            _jwtTokenOptions = jwtTokenOptions.Value;
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        }

        public async Task<AuthorizeTokenResponse> GetTokenAsync(
            AuthorizeTokenRequest request,
            CancellationToken cancellationToken)
        {
            var result = new AuthorizeTokenResponse();

            if (!await _userService.IsValidUserAsync(request.UserName, request.Password, cancellationToken))
            {
                return result;
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, request.UserName)
            };

            try
            {
                result.Success = true;
                result.AccessToken = _jwtTokenService.GenerateAccessToken(claims);
                result.ExpiresIn = DateTimeOffset.UtcNow.AddMinutes(_jwtTokenOptions.AccessExpiration).Ticks;
                result.RefreshToken = _jwtTokenService.GenerateRefreshToken();

                await _userService.UpdateRefreshTokenAsync(
                    request.UserName,
                    result.RefreshToken,
                    DateTime.Now.Add(TimeSpan.FromMinutes(_jwtTokenOptions.AccessExpiration)),
                    cancellationToken);
            }
            catch
            {
                result.Success = false;
            }

            return result;
        }

        public async Task<AuthorizeTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var result = new AuthorizeTokenResponse();

            try
            {
                var accessToken = request.AccessToken;
                var refreshToken = request.RefreshToken;

                var principal = _jwtTokenService.GetPrincipalFromExpiredToken(accessToken);

                // this is mapped to the Name claim by default
                var username = principal.Identity.Name;

                if (!await _userService.ValidateRefreshTokenAsync(username ?? string.Empty, refreshToken, cancellationToken))
                {
                    return result;
                }

                result.Success = true;
                result.AccessToken = _jwtTokenService.GenerateAccessToken(principal.Claims);
                result.ExpiresIn = DateTimeOffset.UtcNow.AddMinutes(_jwtTokenOptions.AccessExpiration).Ticks;
                result.RefreshToken = _jwtTokenService.GenerateRefreshToken();

                await _userService.UpdateRefreshTokenAsync(
                    username ?? string.Empty,
                    result.RefreshToken,
                    DateTime.Now.Add(TimeSpan.FromMinutes(_jwtTokenOptions.RefreshExpiration)),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                result.Success = false;
            }

            return result;
        }

        public Task<bool> RevokeAsync(string userName, CancellationToken cancellation)
        {
            return _userService.UpdateRefreshTokenAsync(userName, string.Empty, default, cancellation);
        }
    }
}
