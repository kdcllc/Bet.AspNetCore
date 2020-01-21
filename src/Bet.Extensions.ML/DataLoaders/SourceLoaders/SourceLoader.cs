using System;
using System.Collections.Generic;

namespace Bet.Extensions.ML.DataLoaders.SourceLoaders
{
    /// <summary>
    /// The ML.NET dataset sources loaders.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public abstract class SourceLoader<TInput> where TInput : class
    {
        public abstract Func<SourceLoaderFile<TInput>, List<TInput>> ProcessFile { get; set; }

        protected SourceLoaderFileOptions<TInput> Options { get; private set; } = new SourceLoaderFileOptions<TInput>();

        public void Setup(SourceLoaderFileOptions<TInput> options)
        {
            Options = options;
        }

        /// <summary>
        /// Loading the dataset based on the specified source.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<TInput> LoadData()
        {
            var list = new List<TInput>();

            foreach (var source in Options.Sources)
            {
                if (source.CustomAction != null)
                {
                    list.AddRange(source.CustomAction());
                }
                else
                {
                    var records = ProcessFile(source);
                    if (records != null)
                    {
                        list.AddRange(records);
                    }
                }
            }

            return list;
        }
    }
}
