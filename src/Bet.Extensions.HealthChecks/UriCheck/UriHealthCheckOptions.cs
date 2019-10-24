using System.Collections.Generic;

namespace Bet.Extensions.HealthChecks.UriCheck
{
    public class UriHealthCheckOptions
    {
        public ICollection<UriOptionsSetup> UriOptions { get; } = new List<UriOptionsSetup>();
    }
}
