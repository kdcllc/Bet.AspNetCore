using System;

using Bet.AspNetCore.ApiKeyAuthentication;
using Bet.AspNetCore.ApiKeyAuthentication.Options;

namespace Microsoft.AspNetCore.Authentication
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddApiKeyHeaderSupport(this AuthenticationBuilder authenticationBuilder, Action<ApiKeyAuthenticationOptions> options)
        {
            return authenticationBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyHeaderAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, options);
        }

        public static AuthenticationBuilder AddApiKeyQuerySupport(this AuthenticationBuilder authenticationBuilder, Action<ApiKeyAuthenticationOptions> options)
        {
            return authenticationBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyQueryAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, options);
        }
    }
}
