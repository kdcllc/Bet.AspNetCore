using Bet.Extensions.ML.Data;
using Bet.Hosting.Sample.ML.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bet.Hosting.Sample
{
    class Program
    {
        static void Main(string[] args)
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
                mlContext.Transforms.CustomMapping<LabelInput, LabelOutput>(mapAction: LabelTransfomer.Transform, contractName: nameof(LabelTransfomer))
                .Append(mlContext
                    .Transforms
                    .Text
                    .FeaturizeText(
                        outputColumnName: DefaultColumnNames.Features,
                        inputColumnName: nameof(SpamInput.Message)))
                .AppendCacheCheckpoint(mlContext);

            //var trainer = mlContext.BinaryClassification.Trainers.FastTree(
            //    labelColumnName: DefaultColumnNames.Label,
            //    featureColumnName: DefaultColumnNames.Features);

            var options = new SdcaBinaryTrainer.Options
            {
                L1Threshold = 0.05f,
                LabelColumn = DefaultColumnNames.Label,
                FeatureColumn = DefaultColumnNames.Features
            };

            var trainer = mlContext.BinaryClassification.Trainers.StochasticDualCoordinateAscent(options);


            var trainingPipeline = dataProcessPipeline.Append(trainer);

            Console.WriteLine("=============== Cross-validating to get model's accuracy metrics ===============");
            var crossValidationResults = mlContext.BinaryClassification.CrossValidate(data: data, estimator: trainingPipeline, numFolds: 5);
            var aucs = crossValidationResults.Select(r => r.Metrics.Auc);
            Console.WriteLine("The AUC is {0}", aucs.Average());

            var model = trainingPipeline.Fit(testTrainSplit.TrainSet);


            var predictions = model.Transform(testTrainSplit.TestSet);
            var calMetrics = mlContext.BinaryClassification.Evaluate(data: predictions, label: DefaultColumnNames.Label, score: DefaultColumnNames.Score);

            var predEngine = model.CreatePredictionEngine<SpamInput, SpamPrediction>(mlContext);
            var prediction = predEngine.Predict(new SpamInput { Message = "you win pills and free entry vouchers" });

            Console.WriteLine($"Accuracy:{calMetrics.Accuracy}-Auc:{calMetrics.Auc}-Prediction:{prediction.IsSpam}");
        }
    }
}
