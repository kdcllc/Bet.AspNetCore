using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace Bet.Extensions.HealthChecks.SigtermCheck
{
    public class SigtermHealthCheck : IHealthCheck
    {
#if NETCOREAPP3_0 || NETSTANDARD2_1
        private readonly IHostApplicationLifetime _applicationLifetime;

        public SigtermHealthCheck(IHostApplicationLifetime applicationLifetime)
        {
            _applicationLifetime = applicationLifetime;
        }
#else
        private readonly IApplicationLifetime _applicationLifetime;

        public SigtermHealthCheck(IApplicationLifetime applicationLifetime)
        {
            _applicationLifetime = applicationLifetime;
        }
#endif

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
