using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.ReCapture.Google
{
    /// <summary>
    /// Adds ReCapture div element to the web page.
    /// </summary>
    [HtmlTargetElement("google-recaptcha")]
    public class ReCaptchaTagHelper : TagHelper
    {
        [HtmlAttributeName("key")]
        public ModelExpression Key { get; set; }

        private readonly IOptionsMonitor<GoogleReCaptchaOptions> _options;

        public ReCaptchaTagHelper(IOptionsMonitor<GoogleReCaptchaOptions> options)
        {
            _options = options;
        }

        /// <summary>
        /// Generates html syntax:
        /// <![CDATA[
        /// <div class="g-recaptcha" data-sitekey="ReCaptchaKey"></div>
        /// ]]>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";

            string key = null;
            if (Key != null)
            {
                key = Key.Model?.ToString();
            }
            else
            {
                key = _options.CurrentValue.ClientKey;
            }
            output.Attributes.Add("class", "g-recaptcha");
            output.Attributes.Add("data-sitekey", key);
        }
    }
}
