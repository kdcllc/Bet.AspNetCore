using System;

namespace Bet.Extensions.AzureAppConfiguration.Options
{
    public class AzureAppConfigurationWorkerOptions
    {
        public TimeSpan? RefreshInterval { get; set; } = TimeSpan.FromSeconds(30);
    }
}
