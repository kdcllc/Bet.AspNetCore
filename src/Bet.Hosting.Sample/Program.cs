using Bet.Extensions.ML.Data;
using Bet.Hosting.Sample.ML.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bet.Hosting.Sample
{
    class Program
    {
        private static string ModelPath = GetAbsolutePath("SpamModel.zip");

        public static void Main(string[] args)
        {
            Console.WriteLine("Building model");

            BuildModel();
        }

        private static void BuildModel()
        {
            var mlContext = new MLContext();

            var records = LoadFromEmbededResource.GetRecords<SpamInput>("Content.SpamDetectionData.csv", delimiter: ",");

            var smsRecords = LoadFromEmbededResource.GetRecords<SpamInput>("Content.SMSSpamCollection.txt", delimiter: "\t", hasHeaderRecord: false);

            records.AddRange(smsRecords);

            var data = mlContext.Data.LoadFromEnumerable(records);

            var testTrainSplit = mlContext.BinaryClassification.TrainTestSplit(data, testFraction: 0.2);

            var dataProcessPipeline =
                    mlContext.Transforms.CustomMapping<LabelInput, LabelOutput>(LabelTransfomer.Transform, nameof(LabelTransfomer.Transform))
                .Append(mlContext
                    .Transforms
                    .Text
                    .FeaturizeText(
                        outputColumnName: DefaultColumnNames.Features,
                        inputColumnName: nameof(SpamInput.Message)))
                //.Append(mlContext.Transforms.Normalize(new NormalizingEstimator.LogMeanVarColumnOptions("LogMeanVarNormalized", DefaultColumnNames.Features,useCdf:true)))
                .Append(mlContext.Transforms.Normalize(new NormalizingEstimator.MinMaxColumnOptions("LogMeanVarNormalized", DefaultColumnNames.Features,fixZero:false)))
                .AppendCacheCheckpoint(mlContext);

            //var trainer = mlContext.BinaryClassification.Trainers.FastTree(
            //    labelColumnName: DefaultColumnNames.Label,
            //    featureColumnName: DefaultColumnNames.Features);

            var options = new SdcaBinaryTrainer.Options
            {
                //L1Threshold = 0.05f,
                LabelColumn = DefaultColumnNames.Label,
                FeatureColumn = "LogMeanVarNormalized"
            };

            var trainer = mlContext.BinaryClassification.Trainers.StochasticDualCoordinateAscent(options);

            var trainingPipeline = dataProcessPipeline.Append(trainer);

            //Console.WriteLine("=============== Cross-validating to get model's accuracy metrics ===============");
            //var crossValidationResults = mlContext.BinaryClassification.CrossValidate(data: data, estimator: trainingPipeline, numFolds: 5);
            //var aucs = crossValidationResults.Select(r => r.Metrics.Auc);
            //Console.WriteLine("The AUC is {0}", aucs.Average());

            Console.WriteLine("=============== Training the model ===============");

            var model = trainingPipeline.Fit(testTrainSplit.TrainSet);

            var predictions = model.Transform(testTrainSplit.TestSet);

            Console.WriteLine("=============== Validating to get model's accuracy metrics ===============");

            var calMetrics = mlContext.BinaryClassification.Evaluate(data: predictions, label: DefaultColumnNames.Label, score: DefaultColumnNames.Score);

            var predEngine = model.CreatePredictionEngine<SpamInput, SpamPrediction>(mlContext);

            Console.WriteLine("=============== Predictions for below data===============");
            // Test a few examples
            ClassifyMessage(predEngine, "That's a great idea. It should work.");
            ClassifyMessage(predEngine, "free medicine winner! congratulations");
            ClassifyMessage(predEngine, "Yes we should meet over the weekend!");
            ClassifyMessage(predEngine, "you win pills and free entry vouchers");

            ClassifyMessage(predEngine, "Albions wight seraphs soul yes was uses full all fountain losel perchance blast at crime concubines another.");
            ClassifyMessage(predEngine, "Albions you win pills and free entry vouchers at crime concubines another.");

            Console.WriteLine("=============== End of process, hit any key to finish =============== ");

            Console.WriteLine($"Accuracy:{calMetrics.Accuracy}-Auc:{calMetrics.Auc}");

            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                mlContext.Model.Save(model, fs);
            }
        }

        public static string GetAbsolutePath(string relativePath)
        {
            var _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            var assemblyFolderPath = _dataRoot.Directory.FullName;

            return Path.Combine(assemblyFolderPath, relativePath);
        }

        public static void ClassifyMessage(PredictionEngine<SpamInput, SpamPrediction> predictor, string message)
        {
            var input = new SpamInput { Message = message };
            var prediction = predictor.Predict(input);
            Console.WriteLine("The message '{0}' is {1}", input.Message, prediction.IsSpam ? "spam" : "not spam");
        }
    }
}

