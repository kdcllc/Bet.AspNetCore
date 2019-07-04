using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Certes;

namespace Bet.AspNetCore.LetsEncrypt.Abstractions
{
    public interface ICertificateStore
    {
        /// <summary>
        /// Gets ACME Account information.
        /// </summary>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns>Returns ACME key that was used to sign the SSL certificate. </returns>
        Task<IKey> GetAccountAsync(CancellationToken cancellationToken);

        /// <summary>
        ///  Saves ACME SSL Certificate.
        /// </summary>
        /// <param name="certificate">The ACME certificate. </param>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns></returns>
        Task SaveAccountAsync(IKey certificate, CancellationToken cancellationToken);

        /// <summary>
        /// Gets SSL Certificate.
        /// </summary>
        /// <param name="hostName">The name of the host.</param>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns>Returns <see cref="X509Certificate2"/> saved certificate. </returns>
        Task<X509Certificate2> GetCertificateAsync(string hostName, CancellationToken cancellationToken);

        /// <summary>
        /// Saves ACME SSL Certificate.
        /// </summary>
        /// <param name="hostName">The name of the host.</param>
        /// <param name="certificateBytes">The certificate.</param>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns></returns>
        Task SaveCertificateAsync(string hostName, byte[] certificateBytes, CancellationToken cancellationToken);
    }
}
