using System;
using System.Threading;
using System.Threading.Tasks;

using Bet.AspNetCore.Jwt.Services;
using Bet.AspNetCore.Jwt.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bet.AspNetCore.Jwt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JwtTokenController : ControllerBase
    {
        private readonly IAuthenticateService _authService;

        public JwtTokenController(IAuthenticateService userService)
        {
            _authService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("/token")]
        public async Task<IActionResult> RequestToken(
            [FromBody] TokenRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.IsAuthenticatedAsync(request, cancellationToken);

            if (result.Success)
            {
                return Ok(new { result.Token });
            }

            return BadRequest("Invalid Request");
        }
    }
}
