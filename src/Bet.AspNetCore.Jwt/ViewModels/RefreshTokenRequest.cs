using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace Bet.AspNetCore.Jwt.ViewModels
{
    public class RefreshTokenRequest
    {
        [Required]
        [JsonPropertyName("access_token")]
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("refresh_token")]
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
