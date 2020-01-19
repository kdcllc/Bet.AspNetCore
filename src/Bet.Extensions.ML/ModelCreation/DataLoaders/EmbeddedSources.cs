using System;
using System.Collections.Generic;

namespace Bet.Extensions.ML.ModelCreation.DataLoaders
{
    /// <summary>
    /// The Embedded Sources for the ML.NET Datasets.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class EmbeddedSources<TInput> where TInput : class
    {
        /// <summary>
        /// The file name inside the embedded.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// The delimiter type.
        /// </summary>
        public string Delimiter { get; set; } = string.Empty;

        /// <summary>
        /// Specify if the dataset contains header record.
        /// </summary>
        public bool HasHeaderRecord { get; set; }

        /// <summary>
        /// This property gives the ability to override the existing loading mechanism.
        /// </summary>
        public Func<IEnumerable<TInput>>? Overrides { get; set; } = null;
    }
}
