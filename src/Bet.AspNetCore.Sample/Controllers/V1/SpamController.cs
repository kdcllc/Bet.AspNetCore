using System;

using Bet.Extensions.ML.Prediction;
using Bet.Extensions.ML.Spam.Models;

using Microsoft.AspNetCore.Mvc;

namespace Bet.AspNetCore.Sample.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("/v{version:apiVersion}/[controller]")]
    public class SpamController : ControllerBase
    {
        private readonly IModelPredictionEngine<SpamInput, SpamPrediction> _spamModel;

        public SpamController(
            IModelPredictionEngine<SpamInput, SpamPrediction> spamModel)
        {
            _spamModel = spamModel ?? throw new ArgumentNullException(nameof(spamModel));
        }

        [HttpPost]
        public ActionResult<SpamPrediction> Predict(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                text = "free medicine winner! congratulations";
            }

            return _spamModel.Predict(MLModels.SpamModel, new SpamInput { Message = text });
        }
    }
}
