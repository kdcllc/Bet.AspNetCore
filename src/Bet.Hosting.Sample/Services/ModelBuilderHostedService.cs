using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.Hosting;
using Bet.Extensions.Hosting.Abstractions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Hosting.Sample.Services
{
    public class ModelBuilderHostedService : TimedHostedService
    {
        private readonly IEnumerable<IModelBuilderService> _modelBuilders;

        public ModelBuilderHostedService(
            IEnumerable<IModelBuilderService> modelBuilders,
            IOptionsMonitor<TimedHostedServiceOptions> options,
            IEnumerable<ITimedHostedLifeCycleHook> lifeCycleHooks,
            ILogger<ITimedHostedService> logger) : base(options, lifeCycleHooks, logger)
        {
            TaskToExecuteAsync = (token) => RunModelGenertorsAsync(token);
            _modelBuilders = modelBuilders ?? throw new ArgumentNullException(nameof(modelBuilders));
        }

        public async Task RunModelGenertorsAsync(CancellationToken cancellationToken)
        {
            foreach (var modelBuilder in _modelBuilders)
            {
                await modelBuilder.TrainModel();

                modelBuilder.ClassifySample();

                modelBuilder.SaveModel();
            }
        }
    }
}
