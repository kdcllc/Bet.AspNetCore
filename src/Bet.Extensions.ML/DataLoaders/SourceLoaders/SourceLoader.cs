using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.ML.DataLoaders.SourceLoaders
{
    /// <summary>
    /// The ML.NET dataset sources loaders.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public abstract class SourceLoader<TInput> where TInput : class
    {
        public abstract Func<SourceLoaderFile<TInput>, IServiceProvider, string, List<TInput>> ProcessFile { get; set; }

        protected SourceLoaderFileOptions<TInput> Options { get; private set; } = new SourceLoaderFileOptions<TInput>();

        protected IServiceProvider Serviceprovider { get; private set; } = default!;

        protected string ModelName { get; private set; } = string.Empty;

        public void Setup(IServiceProvider provider, string modelName)
        {
            Serviceprovider = provider;

            ModelName = modelName;

            Options = provider.GetRequiredService<IOptionsMonitor<SourceLoaderFileOptions<TInput>>>().Get(modelName);
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
                    var records = ProcessFile(source, Serviceprovider, ModelName);
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
