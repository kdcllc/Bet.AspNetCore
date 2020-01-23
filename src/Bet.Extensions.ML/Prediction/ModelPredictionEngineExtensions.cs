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
    }
}
