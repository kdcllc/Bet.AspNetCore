using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting
{
    public class HostStartupService : BackgroundService
    {
        private readonly IEnumerable<IHostStartupFilter> _filters;
        private readonly ILogger<HostStartupService> _logger;
        private readonly IServiceProvider _provider;

        public HostStartupService(
            IEnumerable<IHostStartupFilter> filters,
            IServiceProvider provider,
            ILogger<HostStartupService> logger)
        {
            _filters = filters;
            _logger = logger;
            _provider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("{Service} is starting.", nameof(HostStartupService));

            stoppingToken.Register(() => _logger.LogDebug("{Service} is stopping.", nameof(HostStartupService)));

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_filters != null)
                {
                    foreach (var filter in _filters)
                    {
                        filter.Configure(_provider);
                    }

                    break;
                }

                _logger.LogDebug("{Service}  is running.", nameof(HostStartupService));
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }

            _logger.LogDebug("{Service}  background task is stopping.", nameof(HostStartupService));
        }
    }
}
