namespace Bet.Extensions.LetsEncrypt.Certificates.Stores
{
    public class FileCertificateStoreOptions : CertificateStoreOptions
    {
        public string RootPath { get; set; } = string.Empty;
    }
}
