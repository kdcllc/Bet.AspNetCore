using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AppAuthentication
{
    internal class EnvironmentHostedService : IHostedService
    {
        private readonly WebHostBuilderOptions _options;
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
                _logger.LogTrace("{serviceName} resetting User Environment variables.", nameof(EnvironmentHostedService));
                ResetVariables();
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var domain = _options.IsLocal ? Constants.MsiLocalhostUrl : Constants.MsiContainerUrl;

            var envMsiUrl = $"{string.Format(Constants.HostUrl, domain, _options.Port)}{Constants.MsiEndpoint}";

            if (_options.IsLocal)
            {
                Environment.SetEnvironmentVariable(
                   Constants.AzureAuthConnectionStringEnv,
                   Constants.MsiRunAsApp,
                   EnvironmentVariableTarget.User);
            }

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
            Environment.SetEnvironmentVariable(Constants.AzureAuthConnectionStringEnv, null, EnvironmentVariableTarget.User);
        }
    }
}
