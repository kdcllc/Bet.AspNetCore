using System.Threading.Tasks;

using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.ReCapture.Google
{
    /// <summary>
    /// Adds Google ReCapture script tag only if recapture element is present.
    /// </summary>
    [HtmlTargetElement("body")]
    public class ReCaptureBodyTagHelper : TagHelper
    {
        private readonly IOptionsMonitor<GoogleReCaptchaOptions> _options;

        public ReCaptureBodyTagHelper(IOptionsMonitor<GoogleReCaptchaOptions> options)
        {
            _options = options;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var childContent = output.Content.IsModified ? output.Content.GetContent() :
                (await output.GetChildContentAsync()).GetContent();

            if (childContent.Contains(Constants.BodyElementFilter))
            {
                var script = _options.CurrentValue?.Script ?? Constants.Script;

                if (!string.IsNullOrEmpty(script))
                {
                    output.PostElement.AppendHtml(script);
                }
            }
        }
    }
}
