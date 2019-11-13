using System.ComponentModel.DataAnnotations;

namespace Bet.AspNetCore.ReCapture
{
    public class GoogleReCaptchaOptions
    {
        [Required]
        public string ClientKey { get; set; } = string.Empty;

        [Required]
        public string SecretKey { get; set; } = string.Empty;

        public string ValidationMessage { get; set; } = string.Empty;

        public string Script { get; set; } = string.Empty;
    }
}
