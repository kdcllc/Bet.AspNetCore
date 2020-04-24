using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.ModelCreation;

using CronScheduler.Extensions.Scheduler;
using CronScheduler.Extensions.StartupInitializer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.Sample
{
    public class ModelBuilderJob : IScheduledJob, IStartupJob
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<ModelBuilderJob> _logger;
        private ModelBuilderOptions _options;

        public ModelBuilderJob(
            IServiceProvider provider,
            IOptionsMonitor<ModelBuilderOptions> optionsMonitor,
            ILogger<ModelBuilderJob> logger)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _options = optionsMonitor.Get(Name);

            optionsMonitor.OnChange((o, n) =>
            {
                if (n == Name)
                {
                    _options = o;
                }
            });
        }

        public string Name => nameof(ModelBuilderJob);

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using var scope = _provider.CreateScope();
            var modelBuilders = scope.ServiceProvider.GetRequiredService<IEnumerable<IModelCreationEngine>>();

            // 1. Build models
            _logger.LogInformation("[Started][{jobName}] executing model builders total count: {numberOfModels}", nameof(ModelBuilderJob), modelBuilders?.ToList()?.Count ?? 0);

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
                    _logger.LogError("{modelBuilder} failed with exception: {message}", modelBuilder.GetType(), ex.ToString());
                }
            }

            _logger.LogInformation("[Finished][{jobName}] executing total number successfully {numberOfModels}", nameof(ModelBuilderJob), actualCount);
        }
    }
}
