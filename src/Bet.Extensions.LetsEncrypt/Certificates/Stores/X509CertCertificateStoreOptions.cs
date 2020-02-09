namespace Bet.Extensions.LetsEncrypt.Certificates.Stores
{
    public class X509CertCertificateStoreOptions : CertificateStoreOptions
    {
        public bool AllowInvalidCerts { get; set; }
    }
}
