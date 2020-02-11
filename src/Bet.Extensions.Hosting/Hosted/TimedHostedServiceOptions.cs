using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bet.Extensions.Hosting.Hosted
{
    public class TimedHostedServiceOptions
    {
        public TimedHostedServiceOptions()
        {
            FailMode = FailMode.LogAndRetry;

            TaskToExecuteAsync = (options, sp, token) => Task.CompletedTask;
        }

        public TimeSpan Interval { get; set; }

        public FailMode FailMode { get; set; }

        public TimeSpan RetryInterval { get; set; }

        public Func<TimedHostedServiceOptions, IServiceProvider, CancellationToken, Task> TaskToExecuteAsync { get; set; }
    }
}
