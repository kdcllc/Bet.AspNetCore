using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.HealthChecks.PingCheck
{
    public class PingHealthCheck : IHealthCheck
    {
        private readonly IOptionsMonitor<PingHealthCheckOptions> _optionsMonitor;
        private readonly ILogger<PingHealthCheck> _logger;

        public PingHealthCheck(
            IOptionsMonitor<PingHealthCheckOptions> optionsMonitor,
            ILogger<PingHealthCheck> logger)
        {
            _optionsMonitor = optionsMonitor;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var options = _optionsMonitor.Get(context.Registration.Name);
            var description = $"{context.Registration.Name}-{options.Host}";

            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(options.Host, options.Timeout);
                    if (reply.Status != IPStatus.Success)
                    {
                        return new HealthCheckResult(HealthStatus.Unhealthy, description);
                    }

                    if (reply.RoundtripTime >= options.Timeout)
                    {
                        return new HealthCheckResult(HealthStatus.Degraded, description);
                    }

                    return new HealthCheckResult(HealthStatus.Healthy, description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, description);

                return new HealthCheckResult(HealthStatus.Unhealthy, description, ex);
            }
        }
    }
}
