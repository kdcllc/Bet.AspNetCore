namespace Bet.AspNetCore.LetsEncrypt.Abstractions
{
    /// <summary>
    /// Acme Let's Encrypt response object.
    /// </summary>
    public class AcmeChallengeResponse
    {
        public AcmeChallengeResponse(string token, string response)
        {
            Token = token;
            Response = response;
        }

        public string Token { get; set; }

        public string Response { get; set; }
    }
}
