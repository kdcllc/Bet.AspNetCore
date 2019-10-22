namespace Bet.Extensions.HealthChecks.PingCheck
{
    public class PingHealthCheckOptions
    {
        public string Host { get; set; }

        public int Timeout { get; set; }
    }
}
