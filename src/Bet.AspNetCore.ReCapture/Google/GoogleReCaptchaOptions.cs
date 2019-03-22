using System.ComponentModel.DataAnnotations;

namespace Bet.AspNetCore.ReCapture
{
    public class GoogleReCaptchaOptions
    {
        [Required]
        public string ClientKey { get; set; }

        [Required]
        public string SecretKey { get; set; }

        public string ValidationMessage { get; set; }

        public string Script { get; set; }
    }
}
