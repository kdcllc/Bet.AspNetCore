namespace Bet.Extensions.LetsEncrypt.Order
{
    public class ChallengeResult
    {
        public string Token { get; set; } = string.Empty;

        public string Response { get; set; } = string.Empty;

        public string Domain { get; set; } = string.Empty;
    }
}
