using System.Threading;
using System.Threading.Tasks;

namespace Bet.AspNetCore.LetsEncrypt.Abstractions
{
    /// <summary>
    /// This interface defines a specific strategy for approval challenges responses persistence.
    /// </summary>
    public interface IChallengeStoreProvider
    {
        /// <summary>
        /// Saves the Approval Challenge Response.
        /// </summary>
        /// <param name="bytes">The approval challenge response to save.</param>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns></returns>
        Task SaveAsync(byte[] bytes, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the Approval Challenge Response.
        /// </summary>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns></returns>
        Task<byte[]> GetAsync(CancellationToken cancellationToken);
    }
}
