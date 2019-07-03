using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using Certes;

namespace Bet.AspNetCore.LetsEncrypt.Abstractions
{
    public interface ICertificateStore
    {
        /// <summary>
        /// Gets ACME Account Certificate.
        /// </summary>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns>Returns ACME key that was used to sign the SSL certificate. </returns>
        Task<IKey> GetAccountCertificateAsync(CancellationToken cancellationToken);

        /// <summary>
        ///  Saves ACME SSL Certificate.
        /// </summary>
        /// <param name="certificate">The ACME certificate. </param>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns></returns>
        Task SaveAccountCertificateAsync(IKey certificate, CancellationToken cancellationToken);

        /// <summary>
        /// Gets site specific SSL Certificate.
        /// </summary>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns>Returns <see cref="X509Certificate2"/> saved certificate. </returns>
        Task<X509Certificate2> GetSiteCertificateAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Saves ACME SSL Certificate.
        /// </summary>
        /// <param name="certificateBytes"></param>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns></returns>
        Task SaveSiteCertificateAsync(byte[] certificateBytes, CancellationToken cancellationToken);
    }
}
