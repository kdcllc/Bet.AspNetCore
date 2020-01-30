using Microsoft.Extensions.Logging;
using Microsoft.ML;

namespace Bet.Extensions.ML
{
    public class MLContextOptions
    {
        public MLContext MLContext { get; set; } = new MLContext();

        public LogLevel LogLevel { get; set; } = LogLevel.Trace;
    }
}
