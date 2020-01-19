using System.Collections.Generic;

namespace Bet.Extensions.ML.ModelCreation.DataLoaders
{
    /// <summary>
    /// The ML.NET dataset sources loaders.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public interface ISourceLoader<TInput> where TInput : class
    {
        /// <summary>
        /// Loading the dataset based on the specified source.
        /// </summary>
        /// <returns></returns>
        IEnumerable<TInput> LoadData();
    }
}
