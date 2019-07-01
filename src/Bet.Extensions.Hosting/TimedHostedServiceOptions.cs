using System;

using Bet.Extensions.Hosting.Abstractions;

namespace Bet.Extensions.Hosting
{
    public class TimedHostedServiceOptions
    {
        public TimeSpan Interval { get; set; }

        public FailMode FailMode { get; set; } = FailMode.LogAndRetry;

        public TimeSpan RetryInterval { get; set; }
    }
}
