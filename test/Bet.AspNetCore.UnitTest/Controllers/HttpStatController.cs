using Microsoft.AspNetCore.Mvc;

namespace Bet.AspNetCore.UnitTest.Controllers
{
    [Route("api/[controller]")]
    public class HttpStatController : ControllerBase
    {
        [HttpGet("{code}")]
        public IActionResult Get(int code)
        {
            return StatusCode(code);
        }
    }
}
