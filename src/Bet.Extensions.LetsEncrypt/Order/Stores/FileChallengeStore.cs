using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;

namespace Bet.Extensions.LetsEncrypt.Order.Stores
{
    public class FileChallengeStore : IAcmeChallengeStore
    {
        private readonly FileChallengeStoreOptions _options;

        public FileChallengeStore(IOptions<FileChallengeStoreOptions> options)
        {
            _options = options.Value;
        }

        public bool Configured => _options.Configured;

        public Task DeleteAsync(string name, CancellationToken cancellationToken)
        {
            var fullPath = Path.Combine(_options.RootPath, name);
            if (!File.Exists(fullPath))
            {
                return Task.CompletedTask;
            }

            return Task.Run(() => File.Delete(fullPath), cancellationToken);
        }

        public async Task<T?> LoadAsync<T>(string name, CancellationToken cancellationToken) where T : class
        {
            var fullPath = Path.Combine(_options.RootPath, name);
            if (!File.Exists(fullPath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(fullPath, cancellationToken);

            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task SaveAsync<T>(T value, string name, CancellationToken cancellationToken)
        {
            var fullPath = Path.Combine(_options.RootPath, name);
            var directoryPath = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var json = JsonConvert.SerializeObject(value, Formatting.Indented);
            await File.WriteAllTextAsync(fullPath, json, cancellationToken);
        }
    }
}
