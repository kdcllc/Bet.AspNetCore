using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.Helpers;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Bet.Extensions.ML.DataLoaders.ModelLoaders
{
    public class FileModelLoader : ModelLoader, IDisposable
    {
        private readonly ILogger<FileModelLoader> _logger;

        private ReloadToken? _reloadToken;
        private bool _disposed;
        private FileSystemWatcher? _watcher;
        private int _fileSystemWatcherCounts;

        public FileModelLoader(ILogger<FileModelLoader> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            SaveResultFunc = async (options, json, cancellationToken) =>
            {
                await Task.Run(
                    () =>
                    {
                        var fileLocation = FileHelper.GetAbsolutePath(options.ModelResultFileName);
                        File.WriteAllText(fileLocation, json);
                    },
                    cancellationToken);
            };

            LoadFunc = (options, cancellationToken) => GetMemoryStream(options.ModelResultFileName, cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public override async Task SaveAsync(Stream stream, CancellationToken cancellationToken)
        {
            try
            {
                var previousToken = Interlocked.Exchange(ref _reloadToken, new ReloadToken());

                var fileLocation = FileHelper.GetAbsolutePath(Options.ModelFileName);

                using var fs = new FileStream(fileLocation, FileMode.Create, FileAccess.Write, FileShare.Write);
                stream.Position = 0;
                await stream.CopyToAsync(fs, (int)stream.Length, cancellationToken);

                _logger.LogInformation("[{filename}][{methodName}]", Options.ModelFileName, nameof(SaveAsync));

                previousToken?.OnReload();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public override Task<Stream> LoadAsync(CancellationToken cancellationToken)
        {
            return GetMemoryStream(Options.ModelFileName, cancellationToken);
        }

        public override IChangeToken GetReloadToken()
        {
            if (_reloadToken == null)
            {
                throw new InvalidOperationException($"{nameof(FileModelLoader)} failed to call {nameof(Setup)} method.");
            }

            return _reloadToken;
        }

        protected override void Polling()
        {
            _reloadToken = new ReloadToken();

            if (Options.WatchForChanges)
            {
                var fileLocation = FileHelper.GetAbsolutePath(Options.ModelFileName);
                var fileDirectory = Path.GetDirectoryName(fileLocation);

                _watcher = new FileSystemWatcher(fileDirectory, Options.ModelFileName);
                _watcher.EnableRaisingEvents = true;
                _watcher.NotifyFilter = NotifyFilters.LastWrite;
                _watcher.Changed += WatcherChanged;

                _logger.LogInformation(
                    "[{provideName}][Watching] {modelName}-{fileName}",
                    nameof(FileModelLoader),
                    Options.ModelName,
                    Options.ModelFileName);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _watcher?.Dispose();
                }
            }

            _disposed = true;
        }

        private async void WatcherChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed
                || e.ChangeType == WatcherChangeTypes.Created)
            {
                // Filter several calls in short period of time
                Interlocked.Increment(ref _fileSystemWatcherCounts);

                await Task.Delay(50);

                if (Interlocked.Decrement(ref _fileSystemWatcherCounts) == 0)
                {
                    var previousToken = Interlocked.Exchange(ref _reloadToken, new ReloadToken());

                    _logger.LogInformation("{FileName} {fileChange}", Options.ModelFileName, e.ChangeType);

                    previousToken?.OnReload();
                }
            }
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
