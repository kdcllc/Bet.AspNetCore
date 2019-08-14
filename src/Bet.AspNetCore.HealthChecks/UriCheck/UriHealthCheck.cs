using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.HealthChecks.UriCheck
{
    public class UriHealthCheck : IHealthCheck
    {
        private readonly IOptionsMonitor<UriHealthCheckOptions> _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
        private bool _isHealthy = true;

        public UriHealthCheck(
            IOptionsMonitor<UriHealthCheckOptions> options,
            IHttpClientFactory httpClientFactory)
        {
            _options = options;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var index = 0;
            try
            {
                var checkName = context.Registration.Name;

                var options = _options.Get(checkName);

                foreach (var option in options.UriOptions)
                {
                    var httpClient = _httpClientFactory.CreateClient(checkName);

                    using (var requestMessage = new HttpRequestMessage(option.HttpMethod, option.Uri))
                    {
                        foreach (var (name, value) in option.Headers)
                        {
                            requestMessage.Headers.Add(name, value);
                        }

                        using (var timeoutSource = new CancellationTokenSource(option.Timeout))
                        using (var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken))
                        {
                            var response = await httpClient.SendAsync(requestMessage, linkedSource.Token);

                            if (!((int)response.StatusCode >= option.ExpectedHttpCodes.Min && (int)response.StatusCode <= option.ExpectedHttpCodes.Max))
                            {
                                _isHealthy = false;

                                var errorMessage = $"Discover endpoint #{index} is not responding with code in {option.ExpectedHttpCodes.Min}...{option.ExpectedHttpCodes.Max} range, the current status is {response.StatusCode}.";

                                _data.Add(option.Uri.ToString(), errorMessage);
                            }
                            else
                            {
                                var message = $"Discovered endpoint #{index} is responding with {response.StatusCode}.";
                                _data.Add(option.Uri.ToString(), message);
                            }

                            ++index;
                        }
                    }
                }

                var status = _isHealthy ? HealthStatus.Healthy : context.Registration.FailureStatus;

                return new HealthCheckResult(
                    status,
                    description: $"Reports degraded status if one of {index} failed",
                    exception: null,
                    data: _data);
            }
            catch (Exception ex)
            {
                // TODO: not expose all of the exception details for security reasons.
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
