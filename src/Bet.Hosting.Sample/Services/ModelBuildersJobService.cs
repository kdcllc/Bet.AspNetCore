using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.ModelBuilder;

using Microsoft.Extensions.Logging;

namespace Bet.Hosting.Sample.Services
{
    public class ModelBuildersJobService : IModelBuildersJobService
    {
        private readonly IEnumerable<IModelBuilderService> _modelBuilders;
        private readonly ILogger<ModelBuildersJobService> _logger;

        public ModelBuildersJobService(IEnumerable<IModelBuilderService> modelBuilders, ILogger<ModelBuildersJobService> logger)
        {
            _modelBuilders = modelBuilders ?? throw new ArgumentNullException(nameof(modelBuilders));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            foreach (var modelBuilder in _modelBuilders)
            {
                try
                {
                    _logger.LogInformation("[Start Model Building][{modelName}]", modelBuilder.Name);

                    await modelBuilder.TrainModelAsync(cancellationToken);

                    await modelBuilder.ClassifyTestAsync(cancellationToken);

                    await modelBuilder.SaveModelAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("{modelBuilder} failed with exception: {message}", modelBuilder.GetType(), ex.Message);
                }
            }
        }
    }
}
