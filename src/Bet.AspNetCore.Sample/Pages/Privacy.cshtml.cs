using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bet.AspNetCore.Sample.Pages
{
    public class PrivacyModel : PageModel
    {
        public void OnGet()
        {
        }

        /// <summary>
        /// https://www.joeaudette.com/blog/2018/08/28/gdpr---adding-a-revoke-consent-button-in-aspnet-core.
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPostWithdraw()
        {
            HttpContext.Features.Get<ITrackingConsentFeature>().WithdrawConsent();
            return RedirectToPage("./Index");
        }
    }
}
