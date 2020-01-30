using System.Threading;
using System.Threading.Tasks;

namespace Bet.Extensions.ML.ModelCreation
{
    /// <summary>
    /// ML model builder service provides with away to build/train/evaluate multiple ML models in the CI pipeline.
    /// </summary>
    public interface IModelCreationEngine
    {
        /// <summary>
        /// Train model consist of:
        /// 1. Load dataset.
        /// 2. Build Training pipeline.
        /// 3. Train the ML Model.
        /// 4. Evaluate the Model against test dataset.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task SaveModelAsync(CancellationToken cancellationToken);
    }
}
