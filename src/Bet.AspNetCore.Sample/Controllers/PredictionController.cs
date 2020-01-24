using System;
using System.Collections.Generic;
using Bet.AspNetCore.Sample.Models;
using Bet.Extensions.ML.Prediction;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.AspNetCore.Mvc;

namespace Bet.AspNetCore.Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionController : ControllerBase
    {
        private readonly IModelPredictionEngine<SentimentObservation, SentimentPrediction> _sentimentModel;
        private readonly IModelPredictionEngine<SpamInput, SpamPrediction> _spamModel;

        public PredictionController(
            IModelPredictionEngine<SentimentObservation, SentimentPrediction> sentimentModel,
            IModelPredictionEngine<SpamInput, SpamPrediction> spamModel)
        {
            _sentimentModel = sentimentModel ?? throw new ArgumentNullException(nameof(sentimentModel));
            _spamModel = spamModel ?? throw new ArgumentNullException(nameof(spamModel));
        }

        [HttpPost]
        public ActionResult<SentimentPrediction> Sentiment(SentimentObservation input)
        {
            return _sentimentModel.Predict(MLModels.SentimentModel, input);
        }

        [HttpGet]
        [Route("batchsentiment")]
        public IActionResult BatchSentiment([FromQuery]SentimentObservation[] samples)
        {
            var input = new List<SentimentObservation>
            {
                new SentimentObservation { SentimentText = "This is a very rude movie" },
                new SentimentObservation { SentimentText = "Hate All Of You're Work" },
            };

            if (samples != null)
            {
                input.AddRange(samples);
            }

            var results = _sentimentModel.Predict(MLModels.SentimentModel, input);
            return Ok(results);
        }

        // GET /api/prediction/spam?text=Hello World
        [HttpGet]
        [Route(nameof(Spam))]
        public ActionResult<SpamPrediction> Spam([FromQuery]string text)
        {
            return _spamModel.Predict(MLModels.SpamModel, new SpamInput { Message = text });
        }
    }
}
