namespace Bet.Extensions.LetsEncrypt.Certificates.Stores
{
    public class CertificateStoreOptions
    {
        public bool Configured { get; set; }

        public string NamedOption { get; set; } = string.Empty;
    }
}
