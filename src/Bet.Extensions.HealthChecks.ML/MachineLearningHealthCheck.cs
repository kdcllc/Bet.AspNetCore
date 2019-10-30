using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.ML.Prediction;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Bet.Extensions.HealthChecks.ML
{
    public class MachineLearningHealthCheck<TInput, TPrediction> : IHealthCheck where TInput : class, new() where TPrediction : class, new()
    {
        private readonly IModelPredictionEngine<TInput, TPrediction> _model;
        private readonly ILogger<MachineLearningHealthCheck<TInput, TPrediction>> _logger;

        public MachineLearningHealthCheck(
            IModelPredictionEngine<TInput, TPrediction> model,
            ILogger<MachineLearningHealthCheck<TInput, TPrediction>> logger)
        {
            _model = model;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var name = context.Registration.Name;

            try
            {
                _logger.LogInformation("[HealthCheck][{healthCheckName}]", name);

                var result = await Task.FromResult(_model.Predict(new TInput()));

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
