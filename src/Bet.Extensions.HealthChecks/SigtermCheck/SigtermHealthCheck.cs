using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Bet.Extensions.HealthChecks.SigtermCheck
{
    public class SigtermHealthCheck : IHealthCheck
    {
        private readonly IHostApplicationLifetime _applicationLifetime;

        public SigtermHealthCheck(IHostApplicationLifetime applicationLifetime)
        {
            _applicationLifetime = applicationLifetime;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (_applicationLifetime.ApplicationStopping.IsCancellationRequested)
            {
                return Task.FromResult(new HealthCheckResult(
                    context.Registration.FailureStatus,
                    description: "IApplicationLifetime.ApplicationStopping was requested"));
            }

            return Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}
