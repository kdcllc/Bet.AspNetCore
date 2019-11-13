namespace Bet.Extensions.HealthChecks.PingCheck
{
    public class PingHealthCheckOptions
    {
        public string Host { get; set; } = string.Empty;

        public int Timeout { get; set; }
    }
}
