using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Certes;

namespace Bet.AspNetCore.LetsEncrypt.Abstractions
{
    internal interface ILetsEncryptService
    {
        Task<(X509Certificate2 cert, byte[] rawCert)> AcquireNewCertificateForHosts(CancellationToken cancellationToken);

        Task AuthenticateWithExistingAccount(IKey account, CancellationToken cancellationToken);

        Task<IKey> AuthenticateWithNewAccount(CancellationToken cancellationToken);
    }
}
