using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.Helpers;

using Microsoft.Extensions.Logging;

namespace Bet.Extensions.ML.DataLoaders.ModelLoaders
{
    public class FileModelLoader : ModelLoader
    {
        private readonly ILogger<FileModelLoader> _logger;

        public FileModelLoader(ILogger<FileModelLoader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            SaveFunc = async (fileName, stream, cancellationToken) =>
            {
                var fileLocation = FileHelper.GetAbsolutePath(fileName);

                using var fs = new FileStream(fileLocation, FileMode.Create, FileAccess.Write, FileShare.Write);
                stream.Position = 0;
                await stream.CopyToAsync(fs, (int)stream.Length, cancellationToken);
            };

            SaveResultFunc = async (fileName, json, cancellationToken) =>
            {
                await Task.Run(
                    () =>
                    {
                        var fileLocation = FileHelper.GetAbsolutePath(fileName);
                        File.WriteAllText(fileLocation, json);
                    },
                    cancellationToken);
            };

            LoadFunc = (fileName, cancellationToken) => GetMemoryStream(fileName, cancellationToken);
        }

        private async Task<Stream> GetMemoryStream(string name, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fileLocation = FileHelper.GetAbsolutePath(name);

            using var fs = new FileStream(fileLocation, FileMode.Open, FileAccess.Read);
            var ms = new MemoryStream();

            await fs.CopyToAsync(ms).ConfigureAwait(false);
            return await Task.FromResult(ms);
        }
    }
}
