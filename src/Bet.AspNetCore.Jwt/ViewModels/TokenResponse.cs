namespace Bet.AspNetCore.Jwt.ViewModels
{
    public class TokenResponse
    {
        public bool Success { get; set; }

        public string Token { get; set; } = string.Empty;
    }
}
