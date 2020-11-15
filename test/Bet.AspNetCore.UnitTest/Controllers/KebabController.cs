using Microsoft.AspNetCore.Mvc;

namespace Bet.AspNetCore.UnitTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KebabController : ControllerBase
    {
        [HttpGet]
        [Route("test-route")]
        public ActionResult TestBadRequestException(int param)
        {
            return Ok("success");
        }
    }
}
