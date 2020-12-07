using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Bet.AspNetCore.Sample.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Swashbuckle.AspNetCore.Annotations;

namespace Bet.AspNetCore.Sample.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    //[ApiConventionType(typeof(DefaultApiConventions))]
    [Route("v{version:apiVersion}/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [SwaggerOperation(
        Summary = "Get Weather Forecast",
        Description = "Get Weather Forecast for your region",
        OperationId = "Get",
        Tags = new[] { "Controllers" })
        ]
        //[ApiConventionMethod(
        //    typeof(DefaultApiConventions),
        //    nameof(DefaultApiConventions.Get))]
        public IEnumerable<WeatherForecast> Get(CancellationToken cancellationToken)
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
