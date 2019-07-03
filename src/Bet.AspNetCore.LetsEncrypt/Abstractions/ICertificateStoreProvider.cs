using System.Threading;
using System.Threading.Tasks;

namespace Bet.AspNetCore.LetsEncrypt.Abstractions
{
    /// <summary>
    /// This interface defines a specific strategy for certificates persistence.
    /// </summary>
    public interface ICertificateStoreProvider
    {
        /// <summary>
        /// Saves ACME account information.
        /// </summary>
        /// <param name="bytes">The account information.</param>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns></returns>
        Task SaveAccountCertificateAsync(byte[] bytes, CancellationToken cancellationToken);

        /// <summary>
        /// Gets ACME account information.
        /// </summary>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns></returns>
        Task<byte[]> GetAccountCertificateAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Saves Certificate.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns></returns>
        Task SaveSiteCertificateAsync(byte[] bytes, CancellationToken cancellationToken);

        /// <summary>
        /// Gets Certificate.
        /// </summary>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns></returns>
        Task<byte[]> GetSiteCertificateAsync(CancellationToken cancellationToken);
    }
}
