using System.Threading.Tasks;

namespace Bet.Hosting.Sample.Services
{
    public interface IModelBuilderService
    {
        Task TrainModel();

        void ClassifySample();

        void SaveModel();
    }
}
