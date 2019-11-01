using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Bet.Extensions.HealthChecks.Publishers
{
    public class LoggingHealthCheckPublisher : IHealthCheckPublisher
    {
        private readonly ILogger _logger;
        private readonly IList<string> _excludeList;

        public LoggingHealthCheckPublisher(
            IList<string> excludeList,
            ILogger<LoggingHealthCheckPublisher> logger)
        {
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            _excludeList = excludeList ?? throw new System.ArgumentNullException(nameof(excludeList));
        }

        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            if (report.Status != HealthStatus.Healthy)
            {
                foreach (var entry in report.Entries.Where(m => !_excludeList.Contains(m.Key)))
                {
                    if (entry.Value.Status != HealthStatus.Healthy)
                    {
                        _logger.LogCritical("Service {ServiceName} is not healthy. Current status is {HealthCheckStatus} \n Data: {HealthCheckData} \n Exception: {HealthCheckException}", entry.Key, entry.Value.Status, entry.Value.Data, entry.Value.Exception);
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }
}
