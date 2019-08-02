using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.ModelBuilder;

using CronScheduler.AspNetCore;

using Microsoft.Extensions.Logging;

namespace Bet.ML.WebApi.Sample.Jobs
{
    public class LoadMLModelsJobs : IStartupJob
    {
        private readonly IEnumerable<IModelBuilderService> _modelBuilders;
        private readonly ILogger<LoadMLModelsJobs> _logger;

        public LoadMLModelsJobs(
            IEnumerable<IModelBuilderService> modelBuilders,
            ILogger<LoadMLModelsJobs> logger)
        {
            _modelBuilders = modelBuilders ?? throw new ArgumentNullException(nameof(modelBuilders));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            // 1. Build models
            _logger.LogInformation("[Started][{startupJobName}] executing model builders total count: {numberOfModels}", nameof(LoadMLModelsJobs), _modelBuilders?.ToList()?.Count ?? 0);

            var actualCount = 0;
            foreach (var modelBuilder in _modelBuilders)
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

            _logger.LogInformation("[Finished][{startupJobName}] executing total number successfully {numberOfModels}", nameof(LoadMLModelsJobs), actualCount);
        }
    }
}
