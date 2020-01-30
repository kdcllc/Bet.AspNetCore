using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.ObjectPool;
using Microsoft.ML;

namespace Bet.Extensions.ML.Prediction
{
    /// <summary>
    /// Creates instance of the <see cref="PredictionEngine{TSrc, TDst}"/> for the dataset.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <typeparam name="TPrediction"></typeparam>
    public class ModelPredictionEnginePoolPolicy<TData, TPrediction>
        : IPooledObjectPolicy<PredictionEngine<TData, TPrediction>>
        where TData : class
        where TPrediction : class, new()
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;
        private readonly List<WeakReference> _references = new List<WeakReference>();

        public ModelPredictionEnginePoolPolicy(
            MLContext mlContext,
            ITransformer model)
        {
            _mlContext = mlContext ?? throw new ArgumentNullException(nameof(mlContext));
            _model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public PredictionEngine<TData, TPrediction> Create()
        {
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<TData, TPrediction>(_model);

            _references.Add(new WeakReference(predictionEngine));
            return predictionEngine;
        }

        public bool Return(PredictionEngine<TData, TPrediction> obj)
        {
            return _references.Any(x => x.Target == obj);
        }
    }
}
