using System;

using Certes;
using Certes.Acme;

namespace Bet.AspNetCore.LetsEncrypt.Options
{
    public class LetsEncryptOptions
    {
        private string[] _hostNames = Array.Empty<string>();

        /// <summary>
        /// The domain names for which to generate certificates.
        /// </summary>
        public string[] HostNames
        {
            get => _hostNames;
            set => _hostNames = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Request a new SSL Certificate before.
        /// </summary>
        public int DaysBefore { get; set; } = 15;

        /// <summary>
        /// The email address used to register with letsencrypt.org.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Required. Sent to LetsEncrypt to let them know what details you want in your certificate. Some of the properties are optional.
        /// </summary>
        public CsrInfo CertificateSigningRequest { get; set; }

        /// <summary>
        /// The name is used to generate .pfx certificate file.
        /// </summary>
        public string CertificateFriendlyName { get; set; }

        /// <summary>
        /// The password is used to generate .pfx certificate file.
        /// </summary>
        public string CertificatePassword { get; set; }

        /// <summary>
        /// Use Let's Encrypt staging server.
        /// <para>
        /// This is recommended during development of the application.
        /// </para>
        /// </summary>
        public bool UseStagingServer
        {
            get => AcmeServer == WellKnownServers.LetsEncryptStagingV2;
            set => AcmeServer = value
                    ? WellKnownServers.LetsEncryptStagingV2
                    : WellKnownServers.LetsEncryptV2;
        }

        /// <summary>
        /// The uri to the server that implements thE ACME protocol for certificate generation.
        /// </summary>
        internal Uri AcmeServer { get; set; }

        internal bool IsDevelopment { get; set; } = true;
    }
}
