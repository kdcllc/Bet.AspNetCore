using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.Prediction;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bet.Extensions.HealthChecks.ML
{
    public class MachineLearningHealthCheck<TInput, TPrediction> : IHealthCheck
        where TInput : class, new()
        where TPrediction : class, new()
    {
        private readonly IModelPredictionEngine<TInput, TPrediction> _model;
        private readonly ILogger<MachineLearningHealthCheck<TInput, TPrediction>> _logger;
        private readonly IOptionsMonitor<MachineLearningHealthCheckOptions<TInput>> _optionsMonitor;

        public MachineLearningHealthCheck(
            IModelPredictionEngine<TInput, TPrediction> model,
            IOptionsMonitor<MachineLearningHealthCheckOptions<TInput>> optionsMonitor,
            ILogger<MachineLearningHealthCheck<TInput, TPrediction>> logger)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var name = context.Registration.Name;

            try
            {
                _logger.LogInformation("[HealthCheck][{healthCheckName}]", name);

                var options = _optionsMonitor.Get(name);

                var result = await Task.FromResult(_model.Predict(options.ModelName, options.SampleData));

                var json = JsonConvert.SerializeObject(result);

                var data = new Dictionary<string, object>
                {
                    { "result", json }
                };

                return new HealthCheckResult(HealthStatus.Healthy, name, data: data);
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
    }
}
