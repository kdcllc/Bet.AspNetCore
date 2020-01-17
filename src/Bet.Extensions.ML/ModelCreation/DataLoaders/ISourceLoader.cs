using System.Collections.Generic;

namespace Bet.Extensions.ML.ModelCreation.DataLoaders
{
    public interface ISourceLoader<TInput> where TInput : class
    {
        IEnumerable<TInput> LoadData();
    }
}
