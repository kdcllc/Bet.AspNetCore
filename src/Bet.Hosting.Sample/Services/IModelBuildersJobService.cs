using System.Threading;
using System.Threading.Tasks;

namespace Bet.Hosting.Sample.Services
{
    public interface IModelBuildersJobService
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}