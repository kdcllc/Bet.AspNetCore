using System.Collections.Generic;

using Bet.Extensions.ML.Data;

using Microsoft.Extensions.Options;

namespace Bet.Extensions.ML.ModelCreation.DataLoaders
{
    public class EmbeddedSourceLoader<TInput> : ISourceLoader<TInput> where TInput : class
    {
        private readonly EmbeddedSourceLoaderOptions<TInput> _options;

        public EmbeddedSourceLoader(IOptions<EmbeddedSourceLoaderOptions<TInput>> options)
        {
            _options = options.Value;
        }

        public IEnumerable<TInput> LoadData()
        {
            var list = new List<TInput>();

            foreach (var source in _options.EmbeddedSourcesList)
            {
                if (source.Overrides != null)
                {
                    list.AddRange(source.Overrides());
                }
                else
                {
                    var records = LoadFromEmbededResource.GetRecords<TInput>(source.FileName, source.Delimiter, source.HasHeaderRecord);
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
