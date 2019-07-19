using System;
using System.Threading;
using System.Threading.Tasks;

using CronScheduler.AspNetCore;

namespace Bet.ML.WebApi.Sample.Jobs
{
    public class LoadMLModelsJobs : IStartupJob
    {
        public LoadMLModelsJobs()
        {

        }

        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
