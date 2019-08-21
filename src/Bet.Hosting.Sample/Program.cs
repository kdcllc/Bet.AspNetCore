using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bet.Hosting.Sample
{
    /// <summary>
    /// https://github.com/dotnet/machinelearning/blob/master/docs/code/MlNetCookBook.md.
    /// </summary>
    internal sealed class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                    .ConfigureLogging(logging =>
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    })
                    .ConfigureAppConfiguration((hostContext, config) =>
                    {
                        config.AddEnvironmentVariables();
                        config.AddJsonFile("appsettings.json", optional: true);
                        config.AddCommandLine(args);
                    })
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddModelBuilderService();
                    })
                    .UseConsoleLifetime()
                    .Build();

            var hostedServices = host.Services;

            using (host)
            {
                await host.StartAsync();

                // Wait for the host to shutdown
                await host.WaitForShutdownAsync();
            }
        }
    }
}
