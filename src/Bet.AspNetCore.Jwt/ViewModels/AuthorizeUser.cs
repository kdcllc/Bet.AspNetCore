using System;

namespace Bet.AspNetCore.Jwt.ViewModels
{
    public class AuthorizeUser
    {
        public int Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
