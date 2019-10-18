using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.Hosting;
using Bet.Extensions.Hosting.Abstractions;
using Bet.Extensions.ML.ModelBuilder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Hosting.Sample.Services
{
    public class ModelBuilderHostedService : TimedHostedService
    {
        private readonly IServiceProvider _provider;

        public ModelBuilderHostedService(
            IServiceProvider provider,
            IOptionsMonitor<TimedHostedServiceOptions> options,
            IEnumerable<ITimedHostedLifeCycleHook> lifeCycleHooks,
            ILogger<ITimedHostedService> logger) : base(options, lifeCycleHooks, logger)
        {
            TaskToExecuteAsync = (token) => RunModelGenertorsAsync(token);
            _provider = provider;
        }

        public async Task RunModelGenertorsAsync(CancellationToken cancellationToken)
        {
            using var scope = _provider.CreateScope();
            var modelBuilders = scope.ServiceProvider.GetRequiredService<IEnumerable<IModelBuilderService>>();

            foreach (var modelBuilder in modelBuilders)
            {
                try
                {
                    await modelBuilder.TrainModelAsync(cancellationToken);

                    await modelBuilder.ClassifyTestAsync(cancellationToken);

                    await modelBuilder.SaveModelAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    Logger.LogError("{modelBuilder} failed with exception: {message}", modelBuilder.GetType(), ex.Message);
                }
            }
        }
    }
}
