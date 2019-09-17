using System;
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
        private readonly object _lock = new object();

        private ReloadToken _reloadToken;

        public FileModelStorageProvider()
        {
            _reloadToken = new ReloadToken();
        }

        /// <summary>
        /// Loading the raw data from the file.
        /// </summary>
        /// <typeparam name="TResult">The result set of the retrieved values.</typeparam>
        /// <param name="name">The absolute path to the '.csv' or '.tsv'.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task<IEnumerable<TResult>> LoadRawDataAsync<TResult>(string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileLocation = FileHelper.GetAbsolutePath(name);

            using (var reader = new StreamReader(fileLocation))
            using (var csv = new CsvReader(reader))
            {
                csv.Configuration.HeaderValidated = null;
                csv.Configuration.MissingFieldFound = null;

                return Task.FromResult(csv.GetRecords<TResult>());
            }
        }

        /// <summary>
        /// Loads previous saved model into <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<MemoryStream> LoadModelAsync(string name, CancellationToken cancellationToken)
        {
            return await GetMemoryStream(name, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Save ML.NET Model to the file system.
        /// </summary>
        /// <param name="name">The absolute path to the file.</param>
        /// <param name="stream"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task SaveModelAsync(string name, MemoryStream stream, CancellationToken cancellationToken)
        {
            return Task.Run(
                () =>
                {
                    var previousToken = Interlocked.Exchange(ref _reloadToken, new ReloadToken());

                    lock (_lock)
                    {
                        // var fileLocation = FileHelper.GetAbsolutePath($"{name}-{DateTime.UtcNow.Ticks}.zip");
                        var fileLocation = FileHelper.GetAbsolutePath($"{name}.zip");

                        using (var fs = new FileStream(fileLocation, FileMode.Create, FileAccess.Write, FileShare.Write))
                        {
                            stream.WriteTo(fs);
                        }
                    }

                    previousToken.OnReload();
                },
                cancellationToken);
        }

        /// <summary>
        /// Save ML.NET training/evaluation results.
        /// </summary>
        /// <typeparam name="TResult">The type of the results to be saved.</typeparam>
        /// <param name="result"></param>
        /// <param name="name"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task SaveModelResultAsync<TResult>(TResult result, string name, CancellationToken cancellationToken)
        {
            return Task.Run(
                () =>
            {
                lock (_lock)
                {
                    // var fileLocation = FileHelper.GetAbsolutePath($"{name}-{DateTime.UtcNow.Ticks}.json");
                    var fileLocation = FileHelper.GetAbsolutePath($"{name}.json");

                    var json = JsonConvert.SerializeObject(result, Formatting.Indented);
                    File.WriteAllText(fileLocation, json);
                }
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

        private async Task<MemoryStream> GetMemoryStream(string name, CancellationToken cancellationToken)
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
