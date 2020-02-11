using System;
using System.Collections.Generic;

using Bet.Extensions.LetsEncrypt.Account.Stores;

using Certes.Acme;

namespace Bet.Extensions.LetsEncrypt.Account
{
    public class AcmeAccountOptions
    {
        public List<string> Domains { get; set; } = new List<string>();

        /// <summary>
        /// Used only for LetsEncrypt to contact you when the domain is about to expire - not actually validated.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Recommended while testing - increases your rate limit towards LetsEncrypt. Defaults to false.
        /// </summary>
        public bool UseStaging { get; set; }

        /// <summary>
        /// Gets the uri which will be used to talk to LetsEncrypt servers.
        /// </summary>
        public Uri LetsEncryptUri => UseStaging
            ? WellKnownServers.LetsEncryptStagingV2
            : WellKnownServers.LetsEncryptV2;

        internal IAcmeAccountStore? AccountStore { get; set; }
    }
}
