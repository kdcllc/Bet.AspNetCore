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
            [FromBody] AuthorizeTokenRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _authService.GetTokenAsync(request, cancellationToken);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest("Invalid Request");
        }

        [HttpPost]
        [Route("/refresh")]
        public async Task<IActionResult> RefreshToken(
            [FromBody] RefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _authService.RefreshTokenAsync(request, cancellationToken);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest("Invalid client request");
        }

        [HttpPost]
        [Authorize]
        [Route("/revoke")]
        public async Task<IActionResult> Revoke(CancellationToken cancellationToken)
        {
            var username = User.Identity.Name;

            if (await _authService.RevokeAsync(username, cancellationToken))
            {
                return NoContent();
            }

            return BadRequest();
        }

        private string IpAddress()
        {
            return Request.Headers.ContainsKey("X-Forwarded-For")
                ? (string)Request.Headers["X-Forwarded-For"]
                : HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
