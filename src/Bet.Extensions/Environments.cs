using System.Collections.Generic;

namespace Bet.Extensions
{
    /// <summary>
    /// The custom dictionary to support matching between hosting environments and prefixes.
    /// </summary>
    public class Environments : Dictionary<string, string>
    {
        public Environments() : base(new Dictionary<string, string>
            {
                 { "Development", "dev" },
                 { "Staging", "qa" },
                 { "Production", "prod" }
            })
        {
        }
    }
}
