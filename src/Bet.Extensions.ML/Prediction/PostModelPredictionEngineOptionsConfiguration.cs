using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace Bet.Extensions.ML.Prediction
{
    public class PostModelPredictionEngineOptionsConfiguration : IPostConfigureOptions<ModelPredictionEngineOptions>
    {
        private readonly ILogger<MLContext> _logger;
        private ModelPredictionEngineOptions _options;

        public PostModelPredictionEngineOptionsConfiguration(ILogger<MLContext> logger)
        {
            _logger = logger;
        }

        public void PostConfigure(string name, ModelPredictionEngineOptions options)
        {
            _options = options;

            if (_logger.IsEnabled(options.LogLevel))
            {
                options.MLContext().Log += Log;
            }
        }

        private void Log(object sender, LoggingEventArgs e)
        {
            _logger.Log(_options.LogLevel,e.Message);
        }
    }
}
