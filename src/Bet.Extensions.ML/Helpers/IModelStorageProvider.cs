using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Bet.Hosting.Sample.Services
{
    public interface IModelStorageProvider
    {
        /// <summary>
        /// Saves ML.NET model from the stream to the storage.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SaveModelAsync(string name, MemoryStream stream, CancellationToken cancellationToken);

        /// <summary>
        /// Saves the results from Model Training.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SaveResultsAsync<TResult>(TResult result, string name, CancellationToken cancellationToken);
    }
}
