namespace Bet.AspNetCore.ReCapture.Google
{
    public static class Constants
    {
        public static readonly string Script = "<script src='https://www.google.com/recaptcha/api.js'></script>";

        public static readonly string BodyElementFilter = "recaptcha";

        public static readonly string FormElementResponse = "g-recaptcha-response";

        public static readonly string ValidationMessage = "Google reCAPTCHA validation failed";

        public static readonly string SiteVerifyUrl = "https://www.google.com/recaptcha/api/";
    }
}
