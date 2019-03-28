using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppAuthentication
{
    internal class EnvironmentHostedService : IHostedService
    {
        private WebHostBuilderOptions _options;
        private readonly ILogger<EnvironmentHostedService> _logger;

        public EnvironmentHostedService(
            WebHostBuilderOptions options,
            ILogger<EnvironmentHostedService> logger)
        {
            _options = options;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(Constants.MsiAppServiceEndpointEnv, EnvironmentVariableTarget.User)) &&
                !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(Constants.MsiAppServiceSecretEnv, EnvironmentVariableTarget.User)))
            {
                _logger.LogTrace("{serviceName} resetting User Environment variables.");
                ResetVariables();
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var envMsiUrl = $"{string.Format(Constants.HostUrl, Constants.MsiContainerUrl, _options.Port)}{Constants.MsiEndpoint}";

            Environment.SetEnvironmentVariable(
                Constants.MsiAppServiceEndpointEnv,
                envMsiUrl,
                EnvironmentVariableTarget.User);

            Environment.SetEnvironmentVariable(
                Constants.MsiAppServiceSecretEnv,
                _options.SecretId,
                EnvironmentVariableTarget.User);

            _logger.LogTrace(
                "{serviceName} ended setting up User Environment variables {url}-{secret}",
                nameof(EnvironmentHostedService),
                envMsiUrl,
                _options.SecretId);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Delete the environment variables
            ResetVariables();

            _logger.LogTrace("{serviceName} cleared up User Environment variables", nameof(EnvironmentHostedService));
            return Task.CompletedTask;
        }

        internal static void ResetVariables()
        {
            Environment.SetEnvironmentVariable(Constants.MsiAppServiceEndpointEnv, null, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable(Constants.MsiAppServiceSecretEnv, null, EnvironmentVariableTarget.User);
        }
    }
}
