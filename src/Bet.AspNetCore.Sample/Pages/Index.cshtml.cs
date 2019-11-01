using System;
using System.Threading.Tasks;

using Bet.AspNetCore.Sample.Options;
using Bet.Extensions.AzureStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bet.AspNetCore.Sample.Pages
{
    public class IndexModel : PageModel
    {
        private const string CookieName = "TestCookie";

        private readonly IDataProtector _dataProtector;

        public IndexModel(IDataProtectionProvider dataProtectionProvider)
        {
            _dataProtector = dataProtectionProvider.CreateProtector("Test");
        }

        public string CookieValue { get; set; }

        public bool ShowCookieValue => !string.IsNullOrEmpty(CookieValue);

        public void OnGet()
        {
            if (!Request.Cookies.TryGetValue(CookieName, out var cookieValue))
            {
                var valueToSetInCookie = $"Some text set in cookie at {DateTime.Now.ToString()}";
                var encryptedValue = _dataProtector.Protect(valueToSetInCookie);
                Response.Cookies.Append(CookieName, encryptedValue, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    IsEssential = true
                });
                return;
            }

            CookieValue = _dataProtector.Unprotect(cookieValue);
        }
    }
}
