using System;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.ModelCreation.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Bet.Hosting.Sample
{
    /// <summary>
    /// https://github.com/dotnet/machinelearning/blob/master/docs/code/MlNetCookBook.md.
    /// </summary>
    internal sealed class Program
    {
        private static bool runAsCronJob = false;

        /// <summary>
        /// Run in two different ways:
        /// 1. As Timed Hosted Service.
        /// 2. As CronJob in Kubernetes Cluster. dotnet run -- --runAsCronJob=true.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<int> Main(string[] args)
        {
            using var mutex = new Mutex(true, nameof(Program), out var canCreateNew);
            if (canCreateNew)
            {
                using var host = CreateHostBuilder(args).UseConsoleLifetime().Build();
                if (runAsCronJob)
                {
                    await host.StartAsync();

                    var scope = host.Services.CreateScope();
                    var appLifeTime = scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();

                    var job = scope.ServiceProvider.GetRequiredService<IModelCreationService>();
                    await job.BuildModelsAsync(appLifeTime.ApplicationStopping);

                    await host.StopAsync();
                    return 0;
                }

                await host.RunAsync();
                return 0;
            }
            else
            {
                Console.WriteLine($"Only one instance of the {nameof(Program)} tool can be run at the same time.");
                return -1;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                {
                    // based on environment Development = dev; Production = prod prefix in Azure Vault.
                    var envName = hostingContext.HostingEnvironment.EnvironmentName;

                    var configuration = configBuilder.AddAzureKeyVault(
                        hostingEnviromentName: envName,
                        usePrefix: false,
                        reloadInterval: TimeSpan.FromSeconds(30));

                    runAsCronJob = configuration.GetValue<bool>(nameof(runAsCronJob));

                    // helpful to see what was retrieved from all of the configuration providers.
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        // var configuration = configBuilder.Build();
                        configuration.DebugConfigurations();
                    }
                })
                .UseSerilog((hostingContext, sp, loggerBuilder) =>
                {
                    var applicationName = $"BetHostingSample-{hostingContext.HostingEnvironment.EnvironmentName}";
                    loggerBuilder
                            .ReadFrom.Configuration(hostingContext.Configuration)
                            .Enrich.FromLogContext()
                            .WriteTo.Console()
                            .AddApplicationInsights(sp);

                            // .AddAzureLogAnalytics(hostingContext.Configuration, applicationName: applicationName);
                })
                .ConfigureServices(services =>
                {
                    services.AddApplicationInsightsTelemetryWorkerService();

                    if (runAsCronJob)
                    {
                        services.AddMachineLearningModels();
                    }
                    else
                    {
                        services.AddHealthChecks()

                                // memory check
                                .AddMemoryHealthCheck()

                                // dummy check
                                .AddCheck("Healthy_Check_Two", () => HealthCheckResult.Healthy())

                                // health check for the worker process.
                                .AddSocketListener(8080)

                                // publisher to publish logs.
                                .AddLoggerPublisher();

                        // adds ML.NET model generations every 30 mins.
                        services.AddMachineLearningHostedService();
                    }
                });
        }
    }
}
