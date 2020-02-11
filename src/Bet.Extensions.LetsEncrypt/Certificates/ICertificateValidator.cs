using System.Security.Cryptography.X509Certificates;

namespace Bet.Extensions.LetsEncrypt.Certificates
{
    public interface ICertificateValidator
    {
        bool IsCertificateValid(string named, X509Certificate2? certificate);
    }
}
