using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting
{
    public class HostStartupService : IHostedService
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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("{ServiceName} is starting...", nameof(HostStartupService));

            cancellationToken.Register(() => _logger.LogDebug("{ServiceName} is stopping...", nameof(HostStartupService)));

            var count = 0;

            if (_filters != null)
            {
                foreach (var filter in _filters)
                {
                    filter.Configure(_provider);
                    Interlocked.Increment(ref count);
                }
            }

            _logger.LogDebug("{ServiceName} finished configuring 'IHostStartupFilters' with Total Count of {TotalCount}.", nameof(HostStartupService));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("{ServiceName}  background task is stopping.", nameof(HostStartupService));
            return Task.CompletedTask;
        }
    }
}
