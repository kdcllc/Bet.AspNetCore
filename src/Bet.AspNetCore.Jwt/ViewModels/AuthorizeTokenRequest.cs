using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace Bet.AspNetCore.Jwt.ViewModels
{
    public class AuthorizeTokenRequest
    {
        [Required]
        [JsonPropertyName("username")]
        [JsonProperty("username")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [JsonPropertyName("password")]
        [JsonProperty("password")]
        public string Password { get; set; } = string.Empty;
    }
}
