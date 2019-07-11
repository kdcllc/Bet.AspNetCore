using System;
using System.Threading.Tasks;
using Bet.Extensions.Hosting.Abstractions;
using Bet.Hosting.Sample.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Bet.Hosting.Sample
{
    /// <summary>
    /// https://github.com/dotnet/machinelearning/blob/master/docs/code/MlNetCookBook.md
    /// </summary>
    internal class Program
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

