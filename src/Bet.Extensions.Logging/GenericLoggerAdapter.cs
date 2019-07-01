using System;

using Microsoft.Extensions.Logging;

namespace Bet.Extensions.Logging
{
    public class GenericLoggerAdapter<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public GenericLoggerAdapter(ILogger logger)
        {
            _logger = logger;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
