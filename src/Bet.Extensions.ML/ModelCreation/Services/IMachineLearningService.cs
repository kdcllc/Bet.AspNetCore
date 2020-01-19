using System.Threading;
using System.Threading.Tasks;

namespace Bet.Extensions.ML.ModelCreation.Services
{
    public interface IMachineLearningService
    {
        Task BuildModelsAsync(CancellationToken cancellationToken);
    }
}
