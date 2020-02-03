using System;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.AzureStorage;
using Bet.Extensions.AzureStorage.Options;

using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.HealthChecks.AzureStorage
{
    public class AzureQueueStorageHealthCheck : IHealthCheck
    {
        private readonly IOptionsMonitor<StorageQueueOptions> _storageQueueOptionsMonitor;
        private readonly ILogger<AzureQueueStorageHealthCheck> _logger;
        private readonly IStorageQueue<StorageQueueOptions> _storageQueue;

        public AzureQueueStorageHealthCheck(
            IOptionsMonitor<StorageQueueOptions> storageQueueOptionsMonitor,
            IStorageQueue<StorageQueueOptions> storageQueue,
            ILogger<AzureQueueStorageHealthCheck> logger)
        {
            _storageQueueOptionsMonitor = storageQueueOptionsMonitor ?? throw new ArgumentNullException(nameof(storageQueueOptionsMonitor));
            _storageQueue = storageQueue ?? throw new ArgumentNullException(nameof(storageQueue));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var checkName = context.Registration.Name;
            var options = _storageQueueOptionsMonitor.Get(checkName);
            var fullCheckName = $"HealCheck Name:{checkName}; Queue Name:{options?.QueueName}";

            try
            {
                _logger.LogInformation("[HealthCheck][{checkName}] Queue: {queueName}", checkName, options?.QueueName);

                var queue = await _storageQueue.GetNamedQueue(checkName, cancellationToken).Value;

                if (queue != null)
                {
                    var blobClient = queue.ServiceClient;

                    var serviceProperties = await blobClient.GetServicePropertiesAsync(
                        new QueueRequestOptions(),
                        operationContext: null,
                        cancellationToken: cancellationToken);

                    return new HealthCheckResult(HealthStatus.Healthy, fullCheckName);
                }

                return new HealthCheckResult(context.Registration.FailureStatus, $"{checkName} failed to get {nameof(CloudStorageAccount)} instance for Queue: {options?.QueueName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HealthCheck][{checkName}] Queue: {queueName}", checkName, options?.QueueName);

                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
