using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Bet.Extensions.ML.Helpers
{
    public class StopwatchLogger
    {
        private readonly ILogger<StopwatchLogger> _logger;

        public StopwatchLogger(ILogger<StopwatchLogger> logger)
        {
            _logger = logger;
        }

        public Task<T> ExecuteAsync<T>(AsyncFunc<T> action)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                var sw = Stopwatch.StartNew();
                var result = action.Invoke();
                sw.Stop();

                return result;
            }

            return action.Invoke();
        }
    }
}
