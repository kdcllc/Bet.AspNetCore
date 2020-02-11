using CronScheduler.Extensions.Scheduler;

namespace Bet.AspNetCore.LetsEncrypt.Internal
{
    internal class AcmeRenewalJobOptions : SchedulerOptions
    {
        public string NamedOptions { get; set; } = string.Empty;
    }
}
