using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace Bet.AspNetCore.Jwt.ViewModels
{
    public class AuthorizeTokenResponse
    {
        [JsonPropertyName("token_type")]
        [JsonProperty("token_type")]
        public string TokenType { get; set; } = "Bearer";

        [JsonPropertyName("access_token")]
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        internal bool Success { get; set; }
    }
}
