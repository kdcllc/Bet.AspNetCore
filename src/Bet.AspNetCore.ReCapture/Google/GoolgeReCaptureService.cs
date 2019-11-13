using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Newtonsoft.Json.Linq;

namespace Bet.AspNetCore.ReCapture.Google
{
    /// <summary>
    /// ReCapture Verification service.
    /// </summary>
    public class GoolgeReCaptureService
    {
        private readonly HttpClient _client;
        private readonly IOptionsMonitor<GoogleReCaptchaOptions> _options;

        public GoolgeReCaptureService(
            HttpClient client,
            IOptionsMonitor<GoogleReCaptchaOptions> options)
        {
            _client = client;
            _options = options;
        }

        /// <summary>
        /// Validates Secret and response.
        /// </summary>
        /// <param name="reCaptureResponse"></param>
        /// <returns></returns>
        public async Task<string> Validate(string reCaptureResponse)
        {
            _client.BaseAddress = new Uri(Constants.SiteVerifyUrl);

            var settings = _options.CurrentValue;
            var message = !string.IsNullOrEmpty(settings.ValidationMessage) ? settings.ValidationMessage : Constants.ValidationMessage;

            var response = await _client.GetAsync($"siteverify?secret={settings.SecretKey}&response={reCaptureResponse}");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return message;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            dynamic jsonData = JObject.Parse(jsonResponse);
            if (jsonData.success != "true")
            {
                return message;
            }

            return string.Empty;
        }
    }
}
