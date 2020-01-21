using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.Helpers;

namespace Bet.Extensions.ML.DataLoaders.ModelLoaders
{
    public class FileModelLoader : ModelLoader
    {
        protected override Func<string, Stream, CancellationToken, Task> SaveModelActionAsync { get; set; }
            = async (fileName, stream, cancellationToken) =>
        {
            var fileLocation = FileHelper.GetAbsolutePath(fileName);

            using var fs = new FileStream(fileLocation, FileMode.Create, FileAccess.Write, FileShare.Write);
            stream.Position = 0;
            await stream.CopyToAsync(fs, (int)stream.Length, cancellationToken);
        };

        protected override Func<string, string, CancellationToken, Task> SaveModelResultActionAsync { get; set; }
            = async (fileName, json, cancellationToken) =>
        {
            await Task.Run(
                () =>
                {
                    var fileLocation = FileHelper.GetAbsolutePath(fileName);
                    File.WriteAllText(fileLocation, json);
                },
                cancellationToken);
        };
    }
}
