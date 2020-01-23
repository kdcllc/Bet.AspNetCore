using System;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace Bet.Extensions.ML
{
    public class PostMLContextOptionsSetup : IPostConfigureOptions<MLContextOptions>
    {
        private ILogger<MLContext> _logger;

        public PostMLContextOptionsSetup(ILogger<MLContext> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void PostConfigure(string name, MLContextOptions options)
        {
            if (_logger.IsEnabled(options.LogLevel))
            {
                options.MLContext.Log += Log;
            }
        }

        private void Log(object sender, LoggingEventArgs e)
        {
            _logger.LogTrace(e.Message);
        }
    }
}
