using System;
using System.Collections.Generic;

namespace Bet.Extensions.AzureAppConfiguration.Options
{
    public class FiltersOptions
    {
        public List<string> Sections { get; set; } = new List<string>();

        public List<string> RefresSections { get; set; } = new List<string>();

        /// <summary>
        /// The cache interval for the Options specified.
        /// The default is 1 sec.
        /// </summary>
        public TimeSpan CacheExpirationTime { get; set; } = TimeSpan.FromSeconds(1);
    }
}
