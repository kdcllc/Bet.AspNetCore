using Newtonsoft.Json;

namespace Bet.AspNetCore.Jwt.Options
{
    public class JwtTokenAuthOptions
    {
        [JsonProperty("secret")]
        public string Secret { get; set; } = string.Empty;

        [JsonProperty("issuer")]
        public string Issuer { get; set; } = string.Empty;

        [JsonProperty("audience")]
        public string Audience { get; set; } = string.Empty;

        [JsonProperty("accessExpiration")]
        public int AccessExpiration { get; set; } = 30;

        [JsonProperty("refreshExpiration")]
        public int RefreshExpiration { get; set; } = 60;

        [JsonProperty("salt")]
        public string Salt { get; set; } = string.Empty;
    }
}
