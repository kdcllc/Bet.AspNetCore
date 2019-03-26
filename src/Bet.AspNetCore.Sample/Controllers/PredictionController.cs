using Bet.AspNetCore.Sample.Models;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bet.AspNetCore.Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictorController : ControllerBase
    {
        private readonly MLContext _mlContext;

        public PredictorController()
        {
           _mlContext = new MLContext();
        }

        // GET /api/predictor/spam?text=Hello World
        [HttpGet]
        [Route("spam")]
        public ActionResult<string> PredictSpam([FromQuery]string text)
        {
            var assembly = typeof(Startup).GetTypeInfo().Assembly;
            var resource = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.App_Data.SpamDetectionData.csv");

            var reader = new StreamReader(resource,true);

            var data = new CsvReader(reader);

            data.Configuration.HasHeaderRecord = true;
            data.Configuration.Delimiter = ",";
            data.Configuration.BadDataFound = null;
            data.Configuration.HeaderValidated = null;
            data.Configuration.MissingFieldFound = null;

            var records = data.GetRecords<Message>().ToArray();

            foreach (var record in records)
            {
                switch (record.TextLabel)
                {
                    case "Spam":
                        record.Label = true;
                        break;
                    case "Ham":
                        record.Label = false;
                        break;
                    default:
                        record.Label = false;
                        break;
                }
            }
            var dataView = _mlContext.Data.LoadFromEnumerable<Message>(records);

            var testTrainSplit = _mlContext.BinaryClassification.TrainTestSplit(dataView, testFraction: 0.2);

            var dataProcessPipeline = _mlContext.Transforms.Text.FeaturizeText(
                outputColumnName: DefaultColumnNames.Features,
                inputColumnName: nameof(Message.Text))
                //.Conversion.MapValueToKey(outputColumnName: DefaultColumnNames.Label, inputColumnName: nameof(Message.Label))
                //.Append(_mlContext.Transforms.Text.FeaturizeText(outputColumnName: DefaultColumnNames.Features, inputColumnName: nameof(Message.Text)))
                .AppendCacheCheckpoint(_mlContext);

            var trainer = _mlContext.BinaryClassification.Trainers.FastTree(
                labelColumnName: DefaultColumnNames.Label,
                featureColumnName: DefaultColumnNames.Features);

            var trainingPipeline = dataProcessPipeline.Append(trainer);

            var trainedModel = trainingPipeline.Fit(testTrainSplit.TrainSet);

            var predictions = trainedModel.Transform(testTrainSplit.TestSet);
            var metrics = _mlContext.BinaryClassification.Evaluate(data: predictions, label: DefaultColumnNames.Label, score: DefaultColumnNames.Score);

            var predEngine = trainedModel.CreatePredictionEngine<Message, SpamPrediction>(_mlContext);
            var prediction = predEngine.Predict(new Message { Text = text });


            return $"{metrics.Accuracy}-{metrics.Auc}-{prediction.isSpam}";
        }

        class SpamPrediction
        {
            [ColumnName("PredictedLabel")]
            public bool isSpam { get; set; }

            public float Score { get; set; }
            public float Probability { get; set; }
        }
    }
}
