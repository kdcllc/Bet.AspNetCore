using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bet.Extensions.HealthChecks.CertificateCheck
{
    public class SslCertificateHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SslCertificateHealthCheck(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var checkName = context.Registration.Name;
                var httpClient = _httpClientFactory.CreateClient(checkName);

                using (var response = await httpClient.GetAsync("/", cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                }

                return new HealthCheckResult(HealthStatus.Healthy, $"{checkName}-{httpClient.BaseAddress}");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
