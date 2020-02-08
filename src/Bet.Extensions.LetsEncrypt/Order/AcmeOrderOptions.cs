using Bet.Extensions.LetsEncrypt.Order.Stores;

namespace Bet.Extensions.LetsEncrypt.Order
{
    public class AcmeOrderOptions
    {
        /// <summary>
        /// Gets or sets the <see cref="Certes.KeyAlgorithm"/> used to request a new LetsEncrypt certificate.
        /// </summary>
        public KeyAlgorithm KeyAlgorithm { get; set; } = KeyAlgorithm.ES256;

        /// <summary>
        /// Required. Sent to LetsEncrypt to let them know what details you want in your certificate. Some of the properties are optional.
        /// </summary>
        public Certes.CsrInfo? CertificateSigningRequest { get; set; }

        /// <summary>
        /// Acme Validation Delay in seconds. The default value is 2.
        /// </summary>
        public int ValidationDelay { get; set; } = 2;

        /// <summary>
        /// Acme Validation Retry Count. The default value is 60 seconds.
        /// </summary>
        public int ValidationRetry { get; set; } = 60;

        internal IAcmeChallengeStore? ChallengesStore { get; set; }
    }
}
