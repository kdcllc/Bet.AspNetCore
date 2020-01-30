namespace Bet.Extensions.HealthChecks.ML
{
    public class MachineLearningHealthCheckOptions<TInput>
        where TInput : class, new()
    {
        public string ModelName { get; set; } = string.Empty;

        public TInput SampleData { get; set; } = new TInput();
    }
}
