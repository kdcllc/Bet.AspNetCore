using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Bet.Extensions.ML.ModelCreation.Services
{
    public class MachineLearningService : IMachineLearningService
    {
        private readonly ILogger<MachineLearningService> _logger;
        private readonly IEnumerable<IModelCreationEngine> _modelBuilders;

        public MachineLearningService(
            IEnumerable<IModelCreationEngine> modelBuilders,
            ILogger<MachineLearningService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modelBuilders = modelBuilders;
        }

        public async Task BuildModelsAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[Started Model Building]");

            foreach (var modelBuilder in _modelBuilders)
            {
                try
                {
                    _logger.LogInformation("[Started Model Building][{modelName}]", modelBuilder.Name);

                    await modelBuilder.TrainModelAsync(cancellationToken);

                    await modelBuilder.ClassifyTestAsync(cancellationToken);

                    await modelBuilder.SaveModelAsync(cancellationToken);

                    _logger.LogInformation("[Ended Model Building][{modelName}]", modelBuilder.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError("{modelBuilder} failed with exception: {message}", modelBuilder.Name, ex.ToString());
                }
            }

            _logger.LogInformation("[Ended Model Building]");
        }
    }
}
