namespace Bet.Extensions.LetsEncrypt.Order.Stores
{
    public class FileChallengeStoreOptions : ChallengeStoreOptions
    {
        public string RootPath { get; set; } = string.Empty;
    }
}
