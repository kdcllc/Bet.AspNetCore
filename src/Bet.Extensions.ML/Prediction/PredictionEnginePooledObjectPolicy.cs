using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.ML;
using System;
using System.Diagnostics;

namespace Bet.Extensions.ML.Prediction
{
    /// <summary>
    /// Creates instance of the <see cref="PredictionEngine{TSrc, TDst}"/> for the dataset.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TPrediction"></typeparam>
    public class PredictionEnginePooledObjectPolicy<TData, TPrediction>
        : IPooledObjectPolicy<PredictionEngine<TData, TPrediction>>
        where TData : class
        where TPrediction : class, new()
    {
        private readonly MLContext _mlContext;

        private readonly ITransformer _model;

        private readonly ILogger _logger;

        public PredictionEnginePooledObjectPolicy(
            MLContext mlContext,
            ITransformer model,
            ILogger logger)
        {
            _mlContext = mlContext ?? throw new ArgumentNullException(nameof(mlContext));
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _logger = logger;
        }

        public PredictionEngine<TData, TPrediction> Create()
        {
            var watch = Stopwatch.StartNew();

            var predictionEngine = _model.CreatePredictionEngine<TData, TPrediction>(_mlContext);

            watch.Stop();
            _logger.LogDebug("Time took to create the prediction engine: {elapsed}", watch.ElapsedMilliseconds);

            return predictionEngine;
        }

        public bool Return(PredictionEngine<TData, TPrediction> obj)
        {
            return obj != null;
        }
    }
}
