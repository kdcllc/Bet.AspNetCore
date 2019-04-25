using System;
using System.Diagnostics;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.ML;

namespace Bet.Extensions.ML.Prediction
{
    /// <summary>
    /// Creates instance of the <see cref="PredictionEngine{TSrc, TDst}"/> for the dataset.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TPrediction"></typeparam>
    public class ModelPredictionEnginePooledObjectPolicy<TData, TPrediction>
        : IPooledObjectPolicy<PredictionEngine<TData, TPrediction>>
        where TData : class
        where TPrediction : class, new()
    {
        private readonly MLContext _mlContext;

        private readonly ITransformer _model;

        private readonly ILogger _logger;

        private readonly ModelPredictionEngineOptions<TData, TPrediction> _options;

        public ModelPredictionEnginePooledObjectPolicy(
            MLContext mlContext,
            ITransformer model,
            ModelPredictionEngineOptions<TData, TPrediction> options,
            ILogger logger)
        {
            _mlContext = mlContext ?? throw new ArgumentNullException(nameof(mlContext));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public PredictionEngine<TData, TPrediction> Create()
        {
            var watch = Stopwatch.StartNew();

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<TData, TPrediction>(_model);

            watch.Stop();
            _logger.Log(_options.LogLevel,"Time took to create the prediction engine: {elapsed}", watch.ElapsedMilliseconds);

            return predictionEngine;
        }

        public bool Return(PredictionEngine<TData, TPrediction> obj)
        {
            return obj != null;
        }
    }
}
