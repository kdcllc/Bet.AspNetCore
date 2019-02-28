using System.ComponentModel.DataAnnotations;

namespace Bet.AspNetCore.Sample
{
    public class AppSetting
    {
        [Required]
        public string Title { get; set; }

        [EmailAddress]
        public string Email { get; set; }
    }
}
