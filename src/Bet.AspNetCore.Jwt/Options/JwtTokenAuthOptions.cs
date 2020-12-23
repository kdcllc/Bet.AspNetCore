using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace Bet.AspNetCore.Jwt.Options
{
    public class JwtTokenAuthOptions
    {
        [JsonPropertyName("secret")]
        [JsonProperty("secret")]
        public string Secret { get; set; } = string.Empty;

        [JsonPropertyName("issuer")]
        [JsonProperty("issuer")]
        public string Issuer { get; set; } = string.Empty;

        [JsonPropertyName("audience")]
        [JsonProperty("audience")]
        public string Audience { get; set; } = string.Empty;

        [JsonPropertyName("accessExpiration")]
        [JsonProperty("accessExpiration")]
        public int AccessExpiration { get; set; } = 30;

        [JsonPropertyName("refreshExpiration")]
        [JsonProperty("refreshExpiration")]
        public int RefreshExpiration { get; set; } = 180;

        [JsonPropertyName("salt")]
        [JsonProperty("salt")]
        public string Salt { get; set; } = string.Empty;
    }
}
