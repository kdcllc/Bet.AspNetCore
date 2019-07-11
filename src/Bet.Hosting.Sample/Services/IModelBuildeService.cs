using System.Threading.Tasks;

namespace Bet.Hosting.Sample.Services
{
    public interface IModelBuildeService
    {
        Task GenerateModel();

        void Classify(string text);
    }
}
