using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Bet.Extensions.LetsEncrypt.Certificates.Stores
{
    public interface ICertificateStore
    {
        string NamedOption { get; }

        bool Configured { get; }

        Task<X509Certificate2?> LoadAsync(string name, string certificatePassword, CancellationToken cancellationToken);

        Task SaveAsync(byte[] value, string name, CancellationToken cancellationToken);
    }
}
