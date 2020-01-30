using System.Threading;
using System.Threading.Tasks;

namespace Bet.Extensions.ML.ModelCreation.Services
{
    public interface IModelCreationService
    {
        Task BuildModelsAsync(CancellationToken cancellationToken);
    }
}
