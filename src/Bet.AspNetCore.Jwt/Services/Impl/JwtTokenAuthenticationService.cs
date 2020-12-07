using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Bet.AspNetCore.Jwt.Options;
using Bet.AspNetCore.Jwt.ViewModels;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Bet.AspNetCore.Jwt.Services
{
    public class JwtTokenAuthenticationService : IAuthenticateService
    {
        private readonly IUserService _userService;
        private readonly JwtTokenAuthOptions _jwtTokenOptions;

        public JwtTokenAuthenticationService(
            IUserService service,
            IOptionsSnapshot<JwtTokenAuthOptions> jwtTokenOptions)
        {
            _userService = service;
            _jwtTokenOptions = jwtTokenOptions.Value;
        }

        public async Task<TokenResponse> IsAuthenticatedAsync(
            TokenRequest request,
            CancellationToken cancellationToken)
        {
            var result = new TokenResponse();

            if (!await _userService.IsValidUserAsync(request.Username, request.Password, cancellationToken))
            {
                return result;
            }

            var claim = new[]
            {
                new Claim(ClaimTypes.Name, request.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtTokenOptions.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                _jwtTokenOptions.Issuer,
                _jwtTokenOptions.Audience,
                claim,
                expires: DateTime.Now.AddMinutes(_jwtTokenOptions.AccessExpiration),
                signingCredentials: credentials);

            result.Success = true;
            result.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return result;
        }
    }
}
