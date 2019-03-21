using System;
using System.Net;
using System.Net.Sockets;

namespace AppAuthentication
{
    public class EnvironmentProvider : IDisposable
    {
        public EnvironmentProvider(int? portNumber)
        {
            var port = portNumber ?? GetRandomUnusedPort();

            // Setup the environment variables
            Environment.SetEnvironmentVariable(Constants.MsiAppServiceEndpointEnv, string.Format(Constants.MsiEndpoint,port));
            Environment.SetEnvironmentVariable(Constants.MsiAppServiceSecretEnv, Guid.NewGuid().ToString());
        }

        public void Dispose()
        {
            // Delete the environment variables
            Environment.SetEnvironmentVariable(Constants.MsiAppServiceEndpointEnv, null);
            Environment.SetEnvironmentVariable(Constants.MsiAppServiceSecretEnv, null);
        }

        private int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 5050);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
