using System;
using System.Text;

using Bet.AspNetCore.Jwt.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.AspNetCore.Authentication.JwtBearer
{
    public class PostConfigureJwtBearerOptions : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly IConfiguration _configuration;

        public PostConfigureJwtBearerOptions(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
        }

        public void PostConfigure(string name, JwtBearerOptions options)
        {
            var tokenOptions = _configuration.GetSection(nameof(JwtTokenAuthOptions)).Get<JwtTokenAuthOptions>();

            options.RequireHttpsMetadata = false;
            options.SaveToken = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                LifetimeValidator = (before, expires, token, param) =>
                {
                    return expires > DateTime.UtcNow;
                },

                ValidateIssuer = true,
                ValidateAudience = true,

                ValidateIssuerSigningKey = true,

                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenOptions.Secret)),
                ValidIssuer = tokenOptions.Issuer,
                ValidAudience = tokenOptions.Audience,

                // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                ClockSkew = TimeSpan.Zero
            };
        }
    }
}
