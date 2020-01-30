using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Bet.Extensions.Logging
{
    /// <summary>
    /// The async execution logger class.
    /// </summary>
    public sealed class ExecutionLogger
    {
        private readonly ILogger _logger;

        public ExecutionLogger(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes and logs timestamps for the task.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <param name="action">The async function to be executed.</param>
        /// <param name="methodName">The name of the method to execute.</param>
        /// <returns></returns>
        public async Task<T> ExecuteAndLogAsync<T>(Func<Task<T>> action, string methodName)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var sw = ValueStopwatch.StartNew();

                try
                {
                    var result = await action();
                    _logger.LogDebug("[Azure Table][{methodName}] Elapsed: {ElapsedMilliseconds}ms;", methodName, sw.GetElapsedTime());

                    return result;
                }
                catch (Exception ex)
                {
                    var exception = ex?.GetBaseException() ?? ex;

                    _logger.LogError(exception, "[Azure Table][{methodName}] Elapsed: {ElapsedMilliseconds} ms", methodName, sw.GetElapsedTime());
                    throw;
                }
            }

            return await action();
        }
    }
}
