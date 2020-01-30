using System;
using System.Collections.Generic;
using System.Linq;

using Bet.Extensions.ML.Prediction;
using Bet.Extensions.ML.Sentiment.Models;

using Microsoft.AspNetCore.Mvc;

namespace Bet.AspNetCore.Sample.Controllers
{
    [Route("/v{api-version:apiVersion}/[controller]")]
    [ApiController]
    public class SentimentController : ControllerBase
    {
        private readonly IModelPredictionEngine<SentimentIssue, SentimentPrediction> _sentimentModel;

        public SentimentController(
            IModelPredictionEngine<SentimentIssue, SentimentPrediction> sentimentModel)
        {
            _sentimentModel = sentimentModel ?? throw new ArgumentNullException(nameof(sentimentModel));
        }

        /// <summary>
        /// Predicts batch values.
        /// </summary>
        /// <param name="samples"></param>
         /// <returns></returns>
        [HttpPost]
        public ActionResult<List<SentimentPrediction>> Predict(IEnumerable<SentimentIssue> samples)
        {
            var input = samples.ToList();

            if (samples.Count() == 1)
            {
                var result = new List<SentimentPrediction>();

                result.Add(_sentimentModel.Predict(MLModels.SentimentModel, input[0]));
                return result;
            }

            if (!samples.Any())
            {
                var extra = new List<SentimentIssue>
                {
                    new SentimentIssue { Text = "This is a very rude movie" },
                    new SentimentIssue { Text = "Hate All Of You're Work" },
                };
                input.AddRange(extra);
            }

            var results = _sentimentModel.Predict(MLModels.SentimentModel, input);
            return Ok(results);
        }
    }
}
