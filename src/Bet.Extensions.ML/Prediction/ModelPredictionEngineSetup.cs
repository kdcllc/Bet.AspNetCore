using System;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace Bet.Extensions.ML.Prediction
{
    public class ModelPredictionEngineSetup<TData, TPrediction>
        : IPostConfigureOptions<ModelPredictionEngineOptions<TData, TPrediction>>,
        IConfigureNamedOptions<ModelPredictionEngineOptions<TData, TPrediction>>
        where TData : class
        where TPrediction : class, new()
    {
        private readonly ILogger<MLContext> _logger;
        private readonly IServiceProvider _serviceProvider;

        private ModelPredictionEngineOptions<TData, TPrediction> _options;

        public ModelPredictionEngineSetup(
            IServiceProvider serviceProvider,
            ILogger<MLContext> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public void Configure(string name, ModelPredictionEngineOptions<TData, TPrediction> options)
        {
            options.ModelName = name;
            options.ServiceProvider = _serviceProvider;
        }

        public void Configure(ModelPredictionEngineOptions<TData, TPrediction> options)
        {
            Configure(options.ModelName, options);
        }

        public void PostConfigure(string name, ModelPredictionEngineOptions<TData, TPrediction> options)
        {
            _options = options;

            if (_logger.IsEnabled(options.LogLevel))
            {
                options.MLContext().Log += Log;
            }
        }

        private void Log(object sender, LoggingEventArgs e)
        {
            _logger.Log(_options.LogLevel, e.Message);
        }
    }
}
