using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.Helpers;

using CsvHelper;

using Microsoft.Extensions.Primitives;

using Newtonsoft.Json;

namespace Bet.Extensions.ML.ModelStorageProviders
{
    /// <summary>
    /// The implementation of <see cref="IModelStorageProvider"/> for local file system.
    /// </summary>
    public class FileModelStorageProvider : IModelStorageProvider
    {
        private ReloadToken _reloadToken;

        public FileModelStorageProvider()
        {
            _reloadToken = new ReloadToken();
        }

        /// <summary>
        /// Loads previous saved model into <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Stream> LoadModelAsync(string fileName, CancellationToken cancellationToken)
        {
            return await GetMemoryStream(fileName, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Save ML.NET Model to the file system.
        /// </summary>
        /// <param name="fileName">The absolute path to the file.</param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task SaveModelAsync(string fileName, Stream stream, CancellationToken cancellationToken)
        {
            var previousToken = Interlocked.Exchange(ref _reloadToken, new ReloadToken());

            var fileLocation = FileHelper.GetAbsolutePath(fileName);

            using var fs = new FileStream(fileLocation, FileMode.Create, FileAccess.Write, FileShare.Write);
            stream.Position = 0;
            await stream.CopyToAsync(fs, (int)stream.Length, cancellationToken);

            previousToken.OnReload();
        }

        /// <summary>
        /// Save ML.NET training/evaluation results.
        /// </summary>
        /// <typeparam name="TResult">The type of the results to be saved.</typeparam>
        /// <param name="result"></param>
        /// <param name="fileName"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task SaveModelResultAsync<TResult>(TResult result, string fileName, CancellationToken cancellationToken)
        {
            return Task.Run(
                () =>
                {
                    var fileLocation = FileHelper.GetAbsolutePath(fileName);

                    var json = JsonConvert.SerializeObject(result, Formatting.Indented);
                    File.WriteAllText(fileLocation, json);
                },
                cancellationToken);
        }

        /// <summary>
        /// Load ML.NET result.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TResult> LoadModelResultAsync<TResult>(string name, CancellationToken cancellationToken)
        {
            var ms = await GetMemoryStream(name, cancellationToken);
            using (var reader = new StreamReader(ms))
            {
                var obj = JsonConvert.DeserializeObject<TResult>(reader.ReadToEnd());

                return await Task.FromResult(obj);
            }
        }

        public IChangeToken GetReloadToken()
        {
            return _reloadToken;
        }

        private async Task<Stream> GetMemoryStream(string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileLocation = FileHelper.GetAbsolutePath(name);

#pragma warning disable CA2000 // Dispose objects before losing scope
            var fs = new FileStream(fileLocation, FileMode.Open, FileAccess.Read);
#pragma warning restore CA2000 // Dispose objects before losing scope
            var ms = new MemoryStream();

            await fs.CopyToAsync(ms).ConfigureAwait(false);
            return await Task.FromResult(ms);
        }
    }
}
