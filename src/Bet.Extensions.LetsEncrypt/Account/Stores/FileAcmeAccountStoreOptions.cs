namespace Bet.Extensions.LetsEncrypt.Account.Stores
{
    public class FileAcmeAccountStoreOptions : AcmeAccountStoreOptions
    {
        public string RootPath { get; set; } = string.Empty;
    }
}
