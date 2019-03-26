using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;

namespace Bet.AspNetCore.Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionController : ControllerBase
    {
        private readonly MLContext _mlContext;

        public PredictionController()
        {
           _mlContext = new MLContext();
        }

        // GET /api/predictor/spam?text=Hello World
        [HttpGet]
        [Route("spam")]
        public ActionResult<string> PredictSpam([FromQuery]string text)
        {
            return text;
        }

    }
}
