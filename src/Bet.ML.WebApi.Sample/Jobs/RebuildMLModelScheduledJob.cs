using Bet.Extensions.ML.ModelBuilder;
using CronScheduler.AspNetCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bet.ML.WebApi.Sample.Jobs
{
    public class RebuildMLModelScheduledJob : ScheduledJob
    {
        private readonly IEnumerable<IModelBuilderService> _modelBuilders;
        private readonly ILogger<RebuildMLModelScheduledJob> _logger;
        private readonly RebuildMLModelsOptions _options;

        public RebuildMLModelScheduledJob(
            IEnumerable<IModelBuilderService> modelBuilders,
            IOptionsMonitor<RebuildMLModelsOptions> options,
            ILogger<RebuildMLModelScheduledJob> logger) : base(options.CurrentValue)
        {
            _modelBuilders = modelBuilders ?? throw new ArgumentNullException(nameof(modelBuilders));
            _options = options.CurrentValue;

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // 1. Build models
            _logger.LogInformation("[Started][{startupJobName}] executing model builders total count: {numberOfModels}", nameof(RebuildMLModelScheduledJob), _modelBuilders?.ToList()?.Count ?? 0);

            var actualCount = 0;
            foreach (var modelBuilder in _modelBuilders)
            {
                try
                {
                    await modelBuilder.TrainModelAsync(cancellationToken);

                    await modelBuilder.ClassifyTestAsync(cancellationToken);

                    await modelBuilder.SaveModelAsync(cancellationToken);

                    if (actualCount > 1)
                    {
                        break;
                    }

                    actualCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError("{modelBuilder} failed with exception: {message}", modelBuilder.GetType(), ex.Message);
                }
            }

            _logger.LogInformation("[Finished][{startupJobName}] executing total number successfully {numberOfModels}", nameof(RebuildMLModelScheduledJob), actualCount);
        }
    }
}
