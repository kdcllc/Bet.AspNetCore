using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.ModelBuilder;

using CronScheduler.Extensions.Scheduler;
using CronScheduler.Extensions.StartupInitializer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.ML.WebApi.Sample.Jobs
{
    public class RebuildMLModelScheduledJob : ScheduledJob, IStartupJob
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<RebuildMLModelScheduledJob> _logger;
        private readonly RebuildMLModelsOptions _options;

        public RebuildMLModelScheduledJob(
            IServiceProvider provider,
            IOptionsMonitor<RebuildMLModelsOptions> options,
            ILogger<RebuildMLModelScheduledJob> logger) : base(options.CurrentValue)
        {
            _options = options.CurrentValue;
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using (var scope = _provider.CreateScope())
            {
                var modelBuilders = scope.ServiceProvider.GetRequiredService<IEnumerable<IModelBuilderService>>();

                // 1. Build models
                _logger.LogInformation("[Started][{jobName}] executing model builders total count: {numberOfModels}", nameof(RebuildMLModelScheduledJob), modelBuilders?.ToList()?.Count ?? 0);

                var actualCount = 0;
                foreach (var modelBuilder in modelBuilders)
                {
                    try
                    {
                        await modelBuilder.TrainModelAsync(cancellationToken);

                        await modelBuilder.ClassifyTestAsync(cancellationToken);

                        await modelBuilder.SaveModelAsync(cancellationToken);

                        actualCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("{modelBuilder} failed with exception: {message}", modelBuilder.GetType(), ex.Message);
                    }
                }

                _logger.LogInformation("[Finished][{jobName}] executing total number successfully {numberOfModels}", nameof(RebuildMLModelScheduledJob), actualCount);
            }
        }
    }
}
