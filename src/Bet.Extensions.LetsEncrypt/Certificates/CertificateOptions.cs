using System;

namespace Bet.Extensions.LetsEncrypt.Certificates
{
    public class CertificateOptions
    {
        /// <summary>
        /// The amount of time before the expiry date of the certificate that a new one is created. Defaults to 30 days.
        /// </summary>
        public TimeSpan? TimeUntilExpiryBeforeRenewal { get; set; } = TimeSpan.FromDays(30);

        /// <summary>
        /// The amount of time after the last renewal date that a new one is created. Defaults to null.
        /// </summary>
        public TimeSpan? TimeAfterIssueDateBeforeRenewal { get; set; } = null;

        /// <summary>
        /// The password is used to generate .pfx certificate file.
        /// </summary>
        public string? CertificatePassword { get; set; }
    }
}
