using System;
using System.IO;
using Bet.Extensions.ML.Helpers;
using Bet.Extensions.ML.Sentiment.Models;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.ML;

namespace Bet.Hosting.Sample
{
    /// <summary>
    /// https://github.com/dotnet/machinelearning/blob/master/docs/code/MlNetCookBook.md
    /// </summary>
    class Program
    {
        private static string SpamModelPath = GetAbsolutePath("SpamModel.zip");

        private static string SentimentModelPath = GetAbsolutePath("SentimentModel.zip");

        public static void Main(string[] args)
        {
            Console.WriteLine("Start Creating models");

            BuildSentimentDetectionModel();
            // BuildSpamDetectionModel();

            Console.WriteLine("Building models");

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        private static void BuildSentimentDetectionModel()
        {
            var builder = new Extensions.ML.Sentiment.ModelBuilder<SentimentIssue, SentimentPrediction, BinaryClassificationMetrics>();
            builder.LoadData();

            var result = builder.Train();

            Console.WriteLine(result.ToString());

            var predictor = builder.MlContext.Model.CreatePredictionEngine<SentimentIssue, SentimentPrediction>(builder.Model);
            Console.WriteLine("=============== Predictions for below data===============");

            ClassifySentimentText(predictor, "This is a very rude movie");
            ClassifySentimentText(predictor, "Hate All Of You're Work");

            Console.WriteLine("=================== Saving Model to Disk ============================ ");

            using (var fs = new FileStream(SentimentModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                builder.MlContext.Model.Save(builder.Model, builder.TrainingSchema, fs);
            }

            Console.WriteLine("======================= Creating Model Completed ================== ");
        }

        private static void BuildSpamDetectionModel()
        {
            var mlContext = new MLContext();

            var builder = new Extensions.ML.Spam.ModelBuilder<SpamInput,SpamPrediction, MulticlassClassificationFoldsAverageMetrics>(mlContext);

            // loads based dataset.
            builder.LoadData();

            var result = builder.Train();

            Console.WriteLine(result.ToString());

            var predictor = mlContext.Model.CreatePredictionEngine<SpamInput, SpamPrediction>(builder.Model);

            Console.WriteLine("=============== Predictions for below data===============");
            // Test a few examples
            ClassifySpamMessage(predictor, "That's a great idea. It should work.");
            ClassifySpamMessage(predictor, "free medicine winner! congratulations");
            ClassifySpamMessage(predictor, "Yes we should meet over the weekend!");
            ClassifySpamMessage(predictor, "you win pills and free entry vouchers");


            Console.WriteLine("=================== Saving Model to Disk ============================ ");

            using (var fs = new FileStream(SpamModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                mlContext.Model.Save(builder.Model, builder.TrainingSchema, fs);
            }

            Console.WriteLine("======================= Creating Model Completed ================== ");
        }

        public static string GetAbsolutePath(string relativePath)
        {
            var _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            var assemblyFolderPath = _dataRoot.Directory.FullName;

            return Path.Combine(assemblyFolderPath, relativePath);
        }

        public static void ClassifySpamMessage(PredictionEngine<SpamInput, SpamPrediction> predictor, string message)
        {
            var input = new SpamInput { Message = message };
            var prediction = predictor.Predict(input);
            Console.WriteLine("The message '{0}' is {1}", input.Message, prediction.IsSpam == "spam" ? "spam" : "not spam");
        }

        public static void ClassifySentimentText(PredictionEngine<SentimentIssue, SentimentPrediction> predictor, string text)
        {
            var input = new SentimentIssue { Text = text };
            var prediction = predictor.Predict(input);
            Console.WriteLine("The text '{0}' is {1} Probability of being toxic: {2}", input.Text, Convert.ToBoolean(prediction.Prediction) ? "Toxic" : "Non Toxic", prediction.Probability);
        }
    }
}

