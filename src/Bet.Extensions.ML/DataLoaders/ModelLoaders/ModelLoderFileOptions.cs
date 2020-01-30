using System;

namespace Bet.Extensions.ML.DataLoaders.ModelLoaders
{
    public class ModelLoderFileOptions
    {
        /// <summary>
        /// The name of the model to be build.
        /// </summary>
        public string ModelName { get; set; } = string.Empty;

        public string ModelFileName { get; set; } = string.Empty;

        public string ModelResultFileName { get; set; } = string.Empty;

        public bool WatchForChanges { get; set; } = false;

        public TimeSpan? ReloadInterval { get; set; }
    }
}
