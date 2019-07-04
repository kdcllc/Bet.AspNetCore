using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Certes;

namespace Bet.AspNetCore.LetsEncrypt.Abstractions
{
    internal interface ILetsEncryptService
    {
        Task<(X509Certificate2 cert, byte[] rawCert)> AcquireNewCertificateForHosts(
            string hostName,
            CsrInfo certificateSigningRequest,
            string certificateFriendlyName,
            string certificatePassword,
            CancellationToken cancellationToken);

        Task<IKey> AuthenticateWithExistingAccount(Uri acmeServer, IKey account, CancellationToken cancellationToken);

        Task<IKey> AuthenticateWithNewAccount(string email, Uri acmeServer, bool termsOfServiceAgreed, CancellationToken cancellationToken);
    }
}
