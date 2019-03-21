using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppAuthentication
{
    internal class EnvironmentHostedService : IHostedService
    {
        private int _port;

        public EnvironmentHostedService(WebHostBuilderOptions options)
        {
            _port = options.Port;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Environment.SetEnvironmentVariable(
                Constants.MsiAppServiceEndpointEnv,
                $"{string.Format(Constants.HostUrl, _port)}{Constants.MsiEndpoint}",
                EnvironmentVariableTarget.User);

            Environment.SetEnvironmentVariable(
                Constants.MsiAppServiceSecretEnv,
                Guid.NewGuid().ToString(),
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
