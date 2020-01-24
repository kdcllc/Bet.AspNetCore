using System.Collections.Generic;
using System.Linq;
using Microsoft.ML;

namespace Bet.Extensions.ML.Prediction
{
    public static class ModelPredictionEngineExtensions
    {
        public static TPrediction Predict<TInput, TPrediction>(
            this IModelPredictionEngine<TInput, TPrediction> modelPredictionEngine,
            TInput dataSample)
                where TInput : class
                where TPrediction : class, new()
        {
            var engine = modelPredictionEngine.GetPredictionEngine();

            try
            {
                return engine.Predict(dataSample);
            }
            finally
            {
                modelPredictionEngine.ReturnPredictionEngine(engine);
            }
        }

        public static TPrediction Predict<TInput, TPrediction>(
            this IModelPredictionEngine<TInput, TPrediction> modelPredictionEngine,
            string modelName,
            TInput dataSample)
                where TInput : class
                where TPrediction : class, new()
        {
            var engine = modelPredictionEngine.GetPredictionEngine(modelName);

            try
            {
                return engine.Predict(dataSample);
            }
            finally
            {
                modelPredictionEngine.ReturnPredictionEngine(modelName, engine);
            }
        }

        public static IEnumerable<TPrediction> Predict<TInput, TPrediction>(
           this IModelPredictionEngine<TInput, TPrediction> modelPredictionEngine,
           IEnumerable<TInput> dataSamples)
               where TInput : class
               where TPrediction : class, new()
        {
            var model = modelPredictionEngine.GetModel();

            return modelPredictionEngine.GetBatch(model, dataSamples);
        }

        public static IEnumerable<TPrediction> Predict<TInput, TPrediction>(
            this IModelPredictionEngine<TInput, TPrediction> modelPredictionEngine,
            string modelName,
            IEnumerable<TInput> dataSamples)
                where TInput : class
                where TPrediction : class, new()
        {
            var model = modelPredictionEngine.GetModel(modelName);

            return modelPredictionEngine.GetBatch(model, dataSamples);
        }

        private static IEnumerable<TPrediction> GetBatch<TInput, TPrediction>(
            this IModelPredictionEngine<TInput, TPrediction> modelPredictionEngine,
            ITransformer model,
            IEnumerable<TInput> dataSamples)
                where TInput : class
                where TPrediction : class, new()
        {
            var data = modelPredictionEngine.MLContext.Data.LoadFromEnumerable(dataSamples);

            var dataView = model.Transform(data);

            return modelPredictionEngine.MLContext.Data
                .CreateEnumerable<TPrediction>(dataView, reuseRowObject: false).ToList();
        }
    }
}
