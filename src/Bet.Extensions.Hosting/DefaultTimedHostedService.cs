using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Bet.Extensions.Hosting.Abstractions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.Hosting
{
    internal class DefaultTimedHostedService : TimedHostedService
    {
        public DefaultTimedHostedService(
            Func<Task> task,
            IOptionsMonitor<TimedHostedServiceOptions> options,
            IEnumerable<ITimedHostedLifeCycleHook> lifeCycleHooks,
            ILogger<ITimedHostedService> logger) : base(options, lifeCycleHooks, logger)
        {
            TaskToExecuteTask = task;
        }
    }
}
