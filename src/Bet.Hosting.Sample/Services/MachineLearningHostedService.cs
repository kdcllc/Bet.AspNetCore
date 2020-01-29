using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.Hosting;
using Bet.Extensions.Hosting.Abstractions;
using Bet.Extensions.ML.ModelCreation.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Hosting.Sample.Services
{
    public class MachineLearningHostedService : TimedHostedService
    {
        private readonly IServiceProvider _provider;

        public MachineLearningHostedService(
            IServiceProvider provider,
            IOptionsMonitor<TimedHostedServiceOptions> options,
            IEnumerable<ITimedHostedLifeCycleHook> lifeCycleHooks,
            ILogger<ITimedHostedService> logger) : base(options, lifeCycleHooks, logger)
        {
            TaskToExecuteAsync = (token) => BuildMachineLearningModels(token);
            _provider = provider;
        }

        public async Task BuildMachineLearningModels(CancellationToken cancellationToken)
        {
            using var scope = _provider.CreateScope();
            var job = scope.ServiceProvider.GetRequiredService<IModelCreationService>();

            try
            {
                await job.BuildModelsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError("{serviceName} failed with exception: {message}", nameof(MachineLearningHostedService), ex.Message);
            }
        }
    }
}
