using Microsoft.AspNetCore.Authentication;

namespace Bet.AspNetCore.ApiKeyAuthentication.Options
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "API Key";

        public string AuthenticationType = DefaultScheme;

        public string Scheme => DefaultScheme;

        public string HeaderName { get; set; } = string.Empty;

        public string QueryStringName { get; set; } = "token";
    }
}
