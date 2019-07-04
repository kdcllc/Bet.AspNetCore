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
        /// Saves ACME Account or Site Certificate information.
        /// </summary>
        /// <param name="storeType"></param>
        /// <param name="bytes">The account information.</param>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns></returns>
        Task SaveAsync(string storeType, byte[] bytes, CancellationToken cancellationToken);

        /// <summary>
        /// Gets ACME Account or Site Certificate information.
        /// </summary>
        /// <param name="storeType"></param>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns></returns>
        Task<byte[]> GetAsync(string storeType, CancellationToken cancellationToken);
    }
}
