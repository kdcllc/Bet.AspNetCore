using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.Hosting;
using Bet.Extensions.Hosting.Abstractions;
using Bet.Extensions.ML.ModelBuilder;
using Bet.Extensions.ML.Sentiment.Models;
using Bet.Extensions.ML.Spam.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Hosting.Sample.Services
{
    public class ModelBuilderHostedService : TimedHostedService
    {
        public ModelBuilderHostedService(
            IOptionsMonitor<TimedHostedServiceOptions> options,
            IEnumerable<ITimedHostedLifeCycleHook> lifeCycleHooks,
            ILogger<ITimedHostedService> logger) : base(options, lifeCycleHooks, logger)
        {
            TaskToExecuteAsync = (token) => RunModelGenertorsAsync(token);

        }

        public async Task RunModelGenertorsAsync(CancellationToken cancellationToken)
        {

        }
    }
}
