using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Bet.AspNetCore.HealthChecks.SigtermCheck
{
    public class SigtermHealthCheck : IHealthCheck
    {
        private readonly IApplicationLifetime _applicationLifetime;

        public SigtermHealthCheck(IApplicationLifetime applicationLifetime)
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
