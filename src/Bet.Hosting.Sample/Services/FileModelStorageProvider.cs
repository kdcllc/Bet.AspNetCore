using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Bet.Hosting.Sample.Services
{
    public class FileModelStorageProvider : IModelStorageProvider
    {
        public Task SaveModelAsync(string name, MemoryStream stream, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                using (var fs = new FileStream(name, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    stream.WriteTo(fs);
                }
            },
            cancellationToken);
        }

        public Task SaveResultsAsync<TResult>(TResult result, string name, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var json = JsonConvert.SerializeObject(result, Formatting.Indented);
                File.WriteAllText(name, json);
            },
            cancellationToken);
        }
    }
}
