using System;

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
        public ActionResult<SentimentPrediction> GetSentiment(SentimentObservation input)
        {
            return _sentimentModel.Predict(input);
        }

        // GET /api/prediction/spam?text=Hello World
        [HttpGet]
        [Route("spam")]
        public ActionResult<SpamPrediction> PredictSpam([FromQuery]string text)
        {
            return _spamModel.Predict(new SpamInput { Message = text });
        }
    }
}
