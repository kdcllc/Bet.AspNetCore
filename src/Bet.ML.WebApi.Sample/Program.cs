using System.Threading.Tasks;

using Bet.ML.WebApi.Sample.Jobs;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bet.ML.WebApi.Sample
{
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // begin start up jobs.
            await host.RunStartupJobsAync();

            // start the server.
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();

                        webBuilder.ConfigureServices(services =>
                        {
                            services.AddStartupJob<InitMLModelBuildJob>();
                        });
                    });
        }
    }
}
