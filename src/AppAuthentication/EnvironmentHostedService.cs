using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppAuthentication
{
    internal class EnvironmentHostedService : IHostedService
    {
        private WebHostBuilderOptions _options;

        public EnvironmentHostedService(WebHostBuilderOptions options)
        {
            _options = options;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Environment.SetEnvironmentVariable(
                Constants.MsiAppServiceEndpointEnv,
                $"{string.Format(Constants.HostUrl,Constants.MsiContinerUrl,_options.Port)}{Constants.MsiEndpoint}",
                EnvironmentVariableTarget.User);

            Environment.SetEnvironmentVariable(
                Constants.MsiAppServiceSecretEnv,
                _options.SecretId,
                EnvironmentVariableTarget.User);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Delete the environment variables
            Environment.SetEnvironmentVariable(Constants.MsiAppServiceEndpointEnv, null, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable(Constants.MsiAppServiceSecretEnv, null, EnvironmentVariableTarget.User);

            return Task.CompletedTask;
        }
    }
}
