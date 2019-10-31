using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.HealthChecks.AzureStorage
{
    public class AzureQueueStorageHealthCheck : IHealthCheck
    {
        private readonly IOptionsMonitor<StorageAccountOptions> _options;
        private readonly ILogger<AzureQueueStorageHealthCheck> _logger;

        public AzureQueueStorageHealthCheck(
            IOptionsMonitor<StorageAccountOptions> options,
            ILogger<AzureQueueStorageHealthCheck> logger)
        {
            _options = options;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var checkName = context.Registration.Name;
            var options = _options.Get(checkName);
            var fullCheckName = $"{checkName}-{options?.QueueName}";

            try
            {
                _logger.LogInformation("[HealthCheck][{healthCheckName}]", fullCheckName);

                if (options?.CloudStorageAccount != null)
                {
                    var cloudStorageAccount = await options.CloudStorageAccount.Value;
                    var blobClient = cloudStorageAccount.CreateCloudQueueClient();
                    var serviceProperties = await blobClient.GetServicePropertiesAsync(
                        new QueueRequestOptions(),
                        operationContext: null,
                        cancellationToken: cancellationToken);
                    return new HealthCheckResult(HealthStatus.Healthy, fullCheckName);
                }

                return new HealthCheckResult(context.Registration.FailureStatus, $"{fullCheckName} failed to get {nameof(CloudStorageAccount)} instance");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HealthCheck][{healthCheckName}]", fullCheckName);

                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
