using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.HealthChecks.AzureStorage
{
    public class AzureBlobStorageHealthCheck : IHealthCheck
    {
        private IOptionsMonitor<StorageAccountOptions> _options;
        private ILogger<AzureBlobStorageHealthCheck> _logger;

        public AzureBlobStorageHealthCheck(
            IOptionsMonitor<StorageAccountOptions> options,
            ILogger<AzureBlobStorageHealthCheck> logger)
        {
            _options = options;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var checkName = context.Registration.Name;
            var options = _options.Get(checkName);
            var fullCheckName = $"{checkName}-{options?.ContainerName}";

            try
            {
                _logger.LogInformation("[HealthCheck][{healthCheckName}]", fullCheckName);

                if (options?.CloudStorageAccount != null)
                {
                    var cloudStorageAccount = await options.CloudStorageAccount.Value;

                    var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

                    var cloudBlobContainer = cloudBlobClient.GetContainerReference(options.ContainerName);

                    var serviceProperties = await cloudBlobClient.GetServicePropertiesAsync(
                        new BlobRequestOptions(),
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
