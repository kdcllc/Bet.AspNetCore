using System;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;

using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.HealthChecks.AzureStorage
{
    public class AzureBlobStorageHealthCheck : IHealthCheck
    {
        private readonly IOptionsMonitor<StorageBlobOptions> _storageBlobOptionsMonitor;
        private readonly IStorageBlob<StorageBlobOptions> _storageBlob;

        private ILogger<AzureBlobStorageHealthCheck> _logger;

        public AzureBlobStorageHealthCheck(
            IStorageBlob<StorageBlobOptions> storageBlob,
            IOptionsMonitor<StorageBlobOptions> storageBlobOptionsMonitor,
            ILogger<AzureBlobStorageHealthCheck> logger)
        {
            _storageBlob = storageBlob ?? throw new ArgumentNullException(nameof(storageBlob));
            _storageBlobOptionsMonitor = storageBlobOptionsMonitor ?? throw new ArgumentNullException(nameof(storageBlobOptionsMonitor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var checkName = context.Registration.Name;
            var options = _storageBlobOptionsMonitor.Get(checkName);
            var fullCheckName = $"HealthCheck Name:{checkName}; Container Name:{options?.ContainerName}";

            try
            {
                _logger.LogInformation("[HealthCheck][{checkName}] Container Name: {containerName}", checkName, options?.ContainerName);

                var container = await _storageBlob.GetNamedContainer(checkName, cancellationToken).Value;

                if (container != null)
                {
                    var cloudBlobClient = container.ServiceClient;

                    var serviceProperties = await cloudBlobClient.GetServicePropertiesAsync(
                        new BlobRequestOptions(),
                        operationContext: null,
                        cancellationToken: cancellationToken);

                    return new HealthCheckResult(HealthStatus.Healthy, fullCheckName);
                }

                return new HealthCheckResult(context.Registration.FailureStatus, $"{checkName} failed to get {nameof(CloudStorageAccount)} instance for Container Name: {options?.ContainerName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HealthCheck][{checkName}] Container Name: {containerName}", checkName, options?.ContainerName);

                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
