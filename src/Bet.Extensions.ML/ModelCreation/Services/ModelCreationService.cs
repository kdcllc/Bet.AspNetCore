using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Bet.Extensions.ML.ModelCreation.Services
{
    public class ModelCreationService : IModelCreationService
    {
        private readonly ILogger<ModelCreationService> _logger;
        private readonly IEnumerable<IModelCreationEngine> _modelBuilders;

        public ModelCreationService(
            IEnumerable<IModelCreationEngine> modelBuilders,
            ILogger<ModelCreationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _modelBuilders = modelBuilders;
        }

        public async Task BuildModelsAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[Machine Learning][Started] Models Building");

            foreach (var modelBuilder in _modelBuilders)
            {
                try
                {
                    await modelBuilder.TrainModelAsync(cancellationToken);

                    await modelBuilder.ClassifyTestAsync(cancellationToken);

                    await modelBuilder.SaveModelAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("failed with exception: {message}", ex.ToString());
                }
            }

            _logger.LogInformation("[Machine Learning][Ended] Models Building");
        }
    }
}
