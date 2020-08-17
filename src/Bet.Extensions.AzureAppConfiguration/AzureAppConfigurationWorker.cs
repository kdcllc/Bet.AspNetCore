using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureAppConfiguration.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.AzureAppConfiguration
{
    public sealed class AzureAppConfigurationWorker : BackgroundService
    {
        private readonly ILogger<AzureAppConfigurationWorker> _logger;
        private AzureAppConfigurationWorkerOptions _options;

        public AzureAppConfigurationWorker(
            IOptionsMonitor<AzureAppConfigurationWorkerOptions> optionsMonitor,
            IConfiguration configuration,
            ILogger<AzureAppConfigurationWorker> logger)
        {
            _options = optionsMonitor.CurrentValue;

            optionsMonitor.OnChange(n => _options = n);

            Refreshers = new List<IConfigurationRefresher>();

            if (!(configuration is IConfigurationRoot configurationRoot))
            {
                throw new InvalidOperationException("Unable to access the Azure App Configuration provider. Please ensure that it has been configured correctly.");
            }

            foreach (var provider in configurationRoot.Providers)
            {
                if (provider is IConfigurationRefresher refresher)
                {
                    Refreshers.Add(refresher);
                }
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public List<IConfigurationRefresher> Refreshers { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested
                && _options.RefreshInterval != null)
            {
                _logger.LogInformation("{name} running Refresh: {time}", nameof(AzureAppConfigurationWorker), DateTimeOffset.Now);

                foreach (var refresher in Refreshers)
                {
                    _ = refresher.TryRefreshAsync();
                }

                await Task.Delay(_options.RefreshInterval.Value, stoppingToken);
            }
        }
    }
}
