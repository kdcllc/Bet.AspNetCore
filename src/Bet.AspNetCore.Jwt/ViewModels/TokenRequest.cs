using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

namespace Bet.AspNetCore.Jwt.ViewModels
{
    public class TokenRequest
    {
        [Required]
        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [JsonProperty("password")]
        public string Password { get; set; } = string.Empty;
    }
}
