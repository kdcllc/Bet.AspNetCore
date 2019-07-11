using System.Threading;
using System.Threading.Tasks;

namespace Bet.AspNetCore.LetsEncrypt.Abstractions
{
    public interface IChallengeStore
    {
        /// <summary>
        /// Gets ACME Challenges response.
        /// </summary>
        /// <param name="responseToken"></param>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns>Returns task of ACME Challenges response. </returns>
        Task<AcmeChallengeResponse> GetChallengesAsync(string responseToken, CancellationToken cancellationToken);

        /// <summary>
        /// Saves ACME Challenges response.
        /// </summary>
        /// <param name="challenges">ACME challenge response.</param>
        /// <param name="cancellationToken">The task cancellation token.</param>
        /// <returns></returns>
        Task SaveChallengesAsync(AcmeChallengeResponse challenges, CancellationToken cancellationToken);
    }
}
