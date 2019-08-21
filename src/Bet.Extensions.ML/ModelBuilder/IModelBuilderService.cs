using System.Threading;
using System.Threading.Tasks;

namespace Bet.Extensions.ML.ModelBuilder
{
    /// <summary>
    /// ML model builder service provides with away to build/train/evaluate multiple ML models in the CI pipeline.
    /// </summary>
    public interface IModelBuilderService
    {
        /// <summary>
        /// Model Builder Service Name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Train model consist of:
        /// 1. Load dataset.
        /// 2. Build Training pipeline.
        /// 3. Train the ML Model.
        /// 4. Evaluate the Model against test dataset.
        /// </summary>
        /// <returns></returns>
        Task TrainModelAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Provides with ability to test against know datasets.
        /// </summary>
        /// <returns></returns>
        Task ClassifyTestAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Saves the ML model.
        /// </summary>
        /// <returns></returns>
        Task SaveModelAsync(CancellationToken cancellationToken);
    }
}
