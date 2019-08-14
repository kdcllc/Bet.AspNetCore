using System;
using System.Threading.Tasks;

using Bet.AspNetCore.ReCapture.Google;

using Microsoft.AspNetCore.Mvc.Filters;

namespace Bet.AspNetCore.ReCapture
{
    /// <summary>
    /// Provides Model validation for ReCapture Response.
    /// </summary>
    public class GoogleReCaptureFilter : ActionFilterAttribute
    {
        private readonly GoolgeReCaptureService _service;

        public GoogleReCaptureFilter(GoolgeReCaptureService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var response = context.HttpContext.Request.Form[Constants.FormElementResponse];
            var result = await _service.Validate(response);

            if (!string.IsNullOrWhiteSpace(result))
            {
                context.ModelState.AddModelError(string.Empty, result);
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
