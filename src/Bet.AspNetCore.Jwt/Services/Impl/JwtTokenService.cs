using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Bet.AspNetCore.Jwt.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Bet.AspNetCore.Jwt.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtTokenAuthOptions _jwtTokenOptions;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(
            IOptionsSnapshot<JwtTokenAuthOptions> jwtTokenOptions,
            ILogger<JwtTokenService> logger)
        {
            _jwtTokenOptions = jwtTokenOptions.Value;
            _logger = logger;
        }

        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtTokenOptions.Secret));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var jwtToken = new JwtSecurityToken(
                    _jwtTokenOptions.Issuer,
                    _jwtTokenOptions.Audience,
                    claims,
                    expires: DateTime.Now.AddMinutes(_jwtTokenOptions.AccessExpiration),
                    signingCredentials: credentials);
                return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Access Token Generation failed");
                throw;
            }
        }

        public string GenerateRefreshToken()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[64];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtTokenOptions.Secret)),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            return securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)
                ? throw new SecurityTokenException("Invalid token")
                : principal;
        }
    }
}
