namespace Bet.Extensions.LetsEncrypt.Account.Stores
{
    public class AzureAcmeAccountStoreOptions : AcmeAccountStoreOptions
    {
        public string NamedOption { get; set; } = string.Empty;
    }
}
