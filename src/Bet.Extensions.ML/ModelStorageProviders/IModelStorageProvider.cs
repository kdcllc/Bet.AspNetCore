using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Primitives;

namespace Bet.Extensions.ML.ModelStorageProviders
{
    /// <summary>
    /// ML model storage provider.
    /// </summary>
    public interface IModelStorageProvider
    {
        /// <summary>
        /// Loads Raw ML.NET data from the storage provider.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<TResult>> LoadRawDataAsync<TResult>(string fileName, CancellationToken cancellationToken);

        /// <summary>
        /// Loads Saved Model and return a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Stream> LoadModelAsync(string fileName, CancellationToken cancellationToken);

        /// <summary>
        /// Saves ML.NET model from the stream to the storage.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SaveModelAsync(string fileName, Stream stream, CancellationToken cancellationToken);

        /// <summary>
        /// Loads ML.NET Model builder results.
        /// </summary>
        /// <typeparam name="TResult">The type of the ML.NET result.</typeparam>
        /// <param name="fileName">The name of the model result.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResult> LoadModelResultAsync<TResult>(string fileName, CancellationToken cancellationToken);

        /// <summary>
        /// Saves the results from Model Training.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="result"></param>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SaveModelResultAsync<TResult>(TResult result, string fileName, CancellationToken cancellationToken);

        /// <summary>
        /// Raises the change token to be used with the model consumer.
        /// </summary>
        /// <returns></returns>
        IChangeToken GetReloadToken();
    }
}
